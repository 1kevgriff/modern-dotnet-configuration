using D26.NonWebHosts.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// The same builder a web app uses, minus the web. JSON, user secrets, environment variables and
// command line arrive in the same order, with the same precedence.
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.SingleLine = true);

builder.Services
    .AddOptionsWithValidateOnStart<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddHostedService<ConfigurationReport>();

using var host = builder.Build();
await host.RunAsync();
