using System.Globalization;
using System.Text.RegularExpressions;

namespace D32.FlagInventory;

/// <summary>
/// The convention that makes the inventory possible: a flag name files its own category and expiry.
/// <c>Checkout_V2_Rollout_2026Q1</c> is a release toggle that should be deleted after 2026 Q1.
/// </summary>
internal static partial class FlagNaming
{
    public static string CategoryOf(string flagName) => LastWord(flagName) switch
    {
        "Rollout" => "release",
        "Experiment" => "experiment",
        "KillSwitch" => "ops",
        "Permission" => "permission",
        _ => "uncategorized",
    };

    public static DateOnly? ExpiryOf(string flagName)
    {
        Match match = QuarterSuffix().Match(flagName);

        if (!match.Success)
        {
            // Ops toggles and permission toggles are permanent, and that is fine.
            return null;
        }

        int year = int.Parse(match.Groups["year"].ValueSpan, CultureInfo.InvariantCulture);
        int quarter = int.Parse(match.Groups["quarter"].ValueSpan, CultureInfo.InvariantCulture);
        int lastMonth = quarter * 3;

        return new DateOnly(year, lastMonth, DateTime.DaysInMonth(year, lastMonth));
    }

    private static string LastWord(string flagName)
    {
        string[] parts = flagName.Split('_', StringSplitOptions.RemoveEmptyEntries);

        // A trailing quarter means the category is the word before it.
        return parts.Length > 1 && QuarterSuffix().IsMatch(flagName)
            ? parts[^2]
            : parts[^1];
    }

    [GeneratedRegex(@"_(?<year>\d{4})Q(?<quarter>[1-4])$")]
    private static partial Regex QuarterSuffix();
}
