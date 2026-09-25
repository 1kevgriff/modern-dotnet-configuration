using D15.RecursiveValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();

// --Recursive true swaps in a class that differs by exactly two attributes.
// The config, the nested types, and the validator call are all identical.
var recursive = builder.Configuration.GetValue<bool>("Recursive");
var section = builder.Configuration.GetSection(FlatAppOptions.SectionName);

if (recursive)
{
    builder.Services.AddOptions<RecursiveAppOptions>()
        .Bind(section)
        .ValidateDataAnnotations()
        .ValidateOnStart();
}
else
{
    builder.Services.AddOptions<FlatAppOptions>()
        .Bind(section)
        .ValidateDataAnnotations()
        .ValidateOnStart();
}

const string Rule = "  --------------------------------------------------------------";

Console.WriteLine();
Console.WriteLine("  d15 - nested objects and collections are not validated by default");
Console.WriteLine(Rule);
Console.WriteLine();
Console.WriteLine("  Same bad config in both runs:");
Console.WriteLine();
Console.WriteLine("      App:Database:ConnectionString         \"\"      [Required]");
Console.WriteLine("      App:Database:CommandTimeoutSeconds    999     [Range(1, 300)]");
Console.WriteLine("      App:Servers:0:Port                  70000     [Range(1, 65535)]");
Console.WriteLine();
Console.WriteLine(recursive
    ? "  Options class:   RecursiveAppOptions   [ValidateObjectMembers] + [ValidateEnumeratedItems]"
    : "  Options class:   FlatAppOptions        no recursion attributes");
Console.WriteLine(Rule);
Console.WriteLine();

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using var host = builder.Build();

try
{
    await host.StartAsync(cts.Token);
    await host.StopAsync(cts.Token);

    Console.WriteLine("  STARTED.  Three broken values, zero complaints.");
}
catch (OptionsValidationException ex)
{
    Console.WriteLine($"  REFUSED TO START.  {ex.Failures.Count()} failures:");
    Console.WriteLine();

    foreach (var failure in ex.Failures)
    {
        Console.WriteLine($"      * {failure}");
    }
}

Console.WriteLine();
