namespace D32.FlagInventory;

/// <summary>One row of the inventory: what the flag is, who owns it, and when it should have gone.</summary>
internal sealed record FlagRecord(
    string Name,
    bool Enabled,
    string Category,
    string Owner,
    DateOnly? Expires)
{
    public bool IsOverdue(DateOnly today) => Expires is { } due && due < today;
}
