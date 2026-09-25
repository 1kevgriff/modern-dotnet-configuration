using Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

// d19 — Labels are the environment axis.
//
// One store holds every environment. Select the unlabelled defaults first, then select
// the same key pattern again with the environment's label: the second Select overlays
// the first, key by key. Keys the environment doesn't override simply inherit.

var endpoint = Environment.GetEnvironmentVariable("APPCONFIG_ENDPOINT");
var label = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

if (string.IsNullOrWhiteSpace(endpoint))
{
    Console.WriteLine();
    Console.WriteLine("  This demo needs an Azure App Configuration store:");
    Console.WriteLine();
    Console.WriteLine("      $env:APPCONFIG_ENDPOINT = \"https://<store>.azconfig.io\"");
    Console.WriteLine("      $env:DOTNET_ENVIRONMENT = \"Production\"   # the label to overlay");
    Console.WriteLine("      az login");
    Console.WriteLine("      dotnet run");
    Console.WriteLine();
    Console.WriteLine("  Seed keys are listed in README.md.");
    Console.WriteLine();

    // Not configured is not a failure: verify.ps1 runs every demo unattended.
    return 0;
}

var storeUri = new Uri(endpoint);
var credential = new DefaultAzureCredential();

Console.WriteLine();
Console.WriteLine($"  STORE    {storeUri}");
Console.WriteLine($"  SELECT   Weather:*  with no label, then again with label \"{label}\"");
Console.WriteLine();

IConfigurationRoot defaults;
IConfigurationRoot labelled;
IConfigurationRoot effective;

try
{
    // The unlabelled slice on its own — the defaults every environment starts from.
    defaults = Load(options => options.Select("Weather:*", LabelFilter.Null));

    // The environment's slice on its own — only what this environment overrides.
    labelled = Load(options => options.Select("Weather:*", label));

    // What the app actually gets: defaults first, environment overlaid on top.
    effective = Load(options => options
        .Select("Weather:*", LabelFilter.Null)
        .Select("Weather:*", label));
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

var keys = defaults.AsEnumerable()
    .Concat(labelled.AsEnumerable())
    .Where(pair => pair.Value is not null)
    .Select(pair => pair.Key)
    .Distinct(StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToList();

foreach (var key in keys)
{
    var defaultValue = defaults[key];
    var labelledValue = labelled[key];
    var winner = effective[key];

    Console.WriteLine($"  {key}");
    Console.WriteLine($"     {Marker(defaultValue, winner)} (no label)   {Show(defaultValue)}");
    Console.WriteLine($"     {Marker(labelledValue, winner)} {label,-11}  {Show(labelledValue)}");
    Console.WriteLine();
}

Console.WriteLine($"  {keys.Count} keys, one store, {label} overlaid on the defaults");
Console.WriteLine();

return 0;

IConfigurationRoot Load(Action<AzureAppConfigurationOptions> select) =>
    new ConfigurationBuilder()
        .AddAzureAppConfiguration(options =>
        {
            options.Connect(storeUri, credential);
            select(options);
        })
        .Build();

// "->" marks the value the app actually sees for this key.
static string Marker(string? candidate, string? winner) =>
    candidate is not null && string.Equals(candidate, winner, StringComparison.Ordinal) ? "->" : "  ";

static string Show(string? value) => value ?? "(not set)";

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
