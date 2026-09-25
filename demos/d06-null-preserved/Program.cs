using System.Globalization;
using System.Runtime.InteropServices;
using D06.NullPreserved;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var weather = config.GetSection("Weather").Get<WeatherOptions>() ?? new WeatherOptions();

Console.WriteLine();
Console.WriteLine($"   RUNNING ON   {RuntimeInformation.FrameworkDescription}");
Console.WriteLine();
Console.WriteLine(@"   appsettings.json:   ""ApiBaseUrl"": null,  ""Retries"": null,  ""Tags"": [ ""alpha"", null, ""charlie"" ]");
Console.WriteLine();
Console.WriteLine("   WHAT THE JSON PROVIDER STORES");
Console.WriteLine();
Console.WriteLine($"     Weather:ApiBaseUrl   {Show(config["Weather:ApiBaseUrl"])}");
Console.WriteLine($"     Weather:Tags:1       {Show(config["Weather:Tags:1"])}");
Console.WriteLine();
Console.WriteLine("   WHAT THE BINDER DOES WITH IT");
Console.WriteLine();
Console.WriteLine($"     {"property",-14} {"bound value",-26} {"class default"}");
Console.WriteLine($"     {"ApiBaseUrl",-14} {Show(weather.ApiBaseUrl),-26} https://api.example.com");
Console.WriteLine($"     {"Retries",-14} {Show(weather.Retries?.ToString(CultureInfo.InvariantCulture)),-26} 3");
Console.WriteLine($"     {"Tags",-14} {string.Join(", ", weather.Tags.Select(Show)),-26} (empty array)");
Console.WriteLine();

static string Show(string? value) => value is null ? "(null)" : value.Length == 0 ? "(empty)" : value;
