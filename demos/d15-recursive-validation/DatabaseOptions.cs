using System.ComponentModel.DataAnnotations;

namespace D15.RecursiveValidation;

/// <summary>A nested options object. Its attributes are the ones that get skipped.</summary>
public sealed class DatabaseOptions
{
    [Required(AllowEmptyStrings = false)]
    public required string ConnectionString { get; init; }

    [Range(1, 300)]
    public required int CommandTimeoutSeconds { get; init; }
}
