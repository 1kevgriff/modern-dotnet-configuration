using System.Globalization;
using D32.FlagInventory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

TimeProvider clock = configuration["today"] is { } fixedDay
    ? new FixedTimeProvider(DateTimeOffset.Parse(fixedDay, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal))
    : TimeProvider.System;

DateOnly today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

ServiceCollection services = new();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging();
services.AddFeatureManagement();

await using ServiceProvider provider = services.BuildServiceProvider();
IVariantFeatureManager features = provider.GetRequiredService<IVariantFeatureManager>();
IConfigurationSection owners = configuration.GetSection("flag_owners");

// GetFeatureNamesAsync is the whole trick: the app can enumerate every flag it knows about.
List<FlagRecord> inventory = [];

await foreach (string name in features.GetFeatureNamesAsync(CancellationToken.None))
{
    inventory.Add(new FlagRecord(
        Name: name,
        Enabled: await features.IsEnabledAsync(name, CancellationToken.None),
        Category: FlagNaming.CategoryOf(name),
        Owner: owners[name] ?? "unowned",
        Expires: FlagNaming.ExpiryOf(name)));
}

List<FlagRecord> rows = [.. inventory.OrderBy(f => f.Expires ?? DateOnly.MaxValue).ThenBy(f => f.Name, StringComparer.Ordinal)];
int overdue = rows.Count(f => f.IsOverdue(today));

Console.WriteLine();
Console.WriteLine("  D32 - you can't manage flag debt you can't see");
Console.WriteLine();
Console.WriteLine($"    today: {today:yyyy-MM-dd}");
Console.WriteLine();
Console.WriteLine($"    {"FLAG",-34}{"STATE",-7}{"CATEGORY",-13}{"OWNER",-11}EXPIRES");

foreach (FlagRecord flag in rows)
{
    bool late = flag.IsOverdue(today);

    if (late)
    {
        Console.ForegroundColor = ConsoleColor.Red;
    }

    string expires = flag.Expires is { } due ? due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "never";
    string note = flag.Expires is { } date
        ? late
            ? $"OVERDUE {today.DayNumber - date.DayNumber}d"
            : $"{date.DayNumber - today.DayNumber}d left"
        : string.Empty;

    Console.WriteLine($"    {flag.Name,-34}{(flag.Enabled ? "ON" : "OFF"),-7}{flag.Category,-13}{flag.Owner,-11}{expires,-13}{note}".TrimEnd());

    Console.ResetColor();
}

Console.WriteLine();
Console.WriteLine($"    {overdue} of {rows.Count} flags are past their expiry date.");
Console.WriteLine("    Deleting a flag is a code change, not a config change.");
Console.WriteLine();
