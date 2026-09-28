using D31.FlagVariants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

string user = configuration["user"] ?? "marsha";

ServiceCollection services = new();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging();
services.AddFeatureManagement().AddFeatureFilter<ContextualTargetingFilter>();

await using ServiceProvider provider = services.BuildServiceProvider();
IVariantFeatureManager features = provider.GetRequiredService<IVariantFeatureManager>();

TargetingContext who = new() { UserId = user, Groups = [] };

Variant variant = await features.GetVariantAsync("CheckoutLayout", who, CancellationToken.None);

// variant.Configuration is an IConfigurationSection. Bind it like anything else.
CheckoutLayoutSettings layout = variant.Configuration.Get<CheckoutLayoutSettings>()
    ?? throw new InvalidOperationException("CheckoutLayout variant carried no configuration.");

bool enabled = await features.IsEnabledAsync("CheckoutLayout", who, CancellationToken.None);

Console.WriteLine();
Console.WriteLine("  D31 - a variant returns a configuration section, not a bool");
Console.WriteLine();
Console.WriteLine($"    user: {user}        flag: CheckoutLayout");
Console.WriteLine();
Console.WriteLine($"      variant                  {variant.Name}");
Console.WriteLine();
Console.WriteLine("      CheckoutLayoutSettings   bound from variant.Configuration");
Console.WriteLine($"        ButtonSize             {layout.ButtonSize}");
Console.WriteLine($"        Columns                {layout.Columns}");
Console.WriteLine($"        ShowUpsell             {layout.ShowUpsell}");
Console.WriteLine();
Console.WriteLine($"      IsEnabledAsync()         {enabled}      status_override on the variant");
Console.WriteLine();
