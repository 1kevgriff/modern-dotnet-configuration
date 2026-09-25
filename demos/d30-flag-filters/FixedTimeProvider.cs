namespace D30.FlagFilters;

/// <summary>A clock the demo controls, so the time-window answers are deterministic on stage.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
