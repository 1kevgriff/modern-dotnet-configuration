namespace D27.ReloadOnChange;

public sealed class GreetingOptions
{
    public const string SectionName = "Greeting";

    /// <summary>Set in appsettings.json, watched by the file provider.</summary>
    public required string FromFile { get; init; }

    /// <summary>Set by GREETING__FROMENV, read exactly once at startup.</summary>
    public required string FromEnv { get; init; }
}
