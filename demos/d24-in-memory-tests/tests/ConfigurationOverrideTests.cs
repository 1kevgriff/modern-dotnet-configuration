using System.Net.Http.Json;
using Xunit;

namespace D24.InMemoryTests.Tests;

public sealed class ConfigurationOverrideTests
{
    [Fact]
    public async Task Endpoint_uses_values_that_exist_nowhere_on_disk()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var weather = await client.GetFromJsonAsync<WeatherOptions>(
            "/config",
            TestContext.Current.CancellationToken);

        Assert.NotNull(weather);
        Assert.Equal(ApiFactory.TestApiBaseUrl, weather.ApiBaseUrl);
        Assert.Equal(ApiFactory.TestTimeoutSeconds, weather.TimeoutSeconds);
    }

    [Fact]
    public async Task Committed_appsettings_values_are_the_ones_that_got_beaten()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var weather = await client.GetFromJsonAsync<WeatherOptions>(
            "/config",
            TestContext.Current.CancellationToken);

        Assert.NotNull(weather);
        Assert.NotEqual("https://api.example.com/weather", weather.ApiBaseUrl);
        Assert.NotEqual(30, weather.TimeoutSeconds);
    }
}
