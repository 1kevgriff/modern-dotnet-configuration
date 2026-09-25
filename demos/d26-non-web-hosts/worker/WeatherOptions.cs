using System.ComponentModel.DataAnnotations;

namespace D26.NonWebHosts.Worker;

public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    [Required]
    [Url]
    public required string Endpoint { get; init; }

    [Range(1, 300)]
    public required int TimeoutSeconds { get; init; }
}
