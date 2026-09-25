using System.Globalization;
using D30.FlagFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

DateTimeOffset clock = DateTimeOffset.Parse(
    configuration["clock"] ?? "2026-12-10T09:00:00Z",
    CultureInfo.InvariantCulture,
    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

string tenant = configuration["tenant"] ?? "fabrikam";

ServiceCollection services = new();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton(new TenantContext { Name = tenant });
services.AddLogging();
services.AddFeatureManagement().AddFeatureFilter<TenantFilter>();

await using ServiceProvider provider = services.BuildServiceProvider();

// The built-in time-window filter exposes SystemClock as a TimeProvider precisely so it can be
// driven from a test - or from a stage.
provider.GetServices<IFeatureFilterMetadata>()
    .OfType<TimeWindowFilter>()
    .Single()
    .SystemClock = new FixedTimeProvider(clock);

IVariantFeatureManager features = provider.GetRequiredService<IVariantFeatureManager>();

(string Flag, string Filter, string Window)[] rows =
[
    ("HolidayPricing", "Microsoft.TimeWindow", "1 Dec - 26 Dec 2026"),
    ("NightlyBatch", "Microsoft.TimeWindow", "daily 01:00 - 03:00"),
    ("TenantBeta", "Tenant (custom)", "tenant = contoso"),
    ("HolidayPricingForContoso", "All of both", "window AND tenant"),
];

Console.WriteLine();
Console.WriteLine("  D30 - a flag decides for itself, from data you supply");
Console.WriteLine();
Console.WriteLine($"    clock: {clock:yyyy-MM-dd HH:mm} UTC      tenant: {tenant}");
Console.WriteLine();

foreach ((string flag, string filter, string window) in rows)
{
    bool on = await features.IsEnabledAsync(flag, CancellationToken.None);
    Console.WriteLine($"      {flag,-26}{(on ? "ON " : "OFF")}   {filter,-23}{window}");
}

Console.WriteLine();
