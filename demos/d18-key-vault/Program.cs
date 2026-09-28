using Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

// d18 — Key Vault secrets become configuration keys, and "--" becomes ":".
//
// Key Vault secret names may not contain ":", so the default KeyVaultSecretManager
// maps "--" onto the configuration delimiter. A secret named Weather--ApiKey lands
// in configuration as Weather:ApiKey and binds like any other key.

var vaultName = Environment.GetEnvironmentVariable("KEYVAULT_NAME");

if (string.IsNullOrWhiteSpace(vaultName))
{
    Console.WriteLine();
    Console.WriteLine("  This demo needs a Key Vault. Set the vault name and run again:");
    Console.WriteLine();
    Console.WriteLine("      $env:KEYVAULT_NAME = \"my-vault\"");
    Console.WriteLine("      az login");
    Console.WriteLine("      dotnet run");
    Console.WriteLine();
    Console.WriteLine("  Expected secrets: Weather--ApiKey, Weather--Endpoint, Weather--TimeoutSeconds");
    Console.WriteLine();

    // Not configured is not a failure: verify.ps1 runs every demo unattended.
    return 0;
}

var vaultUri = new Uri($"https://{vaultName}.vault.azure.net/");

Console.WriteLine();
Console.WriteLine($"  VAULT        {vaultUri}");
Console.WriteLine("  CREDENTIAL   DefaultAzureCredential");
Console.WriteLine();

IConfigurationRoot configuration;

try
{
    configuration = new ConfigurationBuilder()
        .AddAzureKeyVault(vaultUri, new DefaultAzureCredential())
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
    // The provider loads synchronously and surfaces transport failures wrapped.
    return Fail(ex);
}

var keys = configuration.AsEnumerable()
    .Where(pair => pair.Value is not null)
    .Select(pair => pair.Key)
    .Order(StringComparer.Ordinal)
    .ToList();

foreach (var key in keys)
{
    // The transform is deterministic, so the secret name is recoverable from the key.
    var secretName = key.Replace(":", "--", StringComparison.Ordinal);

    Console.WriteLine($"  {secretName}");
    Console.WriteLine($"      ->  {key}  =  {Redact(configuration[key])}");
    Console.WriteLine();
}

var provider = configuration.Providers.Single().GetType().Name;

Console.WriteLine($"  every key above came from   {provider}");
Console.WriteLine($"  {keys.Count} secrets loaded — none in the repo, none on the projector");
Console.WriteLine();

return 0;

// Values on a conference projector are values in the audience's phone camera roll.
static string Redact(string? value) =>
    string.IsNullOrEmpty(value)
        ? "(empty)"
        : $"{value[..Math.Min(3, value.Length)]}{new string('*', 8)}  ({value.Length} chars)";

// A stack trace on stage tells the room nothing. One line and a checklist does.
static int Fail(Exception exception)
{
    var root = exception is AggregateException aggregate
        ? aggregate.Flatten().InnerExceptions[0]
        : exception;

    Console.WriteLine($"  Could not read the vault:  {Summarize(root)}");
    Console.WriteLine();
    Console.WriteLine("  Check:  az login  |  the vault name  |  the 'Key Vault Secrets User' role");
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
