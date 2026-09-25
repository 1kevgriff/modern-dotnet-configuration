namespace D25.CustomProvider;

public sealed class GreetingOptions
{
    public const string SectionName = "Greeting";

    public required string Message { get; init; }

    public required string Audience { get; init; }
}
