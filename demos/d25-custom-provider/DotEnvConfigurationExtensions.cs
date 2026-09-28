using Microsoft.Extensions.Configuration;

namespace D25.CustomProvider;

/// <summary>The AddXxx method that makes a custom source look like a built-in one.</summary>
public static class DotEnvConfigurationExtensions
{
    public static IConfigurationBuilder AddDotEnvFile(
        this IConfigurationBuilder builder,
        string path,
        bool reloadOnChange = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.Add(new DotEnvConfigurationSource
        {
            Path = path,
            ReloadOnChange = reloadOnChange,
        });
    }
}
