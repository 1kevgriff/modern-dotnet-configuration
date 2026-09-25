using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace D16.ValidateOptionsClass;

/// <summary>
/// Service-dependent: the rule changes with <see cref="IHostEnvironment"/>. No attribute
/// can reach the environment, so this cannot be expressed declaratively.
/// </summary>
public sealed class WeatherEndpointValidator(IHostEnvironment environment)
    : IValidateOptions<WeatherOptions>
{
    public ValidateOptionsResult Validate(string? name, WeatherOptions options)
    {
        var failures = new List<string>();

        if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out var uri))
        {
            failures.Add($"Weather:ApiBaseUrl is not an absolute URI ('{options.ApiBaseUrl}').");
            return ValidateOptionsResult.Fail(failures);
        }

        if (!uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"Weather:ApiBaseUrl must use HTTPS (found '{uri.Scheme}').");
        }

        if (environment.IsProduction() && uri.IsLoopback)
        {
            failures.Add(
                $"Weather:ApiBaseUrl points at loopback ('{uri.Host}') in {environment.EnvironmentName}.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
