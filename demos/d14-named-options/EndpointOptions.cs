namespace D14.NamedOptions;

/// <summary>
/// One shape, configured several times. The names are constants because option names are
/// case-sensitive strings, and a typo silently hands you an unconfigured instance.
/// </summary>
internal sealed class EndpointOptions
{
    public const string SectionName = "Endpoints";

    public const string Primary = "primary";

    public const string Secondary = "secondary";

    public required string BaseUrl { get; init; }

    public int TimeoutSeconds { get; init; } = 30;
}
