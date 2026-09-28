using Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

// d22 — App Configuration stores a *pointer* to a secret; ConfigureKeyVault dereferences it.
//
// A Key Vault reference is an ordinary key/value in the store whose value is a Key Vault
// URI and whose content type marks it as a reference. The app never names Key Vault and
// never holds a SecretClient — it reads a configuration key and gets a secret.
//
// To make that visible, the store is read twice: once with a resolver that hands back the
// pointer instead of following it, and once with a real credential that follows it.

var endpoint = Environment.GetEnvironmentVariable("APPCONFIG_ENDPOINT");

if (string.IsNullOrWhiteSpace(endpoint))
{
    Console.WriteLine();
    Console.WriteLine("  This demo needs an App Configuration store and a Key Vault:");
    Console.WriteLine();
    Console.WriteLine("      $env:APPCONFIG_ENDPOINT = \"https://<store>.azconfig.io\"");
    Console.WriteLine("      az login");
    Console.WriteLine("      dotnet run");
    Console.WriteLine();
    Console.WriteLine("  The store needs one Key Vault reference. See README.md.");
    Console.WriteLine();

    // Not configured is not a failure: verify.ps1 runs every demo unattended.
    return 0;
}

var storeUri = new Uri(endpoint);
var credential = new DefaultAzureCredential();

Console.WriteLine();
Console.WriteLine($"  STORE    {storeUri}");
Console.WriteLine("  SELECT   Weather:*");
Console.WriteLine();

IConfigurationRoot stored;
IConfigurationRoot resolved;

try
{
    // Hand the pointer straight back instead of following it, so the room can see it.
    stored = Load(keyVault => keyVault.SetSecretResolver(
        uri => new ValueTask<string>(uri.ToString())));

    // What a real app does: follow the pointer with a credential — and give the
    // dereferenced secret its own refresh cadence, or a rotated secret is cached for
    // the life of the process.
    resolved = Load(keyVault => keyVault
        .SetCredential(credential)
        .SetSecretRefreshInterval(TimeSpan.FromMinutes(30)));
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

var keys = stored.AsEnumerable()
    .Where(pair => pair.Value is not null)
    .Select(pair => pair.Key)
    .Order(StringComparer.Ordinal)
    .ToList();

var references = 0;

foreach (var key in keys)
{
    var pointer = stored[key];
    var value = resolved[key];

    // If following the pointer changed the answer, the store held a reference, not a value.
    var isReference = !string.Equals(pointer, value, StringComparison.Ordinal);

    if (isReference)
    {
        references++;
    }

    Console.WriteLine($"  {key}   {(isReference ? "-- Key Vault reference" : "-- plain value")}");
    Console.WriteLine($"        in the store   {pointer}");
    Console.WriteLine($"        in the app     {(isReference ? Redact(value) : value)}");
    Console.WriteLine();
}

Console.WriteLine($"  {references} of {keys.Count} keys came from Key Vault. The app never said so.");
Console.WriteLine();

return 0;

IConfigurationRoot Load(Action<AzureAppConfigurationKeyVaultOptions> configureKeyVault) =>
    new ConfigurationBuilder()
        .AddAzureAppConfiguration(options => options
            .Connect(storeUri, credential)
            .Select("Weather:*")
            .ConfigureKeyVault(configureKeyVault))
        .Build();

// Values on a conference projector are values in the audience's phone camera roll.
static string Redact(string? value) =>
    string.IsNullOrEmpty(value)
        ? "(empty)"
        : $"{value[..Math.Min(3, value.Length)]}{new string('*', 8)}  ({value.Length} chars)";

static int Fail(Exception exception)
{
    var root = exception is AggregateException aggregate
        ? aggregate.Flatten().InnerExceptions[0]
        : exception;

    Console.WriteLine($"  Could not read the store:  {Summarize(root)}");
    Console.WriteLine();
    Console.WriteLine("  Check:  az login  |  'App Configuration Data Reader'  |  'Key Vault Secrets User'");
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
