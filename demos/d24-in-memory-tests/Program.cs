using D24.InMemoryTests;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .ValidateOnStart();

var app = builder.Build();

// The endpoint under test reports the configuration the app actually resolved.
app.MapGet("/config", (IOptions<WeatherOptions> options) => Results.Ok(options.Value));

await app.RunAsync();

/// <summary>Exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can find the entry point.</summary>
public partial class Program;
