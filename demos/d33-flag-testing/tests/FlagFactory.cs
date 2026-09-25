using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace D33.FlagTesting.Tests;

/// <summary>
/// Boots the real app with one flag forced to a known value.
/// The punchline: the Microsoft flag schema written out as flat configuration keys.
/// </summary>
internal sealed class FlagFactory(bool enabled) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["feature_management:feature_flags:0:id"] = "NewCheckout",
                ["feature_management:feature_flags:0:enabled"] = enabled ? "true" : "false",
            }));
}
