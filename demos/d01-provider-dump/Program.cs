using Microsoft.Extensions.Configuration;

// The chain is built by hand so the demo shows exactly four providers, in order.
// A prefix on the environment variables provider keeps the dump to this demo's
// keys instead of every variable on the machine.
IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(prefix: "DEMO_")
    .AddCommandLine(args)
    .Build();

Console.WriteLine();
Console.WriteLine("PROVIDERS   (added first to last - the last one to supply a key wins)");
Console.WriteLine();

var position = 1;
foreach (var provider in config.Providers)
{
    Console.WriteLine($"   {position++}.  {provider}");
}

Console.WriteLine();
Console.WriteLine("GetDebugView()   key = value   (the provider that supplied it)");
Console.WriteLine();
Console.WriteLine(config.GetDebugView(Redact));

// Never print a debug view unredacted.
static string Redact(ConfigurationDebugViewContext context) =>
    context.Key.Contains("secret", StringComparison.OrdinalIgnoreCase)
    || context.Key.Contains("password", StringComparison.OrdinalIgnoreCase)
    || context.Key.Contains("key", StringComparison.OrdinalIgnoreCase)
        ? "***"
        : context.Value ?? string.Empty;
