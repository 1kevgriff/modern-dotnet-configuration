using Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

// d20 — The sentinel key makes a multi-key change land atomically.
//
// RegisterAll() re-reads the whole slice whenever anything in it changes, so an operator
// editing three keys in the portal can be observed a third of the way through. Register a
// single sentinel key instead and bump it *after* every other edit lands: the app sees
// nothing until the sentinel moves, then sees all three changes at once.

const string SentinelKey = "Weather:Sentinel";

var endpoint = Environment.GetEnvironmentVariable("APPCONFIG_ENDPOINT");
var seconds = args.Length > 0 && int.TryParse(args[0], out var parsed) ? parsed : 180;

if (string.IsNullOrWhiteSpace(endpoint))
{
    Console.WriteLine();
    Console.WriteLine("  This demo needs an Azure App Configuration store:");
    Console.WriteLine();
    Console.WriteLine("      $env:APPCONFIG_ENDPOINT = \"https://<store>.azconfig.io\"");
    Console.WriteLine("      az login");
    Console.WriteLine("      dotnet run              # watches for 180s");
    Console.WriteLine("      dotnet run -- 60        # or watch for 60s");
    Console.WriteLine();
    Console.WriteLine("  Seed keys are listed in README.md.");
    Console.WriteLine();

    // Not configured is not a failure: verify.ps1 runs every demo unattended.
    return 0;
}

var storeUri = new Uri(endpoint);
var timeProvider = TimeProvider.System;

IConfigurationRefresher? capturedRefresher = null;
IConfigurationRoot configuration;

try
{
    configuration = new ConfigurationBuilder()
        .AddAzureAppConfiguration(options =>
        {
            options.Connect(storeUri, new DefaultAzureCredential())
                   .Select("Weather:*")
                   .ConfigureRefresh(refresh => refresh
                       .Register(SentinelKey, refreshAll: true)
                       .SetRefreshInterval(TimeSpan.FromSeconds(5)));

            capturedRefresher = options.GetRefresher();
        })
        .Build();
}
catch (AuthenticationFailedException ex)
{
    return Fail(ex);
}
catch (RequestFailedException ex)
{
    return Fail(ex);
}
catch (AggregateException ex)
{
    return Fail(ex);
}

var refresher = capturedRefresher
    ?? throw new InvalidOperationException("The App Configuration refresher was not created.");

Console.WriteLine();
Console.WriteLine($"  STORE      {storeUri}");
Console.WriteLine($"  WATCHING   Weather:*   through the sentinel {SentinelKey}");
Console.WriteLine($"  POLLING    every 5s for {seconds}s   (Ctrl+C to stop)");
Console.WriteLine();

var snapshot = Snapshot();
var revision = 1;
Print(revision, snapshot);

Console.WriteLine("  Now edit Endpoint, Retries and TimeoutSeconds in the portal.");
Console.WriteLine("  Nothing will happen here. Then bump the sentinel.");
Console.WriteLine();

using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2), timeProvider);

try
{
    while (await timer.WaitForNextTickAsync(cancellation.Token))
    {
        // Activity-driven: nothing refreshes unless something asks. A web app asks from
        // middleware on each request; a console app or worker has to ask for itself.
        await refresher.TryRefreshAsync(cancellation.Token);

        var latest = Snapshot();

        if (!latest.SequenceEqual(snapshot))
        {
            snapshot = latest;
            Print(++revision, snapshot);
        }
    }
}
catch (OperationCanceledException)
{
    // Ctrl+C or the time limit. Either way the demo is over.
}

Console.WriteLine($"  {revision} revision(s) observed.");
Console.WriteLine();

return 0;

List<KeyValuePair<string, string?>> Snapshot() =>
    configuration.GetSection("Weather")
        .GetChildren()
        .Where(child => !string.Equals(child.Key, "Sentinel", StringComparison.Ordinal))
        .Select(child => new KeyValuePair<string, string?>(child.Key, child.Value))
        .OrderBy(pair => pair.Key, StringComparer.Ordinal)
        .ToList();

void Print(int index, List<KeyValuePair<string, string?>> values)
{
    var sentinel = configuration[SentinelKey] ?? "(unset)";

    Console.WriteLine($"  #{index}   {timeProvider.GetLocalNow():HH:mm:ss}   sentinel = {sentinel}");

    foreach (var (key, value) in values)
    {
        Console.WriteLine($"         Weather:{key,-16} {value}");
    }

    Console.WriteLine();
}

static int Fail(Exception exception)
{
    var root = exception is AggregateException aggregate
        ? aggregate.Flatten().InnerExceptions[0]
        : exception;

    Console.WriteLine($"  Could not read the store:  {Summarize(root)}");
    Console.WriteLine();
    Console.WriteLine("  Check:  az login  |  the endpoint  |  the 'App Configuration Data Reader' role");
    Console.WriteLine();

    return 1;
}

// Azure SDK failure messages run to several hundred characters and wrap into a wall of
// text on a projector. One clause is all the room needs.
static string Summarize(Exception exception)
{
    var clause = exception.Message.Split(Environment.NewLine)[0].Split(". ")[0].Trim();

    return clause.Length <= 90 ? clause : $"{clause[..90]}...";
}
