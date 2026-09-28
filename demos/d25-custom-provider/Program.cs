using D25.CustomProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

const string EnvFile = "settings.env";

// Base path is the working directory on purpose: dotnet run puts it at the project folder, so
// these are the files you can edit on stage rather than copies under bin/. A published app wants
// AppContext.BaseDirectory instead - see d26.
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddDotEnvFile(EnvFile, reloadOnChange: true)
    .Build();

// The root owns the providers, and one of ours holds a file watcher.
using var configurationLifetime = (IDisposable)configuration;

var services = new ServiceCollection();
services.Configure<GreetingOptions>(configuration.GetSection(GreetingOptions.SectionName));

using var provider = services.BuildServiceProvider();
var monitor = provider.GetRequiredService<IOptionsMonitor<GreetingOptions>>();

Console.WriteLine();
Console.WriteLine("PROVIDERS");
Console.WriteLine();
foreach (var configurationProvider in configuration.Providers)
{
    Console.WriteLine($"    {configurationProvider}");
}

Console.WriteLine();
Console.WriteLine("GetDebugView()   key = value   (the provider that supplied it)");
Console.WriteLine();
Console.WriteLine(configuration.GetDebugView());

// OnChange returns an IDisposable. Dropping it on the floor leaks the subscription.
var reloaded = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
using var subscription = monitor.OnChange(options => reloaded.TrySetResult(options.Message));

Console.WriteLine("LIVE EDIT   (this demo rewrites settings.env for you)");
Console.WriteLine();
Console.WriteLine($"    before   {monitor.CurrentValue.Message}");

WriteEnvFile($"Reloaded at {TimeProvider.System.GetLocalNow():HH:mm:ss}");

var message = await reloaded.Task.WaitAsync(TimeSpan.FromSeconds(10));
Console.WriteLine($"    after    {message}");

Console.WriteLine();
Console.WriteLine("settings.env is now edited - run ./reset.ps1 to put it back.");
Console.WriteLine();

static void WriteEnvFile(string message)
{
    var contents = $"""
        # settings.env - read by a hand-written provider, not by anything built in.
        # '__' becomes ':', so Greeting__Message sets Greeting:Message.
        Greeting__Message={message}
        """;

    File.WriteAllText(EnvFile, contents + Environment.NewLine);
}
