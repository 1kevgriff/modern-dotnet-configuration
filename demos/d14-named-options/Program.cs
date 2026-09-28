using D14.NamedOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var endpoints = builder.Configuration.GetSection(EndpointOptions.SectionName);

builder.Services.Configure<EndpointOptions>(EndpointOptions.Primary, endpoints.GetSection("Primary"));
builder.Services.Configure<EndpointOptions>(EndpointOptions.Secondary, endpoints.GetSection("Secondary"));

builder.Services.AddSingleton<Router>();

using var host = builder.Build();

var router = host.Services.GetRequiredService<Router>();

Console.WriteLine();
Console.WriteLine("  d14 — one options class, several configured instances");
Console.WriteLine();
Console.WriteLine();
Write("""monitor.Get("primary")""", router.Primary);
Write("""monitor.Get("secondary")""", router.Secondary);
Write("monitor.Get(Options.DefaultName)", router.Unnamed);
Console.WriteLine();
Console.WriteLine("  One class, one injection. No EndpointOptionsPrimary, no EndpointOptionsSecondary.");
Console.WriteLine();

static void Write(string call, EndpointOptions options)
{
    var url = string.IsNullOrEmpty(options.BaseUrl) ? "(never configured)" : options.BaseUrl;

    Console.WriteLine($"  {call,-34}{url,-32}{options.TimeoutSeconds,3} s");
    Console.WriteLine();
}
