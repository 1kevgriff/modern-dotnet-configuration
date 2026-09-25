using Microsoft.Extensions.Configuration;

IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: false)
    .Build();

// Read each file on its own, so the two inputs are printed from the files themselves.
Console.WriteLine();
Console.WriteLine($"   appsettings.json               {Join(SingleFile("appsettings.json"))}");
Console.WriteLine($"   appsettings.Development.json   {Join(SingleFile("appsettings.Development.json"))}");
Console.WriteLine();
Console.WriteLine("   WHAT YOUR APP BINDS");
Console.WriteLine();

foreach (var element in config.GetSection("Weather:AllowedOrigins").GetChildren())
{
    Console.WriteLine($"     {element.Path,-31} = {element.Value,-24} from {SourceFile(config, element.Path)}");
}

Console.WriteLine();
Console.WriteLine("   An array is just index keys, and index 0 is the only one the overlay set.");
Console.WriteLine();
Console.WriteLine("   THE FIX - key the section by name, so every override says what it replaces");
Console.WriteLine();

foreach (var element in config.GetSection("Weather:OriginsByName").GetChildren())
{
    Console.WriteLine($"     {element.Path,-31} = {element.Value,-24} from {SourceFile(config, element.Path)}");
}

Console.WriteLine();

static IConfigurationRoot SingleFile(string fileName) => new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(fileName, optional: false, reloadOnChange: false)
    .Build();

static string Join(IConfiguration source) =>
    string.Join(", ", source.GetSection("Weather:AllowedOrigins").GetChildren().Select(c => c.Value));

// The file behind a key: walk the chain in reverse and take the first provider that has it.
static string SourceFile(IConfigurationRoot root, string key)
{
    foreach (var provider in root.Providers.Reverse())
    {
        if (provider.TryGet(key, out _))
        {
            return provider is FileConfigurationProvider file && file.Source.Path is { } path
                ? path
                : provider.ToString() ?? provider.GetType().Name;
        }
    }

    return "nowhere";
}
