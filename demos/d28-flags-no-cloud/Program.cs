using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

// A feature flag is just configuration. This is the ordinary provider chain from d01/d02 --
// nothing here knows the word "Azure".
IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

ServiceCollection services = new();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging();
services.AddFeatureManagement();

await using ServiceProvider provider = services.BuildServiceProvider();
IVariantFeatureManager features = provider.GetRequiredService<IVariantFeatureManager>();

Console.WriteLine();
Console.WriteLine("  D28 - a feature flag is just configuration");
Console.WriteLine();

await foreach (string flag in features.GetFeatureNamesAsync(CancellationToken.None))
{
    bool enabled = await features.IsEnabledAsync(flag, CancellationToken.None);
    Console.WriteLine($"      {flag,-14}{(enabled ? "ON" : "OFF")}");
}

// The flag's "enabled" is an ordinary key. Whichever provider set it last, wins.
const string Key = "feature_management:feature_flags:0:enabled";

Console.WriteLine();
Console.WriteLine($"      {Key}");
Console.WriteLine($"        = {configuration[Key]}   <- {WinningProvider(configuration, Key)}");
Console.WriteLine();

// The ~15-line dump helper: walk the providers backwards, first one that answers is the winner.
static string WinningProvider(IConfigurationRoot root, string key)
{
    foreach (IConfigurationProvider provider in root.Providers.Reverse())
    {
        if (provider.TryGet(key, out _))
        {
            return provider.ToString() ?? provider.GetType().Name;
        }
    }

    return "(not set)";
}
