namespace D30.FlagFilters;

/// <summary>The shape of the custom filter's <c>parameters</c> block in appsettings.json.</summary>
internal sealed class TenantFilterSettings
{
    public IReadOnlyList<string> Tenants { get; init; } = [];
}
