using System.ComponentModel.DataAnnotations;

namespace D12.ValidateOnStart;

/// <summary>
/// The rules live on the options class, so they are checked wherever this section is bound —
/// not scattered through the code that happens to read the values.
/// </summary>
internal sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    [Required]
    [Url]
    public required string ApiBaseUrl { get; init; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    [Required]
    [MinLength(8)]
    public required string ApiKey { get; init; }
}
