using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

// d09 — user secrets.
//
//   The default builders register the user-secrets provider ONLY in Development,
//   and only when the assembly carries a UserSecretsId. It sits above the JSON
//   files and below environment variables and the command line.
//
//   It is not encrypted. It is not a vault. It keeps secrets out of the repo.

var builder = Host.CreateApplicationBuilder(args);
var config = (IConfigurationRoot)builder.Configuration;

// The user-secrets provider is a JSON provider whose file is called secrets.json.
var secrets = config.Providers
    .OfType<JsonConfigurationProvider>()
    .FirstOrDefault(p => string.Equals(p.Source.Path, "secrets.json", StringComparison.Ordinal));

Console.WriteLine();
Console.WriteLine("  d09 — user secrets");
Console.WriteLine();
Console.WriteLine($"  Environment            {builder.Environment.EnvironmentName}");
Console.WriteLine($"  Secrets provider       {(secrets is null ? "NOT REGISTERED" : "registered")}");
Console.WriteLine();

Show("Weather:ApiBaseUrl");
Show("Weather:ApiKey");

Console.WriteLine();
Console.WriteLine($"  secrets.json           {SecretsFolder() ?? "(no provider, nothing to read)"}");
Console.WriteLine();

void Show(string key)
{
    var value = config[key];
    Console.WriteLine($"  {key,-22} {value ?? "(not set)",-24} {Origin(key)}".TrimEnd());
}

// Last provider that can answer for the key is the one that won.
string Origin(string key)
{
    for (var i = config.Providers.Count() - 1; i >= 0; i--)
    {
        var provider = config.Providers.ElementAt(i);

        if (provider.TryGet(key, out _))
        {
            return provider == secrets
                ? "<- user secrets"
                : $"<- {Describe(provider)}";
        }
    }

    return string.Empty;
}

static string Describe(IConfigurationProvider provider) => provider switch
{
    JsonConfigurationProvider json => json.Source.Path ?? "json",
    _ => provider.GetType().Name,
};

string? SecretsFolder() =>
    secrets?.Source.FileProvider is PhysicalFileProvider physical ? physical.Root : null;
