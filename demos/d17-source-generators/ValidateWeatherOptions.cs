using Microsoft.Extensions.Options;

namespace D17.SourceGenerators;

/// <summary>
/// Empty by design. The options-validation source generator writes the
/// <see cref="IValidateOptions{TOptions}"/> body from the attributes on
/// <see cref="WeatherOptions"/>, with no reflection at runtime.
/// </summary>
[OptionsValidator]
public sealed partial class ValidateWeatherOptions : IValidateOptions<WeatherOptions>;
