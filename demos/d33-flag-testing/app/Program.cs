using Microsoft.FeatureManagement;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddFeatureManagement();

WebApplication app = builder.Build();

// One endpoint, one flag, two branches. Both branches must be tested.
app.MapGet("/checkout", async (IVariantFeatureManager features, CancellationToken cancellationToken) =>
    await features.IsEnabledAsync("NewCheckout", cancellationToken)
        ? TypedResults.Ok("new checkout")
        : TypedResults.Ok("legacy checkout"));

await app.RunAsync();

/// <summary>Exposed so WebApplicationFactory can find the entry point of a top-level-statements app.</summary>
public sealed partial class Program;
