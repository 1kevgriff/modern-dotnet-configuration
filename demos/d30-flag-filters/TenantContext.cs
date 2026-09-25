namespace D30.FlagFilters;

/// <summary>The tenant the current unit of work belongs to. A web app would read this off the request.</summary>
internal sealed class TenantContext
{
    public required string Name { get; init; }
}
