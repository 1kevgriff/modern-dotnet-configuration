using System.Collections;
using Microsoft.Extensions.Configuration;

// d05 — App Service connection-string prefixes.
//
//   A '*CONNSTR_{KEY}' environment variable is rewritten into the ConnectionStrings
//   section, and most of them also generate a '{KEY}_ProviderName' entry.
//   .NET 9 recognized four of these prefixes. .NET 10 recognizes eleven.

// The four that have always worked, then the seven added in .NET 10.
Prefix[] prefixes =
[
    new("CUSTOMCONNSTR_", IsNew: false),
    new("MYSQLCONNSTR_", IsNew: false),
    new("SQLCONNSTR_", IsNew: false),
    new("SQLAZURECONNSTR_", IsNew: false),
    new("POSTGRESQLCONNSTR_", IsNew: true),
    new("DOCDBCONNSTR_", IsNew: true),
    new("REDISCACHECONNSTR_", IsNew: true),
    new("SERVICEBUSCONNSTR_", IsNew: true),
    new("EVENTHUBCONNSTR_", IsNew: true),
    new("NOTIFICATIONHUBCONNSTR_", IsNew: true),
    new("APIHUBCONNSTR_", IsNew: true),
];

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

Console.WriteLine();
Console.WriteLine("  d05 — *CONNSTR_ environment variables become connection strings");
Console.WriteLine();

var found = FindConnectionStringVariables().OrderBy(v => v.Name, StringComparer.Ordinal).ToList();

if (found.Count == 0)
{
    Console.WriteLine("  No *CONNSTR_* environment variables are set. See README.md.");
    Console.WriteLine();
    return;
}

foreach (var (name, prefix, key) in found)
{
    var tag = prefix.IsNew ? "new in .NET 10" : "recognized since .NET Core 1.0";

    Console.WriteLine($"  {name.PadRight(28)}  ({tag})");
    Console.WriteLine($"      GetConnectionString(\"{key}\")".PadRight(50) + config.GetConnectionString(key));
    Console.WriteLine($"      ConnectionStrings:{key}_ProviderName".PadRight(50) + (config[$"ConnectionStrings:{key}_ProviderName"] ?? "(none)"));
    Console.WriteLine();
}

Console.WriteLine("  The eleven prefixes (* = added in .NET 10):");
Console.WriteLine();

foreach (var line in prefixes.Chunk(3))
{
    Console.WriteLine(("    " + string.Join("  ", line.Select(p => $"{(p.IsNew ? "*" : " ")}{p.Name}".PadRight(26)))).TrimEnd());
}

Console.WriteLine();

// Every '*CONNSTR_{KEY}' variable in the environment, matched longest prefix first.
IEnumerable<(string Name, Prefix Prefix, string Key)> FindConnectionStringVariables()
{
    foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
    {
        var name = (string)entry.Key;

        var match = prefixes
            .Where(p => name.StartsWith(p.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Name.Length)
            .FirstOrDefault();

        if (match is not null && name.Length > match.Name.Length)
        {
            yield return (name, match, name[match.Name.Length..]);
        }
    }
}

internal sealed record Prefix(string Name, bool IsNew);
