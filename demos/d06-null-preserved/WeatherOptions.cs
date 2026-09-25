namespace D06.NullPreserved;

/// <summary>Two of these three properties have a perfectly good default. That is the point.</summary>
internal sealed class WeatherOptions
{
    public string ApiBaseUrl { get; init; } = "https://api.example.com";

    public int? Retries { get; init; } = 3;

    public string?[] Tags { get; init; } = [];
}
