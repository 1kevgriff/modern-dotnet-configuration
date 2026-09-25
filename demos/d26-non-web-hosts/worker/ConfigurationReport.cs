using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace D26.NonWebHosts.Worker;

/// <summary>Prints what the generic host wired up, then stops the host so the demo ends.</summary>
public sealed class ConfigurationReport(
    IConfiguration configuration,
    IHostEnvironment environment,
    IOptions<WeatherOptions> options,
    IHostApplicationLifetime lifetime,
    ILoggerFactory loggerFactory) : IHostedService
{
    // A short category keeps each line readable from the back of the room.
    private readonly ILogger _logger = loggerFactory.CreateLogger("worker");

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.HostKind(_logger, "Host.CreateApplicationBuilder - no ASP.NET Core");
        Log.Environment(_logger, environment.EnvironmentName);
        Log.Endpoint(_logger, options.Value.Endpoint);
        Log.Timeout(_logger, options.Value.TimeoutSeconds);

        // The host registers its ConfigurationManager as IConfiguration; it is also the root.
        if (configuration is IConfigurationRoot root && _logger.IsEnabled(LogLevel.Information))
        {
            foreach (var provider in root.Providers)
            {
                var description = Describe(provider);
                Log.Provider(_logger, description);
            }
        }

        lifetime.StopApplication();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // File providers override ToString() with something useful. The rest fall back to their full
    // type name, which is too wide for a slide, so shorten those to the bare type name.
    private static string Describe(IConfigurationProvider provider)
    {
        var type = provider.GetType();
        var description = provider.ToString();

        return string.IsNullOrEmpty(description) || description == type.FullName
            ? type.Name
            : description;
    }
}
