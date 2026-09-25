using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace D11.OptionsBinding;

/// <summary>
/// DON'T: this class takes the whole configuration root, so its real dependencies are invisible.
/// Every read is a magic string, a null-forgiving operator, and a parse — on every call.
/// </summary>
internal sealed class WeatherClientBefore(IConfiguration config)
{
    // DON'T: the key is a string, so a rename in appsettings.json compiles fine and fails at runtime.
    public string RawTimeout => config["Weather:TimeoutSeconds"]!;

    public int TimeoutSeconds =>
        int.Parse(config["Weather:TimeoutSeconds"]!, CultureInfo.InvariantCulture);
}
