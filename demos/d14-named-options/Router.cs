using Microsoft.Extensions.Options;

namespace D14.NamedOptions;

/// <summary>
/// One dependency, several configured instances. There is no EndpointOptionsPrimary type,
/// and there never needs to be one.
/// </summary>
internal sealed class Router(IOptionsMonitor<EndpointOptions> monitor)
{
    public EndpointOptions Primary => monitor.Get(EndpointOptions.Primary);

    public EndpointOptions Secondary => monitor.Get(EndpointOptions.Secondary);

    // The unnamed instance is not special: Options.DefaultName is just the empty string.
    public EndpointOptions Unnamed => monitor.Get(Options.DefaultName);
}
