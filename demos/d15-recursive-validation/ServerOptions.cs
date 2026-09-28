using System.ComponentModel.DataAnnotations;

namespace D15.RecursiveValidation;

/// <summary>An item inside a collection. Collection items are skipped too.</summary>
public sealed class ServerOptions
{
    [Required(AllowEmptyStrings = false)]
    public required string Name { get; init; }

    [Range(1, 65535)]
    public required int Port { get; init; }
}
