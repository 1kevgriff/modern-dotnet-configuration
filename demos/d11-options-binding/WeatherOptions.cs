namespace D11.OptionsBinding;

/// <summary>
/// The entire configuration surface of the weather client, in one typed place.
/// Validation arrives in d12 — this demo is only about binding.
/// </summary>
internal sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    public required string ApiBaseUrl { get; init; }

    public int TimeoutSeconds { get; init; } = 30;

    public int Retries { get; init; } = 3;

    public required string ApiKey { get; init; }
}
