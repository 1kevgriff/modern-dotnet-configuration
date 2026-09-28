using System.ComponentModel.DataAnnotations;

namespace D15.RecursiveValidation;

/// <summary>
/// The naive shape. <c>[Required]</c> on <see cref="Database"/> only checks that the
/// object is non-null — nothing inside it, and nothing inside <see cref="Servers"/>,
/// is ever looked at.
/// </summary>
public sealed class FlatAppOptions
{
    public const string SectionName = "App";

    [Required]
    public required DatabaseOptions Database { get; init; }

    public required List<ServerOptions> Servers { get; init; }
}
