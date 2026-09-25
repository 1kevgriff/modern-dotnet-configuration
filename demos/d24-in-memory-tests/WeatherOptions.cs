namespace D24.InMemoryTests;

public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    public required string ApiBaseUrl { get; init; }

    public required int TimeoutSeconds { get; init; }
}
