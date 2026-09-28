using Microsoft.Extensions.Configuration;

const string Key = "Weather:TimeoutSeconds";

// The environment name is just a string: it picks which JSON file overlays the base one.
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

Console.WriteLine();
Console.WriteLine($"   ENVIRONMENT   {environment}");
Console.WriteLine();
Console.WriteLine($"   {Key}   =   {config[Key]}");
Console.WriteLine();
Console.WriteLine($"   supplied by   {WinningProvider(config, Key)}");
Console.WriteLine();

// Reads walk the provider list in reverse and take the first hit. That is all
// "last provider wins" means.
static string WinningProvider(IConfigurationRoot root, string key)
{
    foreach (var provider in root.Providers.Reverse())
    {
        if (provider.TryGet(key, out _))
        {
            return provider.ToString() ?? provider.GetType().Name;
        }
    }

    return "nothing - the key is not set anywhere";
}
