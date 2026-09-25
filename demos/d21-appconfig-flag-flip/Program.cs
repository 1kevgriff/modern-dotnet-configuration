using Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

// d21 — Flip a flag in the portal; the running app changes its mind. No redeploy.
//
// d28 already showed that a feature flag is just configuration, with no cloud involved.
// This is the same flag, moved into App Configuration, so the value can change while the
// process is running. UseFeatureFlags registers itself for refresh — unlike ordinary keys,
// flags need no explicit ConfigureRefresh — but something still has to ask for the refresh.

const string FlagName = "Beta";

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
    Console.WriteLine($"  The store needs one feature flag named \"{FlagName}\". See README.md.");
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
                   .UseFeatureFlags(flags => flags.SetRefreshInterval(TimeSpan.FromSeconds(5)));

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

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddFeatureManagement();

await using var provider = services.BuildServiceProvider();
var features = provider.GetRequiredService<IVariantFeatureManager>();

using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

Console.WriteLine();
Console.WriteLine($"  STORE   {storeUri}");
Console.WriteLine($"  FLAG    {FlagName}   refreshing every 5s for {seconds}s   (Ctrl+C to stop)");
Console.WriteLine();

var enabled = await features.IsEnabledAsync(FlagName, cancellation.Token);
var flips = 0;
Report(enabled, flips);

Console.WriteLine("  Now toggle the flag in the portal. The process keeps running.");
Console.WriteLine();

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2), timeProvider);

try
{
    while (await timer.WaitForNextTickAsync(cancellation.Token))
    {
        await refresher.TryRefreshAsync(cancellation.Token);

        var latest = await features.IsEnabledAsync(FlagName, cancellation.Token);

        if (latest != enabled)
        {
            enabled = latest;
            Report(enabled, ++flips);
        }
    }
}
catch (OperationCanceledException)
{
    // Ctrl+C or the time limit. Either way the demo is over.
}

Console.WriteLine($"  {flips} flip(s), 0 deployments.");
Console.WriteLine();

return 0;

void Report(bool isEnabled, int flipCount)
{
    var state = isEnabled ? "ON " : "OFF";
    var note = flipCount == 0 ? string.Empty : "   <-- flipped in the portal";

    Console.WriteLine($"  {timeProvider.GetLocalNow():HH:mm:ss}   {FlagName} = {state}{note}");
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
