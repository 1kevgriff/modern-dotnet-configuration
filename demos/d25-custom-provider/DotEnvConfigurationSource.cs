using Microsoft.Extensions.Configuration;

namespace D25.CustomProvider;

/// <summary>
/// The registration half of a provider: it holds the settings and knows how to build the provider.
/// </summary>
public sealed class DotEnvConfigurationSource : IConfigurationSource
{
    public required string Path { get; init; }

    public bool ReloadOnChange { get; init; }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new DotEnvConfigurationProvider(this);
}
