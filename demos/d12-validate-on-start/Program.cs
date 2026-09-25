using D12.ValidateOnStart;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

// Stage noise only — every line below is printed deliberately.
builder.Logging.ClearProviders();

// appsettings.json ships a deliberately invalid Weather:TimeoutSeconds (900, limit is 300).
// The only thing this switch changes is *when* anyone notices.
var validateOnStart = builder.Configuration.GetValue<bool>("Validation:OnStart");

var options = builder.Services
    .AddOptions<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .ValidateDataAnnotations();

if (validateOnStart)
{
    options.ValidateOnStart();
}

using var host = builder.Build();

Console.WriteLine();
Console.WriteLine($"  d12 — ValidateOnStart is {(validateOnStart ? "ON" : "OFF")}");
Console.WriteLine();
Console.WriteLine();

try
{
    await host.StartAsync().ConfigureAwait(false);
}
catch (OptionsValidationException ex)
{
    Console.WriteLine("  REFUSED TO START");
    Console.WriteLine();
    WriteFailures(ex);
    Console.WriteLine("  The rolling deployment stops here. No instance ever takes traffic.");
    Console.WriteLine();
    return 1;
}

Console.WriteLine("  STARTED — health checks green, load balancer sending traffic.");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("  ... first request reaches the code that reads the value ...");
Console.WriteLine();
Console.WriteLine();

try
{
    var weather = host.Services.GetRequiredService<IOptions<WeatherOptions>>().Value;
    Console.WriteLine($"  Timeout is {weather.TimeoutSeconds}s.");
}
catch (OptionsValidationException ex)
{
    Console.WriteLine("  REQUEST FAILED");
    Console.WriteLine();
    WriteFailures(ex);
    Console.WriteLine("  Same bad config. Found by a customer instead of by the deployment.");
}

Console.WriteLine();
await host.StopAsync().ConfigureAwait(false);
return 0;

static void WriteFailures(OptionsValidationException exception)
{
    foreach (var failure in exception.Failures)
    {
        Console.WriteLine($"      {failure}");
    }

    Console.WriteLine();
}
