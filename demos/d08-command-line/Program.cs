using Microsoft.Extensions.Configuration;

// d08 — the command line, and short aliases for long keys.
//
//   Three accepted spellings:  --Key value    /Key value    Key=value
//   A switch mapping gives a key a short alias:  -t 5

const string Key = "Weather:TimeoutSeconds";

var switchMappings = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["-t"] = Key,
    ["--timeout"] = Key,
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?> { [Key] = "30" })   // stands in for appsettings.json
    .AddCommandLine(args, switchMappings)                                     // last provider wins
    .Build();

Console.WriteLine();
Console.WriteLine("  d08 — command line arguments");
Console.WriteLine();
Console.WriteLine($"  args              {(args.Length == 0 ? "(none)" : string.Join(' ', args))}");
Console.WriteLine();
Console.WriteLine("  switch mappings   -t         ->  " + Key);
Console.WriteLine("                    --timeout  ->  " + Key);
Console.WriteLine();
Console.WriteLine($"  {Key}    =  {config[Key]}          (default 30)");
Console.WriteLine();
