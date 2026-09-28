namespace D13.OptionsLifetimes;

/// <summary>
/// Structured logging, source generated. The web SDK's analyzers reject
/// <c>logger.LogInformation(...)</c> here (CA1848), and this is the shape to copy anyway:
/// the message is a template with named placeholders, never an interpolated string.
/// </summary>
internal static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "appsettings.json changed — timeout is now {TimeoutSeconds}s")]
    public static partial void OptionsChanged(ILogger logger, int timeoutSeconds);
}
