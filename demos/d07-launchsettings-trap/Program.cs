using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

// The SDK sets this when it applies a profile from launchSettings.json.
var profile = Environment.GetEnvironmentVariable("DOTNET_LAUNCH_PROFILE");

Console.WriteLine();
Console.WriteLine($"   LAUNCH PROFILE   {(string.IsNullOrEmpty(profile) ? "(none)" : profile)}");
Console.WriteLine();
Console.WriteLine($"   Weather:ApiBaseUrl   =   {config["Weather:ApiBaseUrl"]}");
Console.WriteLine();
Console.WriteLine("   candidates:  appsettings.json          https://api.example.com");
Console.WriteLine("                machine or shell         Weather__ApiBaseUrl");
Console.WriteLine("                launchSettings.json       https://launchsettings.example.com");
Console.WriteLine();
