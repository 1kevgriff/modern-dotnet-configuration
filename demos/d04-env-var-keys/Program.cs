using System.Collections;
using Microsoft.Extensions.Configuration;

// d04 — environment variables become configuration keys.
//
//   ':' is not portable in an environment variable name. '__' is, and the
//   environment-variable provider translates it to ':' on the way in.
//   A prefix passed to the provider is stripped off the resulting key.

const string AppPrefix = "MYAPP_";
const string Section = "Weather";

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()                     // Weather__ApiBaseUrl        -> Weather:ApiBaseUrl
    .AddEnvironmentVariables(prefix: AppPrefix)    // MYAPP_Weather__TimeoutSeconds -> Weather:TimeoutSeconds
    .Build();

Console.WriteLine();
Console.WriteLine("  d04 — environment variables -> configuration keys");
Console.WriteLine();

var rows = Translate().OrderBy(r => r.Key, StringComparer.Ordinal).ToList();

if (rows.Count == 0)
{
    Console.WriteLine($"  No {Section}__* environment variables are set. See README.md.");
    Console.WriteLine();
    return;
}

Console.WriteLine($"  {"ENVIRONMENT VARIABLE",-32}  {"CONFIGURATION KEY",-26}  VALUE");
Console.WriteLine($"  {new string('-', 32)}  {new string('-', 26)}  {new string('-', 26)}");

foreach (var row in rows)
{
    Console.WriteLine($"  {row.Variable,-32}  {row.Key,-26}  {config[row.Key] ?? "(unresolved)"}");
}

Console.WriteLine();

// One variable, two providers: only the prefixed one strips the prefix off the key.
foreach (var row in rows.Where(r => r.Prefixed))
{
    Console.WriteLine($"  One variable, two providers:   {row.Variable}");
    Console.WriteLine();
    var prefixed = $"AddEnvironmentVariables(\"{AppPrefix}\")";

    Console.WriteLine($"    {"AddEnvironmentVariables()",-36}  ->  {AppPrefix + row.Key,-30}  {config[AppPrefix + row.Key]}");
    Console.WriteLine($"    {prefixed,-36}  ->  {row.Key,-30}  {config[row.Key]}   <- prefix stripped");
}

Console.WriteLine();

// Every 'Weather__*' variable in the environment, and the key the provider makes from it.
IEnumerable<(string Variable, string Key, bool Prefixed)> Translate()
{
    foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
    {
        var name = (string)entry.Key;

        var prefixed = name.StartsWith(AppPrefix, StringComparison.Ordinal);
        var bare = prefixed ? name[AppPrefix.Length..] : name;

        if (!bare.StartsWith(Section + "__", StringComparison.Ordinal))
        {
            continue;
        }

        yield return (name, bare.Replace("__", ":", StringComparison.Ordinal), prefixed);
    }
}
