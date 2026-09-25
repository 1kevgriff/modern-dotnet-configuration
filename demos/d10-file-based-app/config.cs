#:package Microsoft.Extensions.Configuration.UserSecrets@10.0.11
#:property TreatWarningsAsErrors=true
#:property EnforceCodeStyleInBuild=true
#:property AnalysisLevel=latest-recommended

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;

var app = Assembly.GetExecutingAssembly();

var config = new ConfigurationBuilder()
    .AddUserSecrets(app, optional: true)
    .Build();

Console.WriteLine();
Console.WriteLine("  d10 — one .cs file, no .csproj, real configuration");
Console.WriteLine();
Console.WriteLine($"  UserSecretsId   {app.GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId ?? "(none)"}");
Console.WriteLine($"  ApiKey          {config["ApiKey"] ?? "(not set)"}");
Console.WriteLine();
