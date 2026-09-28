using D27.ReloadOnChange;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

const string EnvVarName = "GREETING__FROMENV";
const string FileV1 = "v1 - from appsettings.json";
const string FileV2 = "v2 - after the file was edited";
const string EnvV1 = "v1 - from the environment variable";
const string EnvV2 = "v2 - after the variable was changed";
const string Rule = "  ------------------------------------------------------------";

// Set before the host is built, so the environment provider picks it up at startup.
Environment.SetEnvironmentVariable(EnvVarName, EnvV1);

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<GreetingOptions>(
    builder.Configuration.GetSection(GreetingOptions.SectionName));

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using var host = builder.Build();

var monitor = host.Services.GetRequiredService<IOptionsMonitor<GreetingOptions>>();
var timeProvider = host.Services.GetRequiredService<TimeProvider>();
var environment = host.Services.GetRequiredService<IHostEnvironment>();
var settingsPath = Path.Combine(environment.ContentRootPath, "appsettings.json");
var original = await File.ReadAllTextAsync(settingsPath, cts.Token);

// IOptionsMonitor.OnChange returns a registration that has to be disposed.
var fires = 0;
using var subscription = monitor.OnChange(_ => Interlocked.Increment(ref fires));

Console.WriteLine();
Console.WriteLine("  d27 - files reload, environment variables do not");
Console.WriteLine(Rule);
Console.WriteLine();
Console.WriteLine("  STEP 1    as the app started");
Console.WriteLine();
Console.WriteLine($"      Greeting:FromFile    {monitor.CurrentValue.FromFile}");
Console.WriteLine($"      Greeting:FromEnv     {monitor.CurrentValue.FromEnv}");
Console.WriteLine();

try
{
    // ---- The file edit ----------------------------------------------------
    var startedAt = timeProvider.GetTimestamp();
    await File.WriteAllTextAsync(settingsPath, original.Replace(FileV1, FileV2, StringComparison.Ordinal), cts.Token);

    var reloaded = await WaitUntilAsync(
        () => monitor.CurrentValue.FromFile == FileV2, TimeSpan.FromSeconds(10));
    var reloadTook = timeProvider.GetElapsedTime(startedAt);

    // Let a second watcher event (write + metadata) land before counting.
    await Task.Delay(TimeSpan.FromMilliseconds(750), timeProvider, cts.Token);

    Console.WriteLine("  STEP 2    appsettings.json edited on disk");
    Console.WriteLine();
    Console.WriteLine($"      Greeting:FromFile    {monitor.CurrentValue.FromFile}");
    Console.WriteLine($"                           {(reloaded ? $"CHANGED after {reloadTook.TotalSeconds:F1}s - no restart" : "TIMED OUT waiting for the watcher")}");
    Console.WriteLine();
    Console.WriteLine($"      OnChange fired {Volatile.Read(ref fires)} time(s) for that one edit.");
    Console.WriteLine();

    // ---- The environment variable change ----------------------------------
    // Wait at least as long as the file took, so the comparison is honest.
    var envWait = TimeSpan.FromSeconds(Math.Max(2, reloadTook.TotalSeconds));
    Environment.SetEnvironmentVariable(EnvVarName, EnvV2);

    var envChanged = await WaitUntilAsync(
        () => monitor.CurrentValue.FromEnv == EnvV2, envWait);

    Console.WriteLine($"  STEP 3    GREETING__FROMENV changed, waited {envWait.TotalSeconds:F1}s");
    Console.WriteLine();
    Console.WriteLine($"      Greeting:FromEnv     {monitor.CurrentValue.FromEnv}");
    Console.WriteLine($"                           {(envChanged ? "CHANGED" : "UNCHANGED - still the startup value")}");
    Console.WriteLine();
}
finally
{
    await File.WriteAllTextAsync(settingsPath, original, CancellationToken.None);
}

Console.WriteLine(Rule);
Console.WriteLine("  Files reload. Environment variables are read once, at startup.");
Console.WriteLine();

async Task<bool> WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
{
    var startedWaitingAt = timeProvider.GetTimestamp();

    while (timeProvider.GetElapsedTime(startedWaitingAt) < timeout)
    {
        if (predicate())
        {
            return true;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(50), timeProvider, cts.Token);
    }

    return predicate();
}
