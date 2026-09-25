namespace D32.FlagInventory;

/// <summary>A clock the demo controls, so the overdue column is deterministic on stage.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
