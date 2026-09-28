namespace D16.ValidateOptionsClass;

/// <summary>
/// Deliberately carries no validation attributes. Every rule that matters here is
/// either cross-field or depends on a service, and attributes can express neither.
/// </summary>
public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    public required string ApiBaseUrl { get; init; }

    public required int TimeoutSeconds { get; init; }

    public required int Retries { get; init; }
}
