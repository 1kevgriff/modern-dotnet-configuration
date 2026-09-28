using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace D15.RecursiveValidation;

/// <summary>
/// Identical to <see cref="FlatAppOptions"/> except for the two attributes that opt
/// into recursion. That two-line diff is the entire demo.
/// </summary>
public sealed class RecursiveAppOptions
{
    [Required]
    [ValidateObjectMembers]
    public required DatabaseOptions Database { get; init; }

    [ValidateEnumeratedItems]
    public required List<ServerOptions> Servers { get; init; }
}
