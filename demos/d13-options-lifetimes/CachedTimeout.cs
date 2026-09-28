using Microsoft.Extensions.Options;

namespace D13.OptionsLifetimes;

/// <summary>
/// DON'T: this is the second classic bug. Reading <c>CurrentValue</c> once, at construction, and
/// keeping it in a field quietly turns an <c>IOptionsMonitor&lt;T&gt;</c> back into an
/// <c>IOptions&lt;T&gt;</c> — the subscription is still live, the value never moves.
/// </summary>
internal sealed class CachedTimeout(IOptionsMonitor<WeatherOptions> monitor)
{
    public int TimeoutSeconds { get; } = monitor.CurrentValue.TimeoutSeconds;
}
