using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace D24.InMemoryTests.Tests;

/// <summary>
/// Adds one more configuration provider after everything the app registered.
/// Last provider wins, so these values beat appsettings.json without touching it.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string TestApiBaseUrl = "https://stub.invalid/weather";
    public const int TestTimeoutSeconds = 1;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Weather:ApiBaseUrl"] = TestApiBaseUrl,
                ["Weather:TimeoutSeconds"] = TestTimeoutSeconds.ToString(CultureInfo.InvariantCulture),
            }));

        // Keep the test summary readable on a projector.
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }
}
