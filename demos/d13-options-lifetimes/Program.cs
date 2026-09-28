using System.Globalization;
using D13.OptionsLifetimes;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Stage noise control: only this demo's own log line gets through.
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.SingleLine = true);
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddFilter("d13", LogLevel.Information);

builder.Services.Configure<WeatherOptions>(
    builder.Configuration.GetSection(WeatherOptions.SectionName));

// DON'T: registered here only so the bug is visible next to the correct answers.
builder.Services.AddSingleton<CachedTimeout>();

builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.MapGet("/options", (
    IOptions<WeatherOptions> singleton,
    IOptionsSnapshot<WeatherOptions> scoped,
    IOptionsMonitor<WeatherOptions> monitor,
    CachedTimeout cached,
    TimeProvider time) => Results.Text($"""

      d13 — three interfaces, one moment in time      {time.GetLocalNow().ToString("HH:mm:ss", CultureInfo.InvariantCulture)}


      IOptions<T>            {singleton.Value.TimeoutSeconds,4} s     singleton    bound once at startup, never again

      IOptionsSnapshot<T>    {scoped.Value.TimeoutSeconds,4} s     scoped       re-bound once per request

      IOptionsMonitor<T>     {monitor.CurrentValue.TimeoutSeconds,4} s     singleton    re-bound when appsettings.json changes


      DON'T: cached field    {cached.TimeoutSeconds,4} s     singleton    CurrentValue read once — a monitor that stopped moving


      edit appsettings.json, save, then curl again

    """));

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("d13");
var weatherMonitor = app.Services.GetRequiredService<IOptionsMonitor<WeatherOptions>>();

// OnChange returns the registration. Hold it and dispose it, or you leak the subscription.
using var onChange = weatherMonitor.OnChange(options => Log.OptionsChanged(logger, options.TimeoutSeconds));

Console.WriteLine();
Console.WriteLine("  d13 listening on http://localhost:5000/options");
Console.WriteLine();

await app.RunAsync("http://localhost:5000").ConfigureAwait(false);
