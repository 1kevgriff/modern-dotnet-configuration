using Microsoft.Extensions.Options;

namespace D11.OptionsBinding;

/// <summary>
/// DO: the constructor names exactly what this class needs, and the values are already typed.
/// There is not a single configuration key anywhere in here.
/// </summary>
internal sealed class WeatherClient(IOptions<WeatherOptions> options)
{
    public WeatherOptions Settings { get; } = options.Value;
}
