using D16.ValidateOptionsClass;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();

var fix = builder.Configuration.GetValue<bool>("Fix");
var registration = builder.Configuration.GetValue<string>("Registration") ?? "tryaddenumerable";

if (fix)
{
    // Last provider wins - overlay a configuration that satisfies every rule.
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Weather:ApiBaseUrl"] = "https://api.example.com/weather",
        ["Weather:TimeoutSeconds"] = "5",
        ["Weather:Retries"] = "2",
    });
}

builder.Services.AddOptions<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .ValidateOnStart();

if (registration.Equals("tryaddsingleton", StringComparison.OrdinalIgnoreCase))
{
    // DON'T: TryAddSingleton keeps the first registration for the service type and
    // silently discards every later one, so only one of these two validators ever runs.
    builder.Services.TryAddSingleton<IValidateOptions<WeatherOptions>, WeatherEndpointValidator>();
    builder.Services.TryAddSingleton<IValidateOptions<WeatherOptions>, WeatherBudgetValidator>();
}
else
{
    // Options resolves IEnumerable<IValidateOptions<T>>: every validator runs, and
    // TryAddEnumerable appends without duplicating the same implementation type.
    builder.Services.TryAddEnumerable(
        ServiceDescriptor.Singleton<IValidateOptions<WeatherOptions>, WeatherEndpointValidator>());
    builder.Services.TryAddEnumerable(
        ServiceDescriptor.Singleton<IValidateOptions<WeatherOptions>, WeatherBudgetValidator>());
}

const string Rule = "  --------------------------------------------------------------";

using var host = builder.Build();
var registered = host.Services.GetServices<IValidateOptions<WeatherOptions>>().Count();

Console.WriteLine();
Console.WriteLine("  d16 - rules that attributes cannot express");
Console.WriteLine(Rule);
Console.WriteLine();
Console.WriteLine($"  Registration:   {registration}   ->  {registered} validator(s) will run");
Console.WriteLine($"  Config:         {(fix ? "--Fix true overlay" : "appsettings.json")}");
Console.WriteLine(Rule);
Console.WriteLine();

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    await host.StartAsync(cts.Token);
    await host.StopAsync(cts.Token);

    Console.WriteLine("  STARTED.  Every rule satisfied.");
}
catch (OptionsValidationException ex)
{
    Console.WriteLine($"  REFUSED TO START.  {ex.Failures.Count()} failures:");
    Console.WriteLine();

    foreach (var failure in ex.Failures)
    {
        Console.WriteLine($"      * {failure}");
    }
}

Console.WriteLine();
