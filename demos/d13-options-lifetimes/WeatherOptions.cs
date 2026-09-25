namespace D13.OptionsLifetimes;

/// <summary>
/// Exactly one setting, so the only thing moving on the projector is the thing being taught.
/// </summary>
internal sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    public int TimeoutSeconds { get; init; } = 30;
}
