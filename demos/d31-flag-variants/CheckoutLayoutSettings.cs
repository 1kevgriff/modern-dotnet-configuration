namespace D31.FlagVariants;

/// <summary>An ordinary options class. The variant's configuration binds into it exactly like SPEC section 5.</summary>
internal sealed class CheckoutLayoutSettings
{
    public required string ButtonSize { get; init; }

    public required int Columns { get; init; }

    public required bool ShowUpsell { get; init; }
}
