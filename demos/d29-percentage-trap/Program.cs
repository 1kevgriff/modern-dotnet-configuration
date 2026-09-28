using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

const int Checks = 10;
string[] Crowd = ["alex", "marsha", "sam", "jo", "kim", "raj", "lee", "ana"];

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

string user = configuration["user"] ?? "alex";

ServiceCollection services = new();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging();

// ContextualTargetingFilter takes the user at the call site, so the demo never hides who is asking.
services.AddFeatureManagement().AddFeatureFilter<ContextualTargetingFilter>();

await using ServiceProvider provider = services.BuildServiceProvider();
IVariantFeatureManager features = provider.GetRequiredService<IVariantFeatureManager>();

TargetingContext who = new() { UserId = user, Groups = [] };

Console.WriteLine();
Console.WriteLine("  D29 - Microsoft.Percentage is per-call, not per-user");
Console.WriteLine();
Console.WriteLine($"    user: {user}      {Checks} checks, one process, one second");
Console.WriteLine();

Console.WriteLine("    HalfOn        Microsoft.Percentage  50%");
Console.WriteLine($"      {await RowAsync(features, "HalfOn", who)}");
Console.WriteLine();

Console.WriteLine("    HalfOnStable  Microsoft.Targeting   50%");
Console.WriteLine($"      {await RowAsync(features, "HalfOnStable", who)}");
Console.WriteLine();

Console.WriteLine("    Percentage re-rolls the dice every evaluation. Targeting hashes the user,");
Console.WriteLine("    so one user gets one answer - and the crowd still splits about 50/50:");
Console.WriteLine();

List<string> crowd = [];
foreach (string name in Crowd)
{
    TargetingContext member = new() { UserId = name, Groups = [] };
    bool on = await features.IsEnabledAsync("HalfOnStable", member, CancellationToken.None);
    crowd.Add($"{name} {(on ? "ON " : "OFF")}");
}

Console.WriteLine($"      {string.Join("   ", crowd)}");
Console.WriteLine();

static async Task<string> RowAsync(IVariantFeatureManager features, string flag, TargetingContext who)
{
    List<string> answers = [];
    for (int i = 0; i < Checks; i++)
    {
        bool on = await features.IsEnabledAsync(flag, who, CancellationToken.None);
        answers.Add(on ? "ON " : "OFF");
    }

    return string.Join("  ", answers);
}
