using D11.OptionsBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// DON'T: hand the whole configuration root to a service.
builder.Services.AddSingleton<WeatherClientBefore>();

// DO: bind the section once, then inject the bound class.
builder.Services
    .AddOptions<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName));

builder.Services.AddSingleton<WeatherClient>();

using var host = builder.Build();

var before = host.Services.GetRequiredService<WeatherClientBefore>();
var after = host.Services.GetRequiredService<WeatherClient>();

Console.WriteLine();
Console.WriteLine("  d11 — bind a class, stop injecting IConfiguration");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("  BEFORE    WeatherClientBefore(IConfiguration config)");
Console.WriteLine();
Console.WriteLine($"""            config["Weather:TimeoutSeconds"]   ->   "{before.RawTimeout}"      a string, re-read every call""");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("  AFTER     WeatherClient(IOptions<WeatherOptions> options)");
Console.WriteLine();
Console.WriteLine($"            options.Value.TimeoutSeconds       ->    {after.Settings.TimeoutSeconds}       an int, bound once");
Console.WriteLine();
Console.WriteLine($"            ApiBaseUrl   {after.Settings.ApiBaseUrl}");
Console.WriteLine($"            Retries      {after.Settings.Retries}");
Console.WriteLine($"            ApiKey       {new string('*', 12)}   (placeholder — real value lives in user secrets)");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("  Same value twice. Only one of them breaks the build when the key is renamed.");
Console.WriteLine();
