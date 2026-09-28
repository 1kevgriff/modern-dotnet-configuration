using System.ComponentModel.DataAnnotations;

namespace D17.SourceGenerators;

/// <summary>
/// Note the plain <c>set</c> accessors. The configuration binding source generator
/// does not write to <c>init</c>-only properties — it skips them silently, leaving
/// the whole object empty. See the README; this is the one place in the repo that
/// deviates from the required+init house rule, and it is not optional.
/// </summary>
public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    [Required(AllowEmptyStrings = false)]
    public required string ApiBaseUrl { get; set; }

    [Range(1, 60)]
    public required int TimeoutSeconds { get; set; }

    [Range(0, 5)]
    public required int Retries { get; set; }
}
