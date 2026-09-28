using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;

namespace D30.FlagFilters;

/// <summary>A custom filter is one method. The alias is the name used in appsettings.json.</summary>
[FilterAlias("Tenant")]
internal sealed class TenantFilter(TenantContext tenant) : IFeatureFilter
{
    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        TenantFilterSettings settings = context.Parameters.Get<TenantFilterSettings>() ?? new TenantFilterSettings();

        return Task.FromResult(settings.Tenants.Contains(tenant.Name, StringComparer.OrdinalIgnoreCase));
    }
}
