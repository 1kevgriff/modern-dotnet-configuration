using Microsoft.Extensions.Configuration;

// A double-clicked EXE does not start in its install directory, so the base path has to be stated.
// This is Option A from SPEC 4.11: no host, IConfiguration built by hand.
IConfigurationRoot installed = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

// The same chain, anchored to the working directory instead - the bug this demo exists to show.
IConfigurationRoot workingDirectory = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

using var installedLifetime = (IDisposable)installed;
using var workingDirectoryLifetime = (IDisposable)workingDirectory;

Console.WriteLine();
Console.WriteLine("DESKTOP APP   no host, IConfiguration built by hand");
Console.WriteLine();
Console.WriteLine($"    AppContext.BaseDirectory          {AppContext.BaseDirectory}");
Console.WriteLine($"    Directory.GetCurrentDirectory()   {Directory.GetCurrentDirectory()}");

Console.WriteLine();
Console.WriteLine("Weather:Endpoint, resolved from each of those two base paths");
Console.WriteLine();
Console.WriteLine($"    from base directory      {Describe(installed["Weather:Endpoint"])}");
Console.WriteLine($"    from working directory   {Describe(workingDirectory["Weather:Endpoint"])}");

Console.WriteLine();
Console.WriteLine("PROVIDERS   (the same chain a worker gets, just assembled by hand)");
Console.WriteLine();
foreach (var provider in installed.Providers)
{
    Console.WriteLine($"    {provider}");
}

// IConfiguration is read-only by design. Per-user settings are a different problem with a
// different home, and they are the one thing a desktop app writes back.
var perUser = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "D26.NonWebHosts.Desktop",
    "user.json");

Console.WriteLine();
Console.WriteLine("IConfiguration has no write API. Per-user settings would live in");
Console.WriteLine($"    {perUser}");
Console.WriteLine();

static string Describe(string? value) => value ?? "(no appsettings.json on this path)";
