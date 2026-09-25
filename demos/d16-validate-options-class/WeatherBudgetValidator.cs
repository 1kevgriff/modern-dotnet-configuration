using Microsoft.Extensions.Options;

namespace D16.ValidateOptionsClass;

/// <summary>
/// Cross-field: neither value is wrong on its own, only their product is. A second,
/// separate validator — both run because of TryAddEnumerable.
/// </summary>
public sealed class WeatherBudgetValidator : IValidateOptions<WeatherOptions>
{
    private const int MaxBudgetSeconds = 30;

    public ValidateOptionsResult Validate(string? name, WeatherOptions options)
    {
        var worstCase = options.TimeoutSeconds * (options.Retries + 1);

        return worstCase <= MaxBudgetSeconds
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                $"Weather timeout budget is {worstCase}s " +
                $"({options.TimeoutSeconds}s x {options.Retries + 1} attempts); " +
                $"the limit is {MaxBudgetSeconds}s.");
    }
}
