using D17.SourceGenerators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();

var options = builder.Services.AddOptions<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName));

#if USE_GENERATORS
// Generated validator: no reflection, no IL2026.
builder.Services.AddSingleton<IValidateOptions<WeatherOptions>, ValidateWeatherOptions>();
const string Mode = "generators ON   (EnableConfigurationBindingGenerator + [OptionsValidator])";
#else
// Reflection validator: this call is what the AOT analyzer complains about.
options.ValidateDataAnnotations();
const string Mode = "generators OFF  (reflection binding + ValidateDataAnnotations)";
#endif

options.ValidateOnStart();

const string Rule = "  --------------------------------------------------------------";

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using var host = builder.Build();

await host.StartAsync(cts.Token);
var weather = host.Services.GetRequiredService<IOptions<WeatherOptions>>().Value;
await host.StopAsync(cts.Token);

Console.WriteLine();
Console.WriteLine("  d17 - source generators remove reflection from binding and validation");
Console.WriteLine(Rule);
Console.WriteLine();
Console.WriteLine($"  Mode:      {Mode}");
Console.WriteLine();
Console.WriteLine($"  Bound:     {weather.ApiBaseUrl}");
Console.WriteLine($"             timeout {weather.TimeoutSeconds}s, {weather.Retries} retries");
Console.WriteLine();
Console.WriteLine("  The payoff is in the BUILD output, not here - count the IL2026/IL3050 lines.");
Console.WriteLine();
