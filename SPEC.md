# Modern .NET Configuration — Content Spec

Working spec for the talk. Target runtime: **.NET 10 (LTS)** / **C# 14**.
Every code sample here is meant to be lifted into `/demos` more or less as-is.

Sources: [Configuration in .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration),
[Configuration providers](https://learn.microsoft.com/dotnet/core/extensions/configuration-providers),
[Configuration in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0),
[Options pattern](https://learn.microsoft.com/dotnet/core/extensions/options).

---

## 0. The session

| | |
| --- | --- |
| **Event** | [Cloud & AI Summit](https://www.cloudandaisummit.com/) |
| **Slot** | Wednesday, September 30 · 10:15 AM – 11:45 AM (**90 minutes**) |
| **Room** | Discovery Ballroom C |
| **Track / level** | Pro Code · Intermediate |

**Abstract as submitted:**

> Remember the good ol' days when configuration meant hardcoding a few constants and commenting out
> everything but PRODUCTION before a commit? Life was simple—and dangerous. Thankfully, things have
> evolved. Since the introduction of the .NET Generic Host, we've gained a powerful and flexible
> configuration system that works across all types of applications. But with great power comes great
> complexity. In this talk, we'll dive deep into the .NET configuration model. We'll explore when to
> use configuration files, environment variables, and cloud-based solutions like Azure App
> Configuration. We'll also cover common pitfalls, real-world patterns, and how to choose the right
> configuration strategy for your app. Whether you're building cloud-native microservices or desktop
> apps, you'll leave with a clear understanding of how to wrangle configuration in a clean, secure,
> and scalable way.

Promises the abstract makes, and where they're paid off:

| Promise | Section |
| --- | --- |
| Why externalize configuration at all | §1.1 (five reasons) |
| "the good ol' days" → Generic Host | §1.2 |
| Deep dive on the configuration model | §2, §3 |
| When to use files vs env vars vs cloud | §1.4 spine, §4, matrix in §7 |
| Cloud-based solutions like Azure App Configuration | §4.6, §6 |
| Common pitfalls | §1.5 failure modes, §8 (reload), §10 (anti-patterns) |
| Real-world patterns | §5 (options), §4.6 (sentinel key), §6.6 (flag debt) |
| Choosing a strategy | §7 |
| **"cloud-native microservices *or desktop apps*"** | §4.11 |

Not in the abstract but now ~25% of the talk: **feature flags** (§6). If the abstract is ever
resubmitted, add a clause — the current text only implies them via "Azure App Configuration."

---

## 1. Opening: why, then how

The talk opens cold on a demo, then two questions. Everything after §1 answers the second one.

### 1.0 Cold open (before any slide)

Run Demo 01 with no setup and no explanation: an app printing a value that is *wrong*, and a
`GetDebugView()` dump showing an unexpected provider won. Let the room sit with it for fifteen
seconds. Don't explain it yet — say "we'll come back to this" and move to §1.1.

Why open here: "why externalize configuration" and "how do we load it" are both expository. Two
explanatory beats back to back is a flat first ten minutes. Opening on a small mystery buys attention
for the parts that are genuinely just exposition, and §2 pays it off.

### 1.1 Why break configuration out of the app at all?

Five answers. The first three are the obvious ones; the last two are the reasons that actually hold
up in 2026.

1. **No hard-coding.** A value baked into a compiled assembly changes only by editing code,
   rebuilding, retesting, and redeploying. The value and the logic have completely different rates of
   change, so they shouldn't share a lifecycle.
2. **Values that vary by environment.** The same binary talks to a local database, a staging API, and
   a production endpoint. If the *code* differs per environment, you aren't testing the thing you
   ship.
3. **Changing a value on the fly.** Some values must move while the app is running — a timeout, a log
   level, a feature toggle — with no deployment window and no dropped traffic.
4. **Trust.** A credential compiled into an assembly ships everywhere the assembly ships: every
   laptop, every build artifact, every container layer, every decompiler. This isn't about rate of
   change, it's about *blast radius*, and it's arguably the number one reason configuration is
   externalized today.
5. **Ownership.** The person who needs to change the value is frequently not the person who can
   compile the code. Configuration is the seam between dev and ops — an SRE raising a timeout at 3am
   shouldn't need your build pipeline, your PR review, or you.

Reasons 1–2 were solved by `web.config` twenty years ago. **Reasons 3, 4, and 5 are why this talk
exists**, and they're the three that cost something: a value that lives outside the binary can be
missing, malformed, stale, or arriving from a source you forgot about. The rest of the talk is about
paying that cost deliberately.

### 1.2 The cold open, part two: how we got here

Three slides, about three minutes, scoring each era against those five reasons:

1. **Constants and `#if DEBUG`.** Hardcoded values, commented-out blocks, a pre-commit ritual.
   Simple, and dangerous — the failure mode was shipping the wrong uncommented line. Fails #1
   outright, and #4 catastrophically.
2. **`web.config` / `app.config` and `ConfigurationManager.AppSettings`.** XML, static, a single
   string-keyed bag, no reload, no layering, framework-only, and transforms (`Web.Release.config`)
   that ran at *build* time. Wins #1 and #2 — but you built one artifact per environment, and #3 was
   impossible.
3. **The .NET Generic Host (2018, .NET Core 2.1) and `IConfiguration`.** Provider chain, layering,
   binding, DI, reload, and *the same model for a web app, a worker, a console tool, and a WinForms
   app*. One artifact, many environments — all five reasons, finally.

The through-line: **we traded a build-time decision for a runtime one.** Everything hard about modern
configuration comes from that trade, and §6.8 shows feature flags making the same trade one level up.

### 1.3 The thesis: configuration for everything, flags for everything

State the position plainly, because the rest of the talk is its defense:

> **If an operational value can differ by environment, deployment, or time, configure it.
> If a code path must be switchable without a deploy, flag it.**

Note the word *operational*. This is deliberately not "anything that might ever change" — user
preferences, tenant data and business rules all vary without belonging in application configuration,
and §4.11 says so explicitly about desktop. The claim is about the values that operate the app.

Within that scope the bar is still anticipatory, not reactive. Moving a value into configuration
later is a code change under time pressure; putting it there now costs one line. Same for flags:
adding one before you ship is cheap, adding one during an incident is not.

Two objections to handle out loud, both fair:

- **"Then everything is configurable and nothing is knowable."** True if you stop at externalizing.
  It's why §5's binding and `ValidateOnStart` exist — a configurable value that's validated at
  startup is *more* knowable than a constant buried in code, because it's declared, typed, and
  checked in one place.
- **"That's a lot of flags."** Yes, and every one is debt. §6.6 is the flag lifecycle discipline that
  makes this position survivable rather than reckless.

### 1.4 How do we manage and load configuration in .NET?

The second opening question, and the spine of the talk. Three stages, in increasing order of both
capability and operational cost:

> **supporting local dev → supporting deployment → supporting shared**

("Shared" rather than "scale" on purpose. Stage three is about configuration that outlives or spans a
single deployment unit — many apps, many instances, values that change without a deploy. Scale is one
reason you get there, not the definition.)

| Stage | The problem it solves | Primary sources | Spec | Projects |
| --- | --- | --- | --- | --- |
| **Local dev** | A developer clones the repo and it runs. Secrets never touch git. | `appsettings.json`, `appsettings.Development.json`, user secrets, `launchSettings.json` | §4.1, §4.4 | 01–04, 07, 09, 10 |
| **Deployment** | One artifact, many environments. The platform supplies the values. | Environment variables, command line, key-per-file mounts, Key Vault | §4.2, §4.3, §4.5, §4.7 | 05, 08, 18, 23 |
| **Shared** | Many apps and instances; values that change without a deploy. | Azure App Configuration — labels, refresh, sentinel keys, feature flags | §4.6, §6 | 19–22, 28–32 |

**Each stage is introduced by the failure that forces you to the next one.** This is what gives the
spine motion instead of making it a list:

| Stage | Its signature failure | Which pushes you to |
| --- | --- | --- |
| Local dev | *"Works on my machine"* — `launchSettings.json` overriding the env var you set, secrets that only exist on one laptop | Deployment |
| Deployment | *"The value didn't make it"* — a provider you forgot was winning, a key whose shape was wrong, a secret cached until restart | Shared |
| Shared | *"It changed, but not everywhere at once"* — stale caches, half-applied multi-key edits, an idle instance that never refreshed | Discipline: sentinel keys, validation, flag hygiene |

Two things to say with this slide up:

- **The stages are cumulative, not alternatives.** A stage-three app still has `appsettings.json` for
  defaults and still takes per-instance values from environment variables. You add layers; you don't
  replace them.
- **Know where to stop.** Stage three earns its complexity when more than one app shares values, or
  when something genuinely must change without a deploy. §7 is the framework for deciding.

### 1.5 The four failure modes (recurring motif)

Not a section — a label to hang on things as they come up. Whatever stage you're in, almost every
configuration bug is one of four things:

1. **Order** — a provider you forgot about is winning.
2. **Shape** — the key you set doesn't produce the key the binder is looking for.
3. **Lifetime** — you cached a value that was supposed to change, or didn't cache one that wasn't.
4. **Trust** — a secret is sitting in a file that ships with the app.

Name them out loud during the demos: order in Demo 02, shape in Demo 04, lifetime in Demo 13, trust
in Demo 09. By the anti-patterns slide (§10) the audience should be calling them before you do.

### 1.6 The mental model in one sentence

Configuration is not "the appsettings.json file." It's an ordered chain of key/value providers
collapsed into one flat, case-insensitive dictionary of strings, plus a binder that projects slices
of that dictionary onto typed objects. §2 unpacks it — and explains the cold open.

---

## 2. The mental model

- Configuration is a flat `IDictionary<string, string?>` with `:` as the hierarchy delimiter.
  `{"Db": {"Timeout": 30}}` is the single key `Db:Timeout` with the **string** value `"30"`.
- **Keys are case-insensitive.** `DB:TIMEOUT` == `db:timeout`.
- **Every scalar is text — or null.** The contract is `string?`. Every `int`, `bool`, `TimeSpan`
  and `Uri` is a binder conversion. Since .NET 10 a real `null` survives as `null` rather than
  being flattened to `""` (§9), so "always a string" is no longer quite true.
- **Last provider wins.** `IConfigurationBuilder.Add` appends; reads walk the provider list in
  reverse and take the first hit.
- Array elements are keys too: `Servers:0:Host`, `Servers:1:Host`.
- A duplicate key **inside one file provider** throws `FormatException`. A duplicate key **across
  providers** is the whole point of the system.

```csharp
// Everything below reads the same key.
config["Db:Timeout"];
config.GetSection("Db")["Timeout"];
config.GetSection("Db").GetValue<int>("Timeout");
config.GetValue<int>("Db:Timeout", defaultValue: 30);
```

### 2.1 The single most useful diagnostic

`GetDebugView()` prints every key, its effective value, **and the provider that supplied it**.
This is the "show, don't tell" moment of the talk.

```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Redact anything that smells like a secret before printing.
    var dump = ((IConfigurationRoot)app.Configuration).GetDebugView(ctx =>
        ctx.Key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
        ctx.Key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
        ctx.Key.Contains("key", StringComparison.OrdinalIgnoreCase)
            ? "***"
            : ctx.Value);

    app.Logger.LogInformation("Configuration:\n{Dump}", dump);
}
```

Output shape:

```text
Db:
  Timeout=30 (JsonConfigurationProvider for 'appsettings.json' (Optional))
  ConnectionString=*** (EnvironmentVariablesConfigurationProvider Prefix: '')
```

The `processValue` overload takes a `Func<ConfigurationDebugViewContext, string>`; the context
carries `Path`, `Key`, `Value`, and `ConfigurationProvider`.

To enumerate the chain directly:

```csharp
foreach (var provider in ((IConfigurationRoot)app.Configuration).Providers)
{
    Console.WriteLine(provider);
}
```

---

## 3. Default provider order (.NET 10)

### 3.1 App configuration

`WebApplication.CreateBuilder(args)` and `Host.CreateApplicationBuilder(args)`, highest priority first:

| # | Source | Notes |
| --- | --- | --- |
| 1 | Host / chained configuration | `ChainedConfigurationProvider` — added **last**, so host values win |
| 2 | Command-line arguments | The command-line provider runs twice; this is the second pass |
| 3 | Environment variables (unprefixed) | `DOTNET_` / `ASPNETCORE_` are host-level and read earlier |
| 4 | User secrets | **`Development` environment only** |
| 5 | `{ApplicationName}.settings.{Environment}.json` | Almost nobody knows these exist |
| 6 | `{ApplicationName}.settings.json` | |
| 7 | `appsettings.{Environment}.json` | e.g. `appsettings.Production.json` |
| 8 | `appsettings.json` | |

> **`{ApplicationName}.settings.json` is not a non-web thing.** An earlier draft of this spec said
> those two files only appeared under `Host.CreateApplicationBuilder`. That is wrong.
> `WebApplication.CreateBuilder` constructs a `HostApplicationBuilder` internally, so it inherits
> them too. Verified on .NET SDK 10.0.303 — a file-based app `web.cs` really does probe
> `web.settings.json` and `web.settings.Development.json`. The known exception is
> `WebApplication.CreateSlimBuilder`, whose copied slim defaults omit them.
> Say it out loud when the provider dump puts them on screen, because the audience will see
> providers the slide doesn't list.

Verified dump, `WebApplication.CreateBuilder`, `--environment Development` plus one CLI arg
(add order — **last wins**, so read it bottom-up for priority):

```text
MemoryConfigurationProvider
EnvironmentVariablesConfigurationProvider Prefix: 'ASPNETCORE_'
MemoryConfigurationProvider
EnvironmentVariablesConfigurationProvider Prefix: 'DOTNET_'
CommandLineConfigurationProvider                         <- host pass
JsonConfigurationProvider for 'appsettings.json'
JsonConfigurationProvider for 'appsettings.Development.json'
JsonConfigurationProvider for 'web.settings.json'
JsonConfigurationProvider for 'web.settings.Development.json'
JsonConfigurationProvider for 'secrets.json'
EnvironmentVariablesConfigurationProvider
CommandLineConfigurationProvider                         <- app pass
ChainedConfigurationProvider                             <- highest priority
```

### 3.2 Host configuration

Read *first*, and it determines `EnvironmentName` — which decides which `appsettings.{Environment}.json`
even gets loaded. For `WebApplicationBuilder`:

1. Command-line arguments
2. `DOTNET_`-prefixed environment variables
3. `ASPNETCORE_`-prefixed environment variables

The command-line provider is deliberately used **twice** (start and end) so `--environment Staging`
can influence which files load and still win the final read.

> Talk beat: this is why `ASPNETCORE_ENVIRONMENT` is not "just another setting." It's the input
> that picks the rest of your inputs.

### 3.3 Provider catalog

| Provider | Package | In the default chain? |
| --- | --- | --- |
| JSON | `Microsoft.Extensions.Configuration.Json` | Yes |
| Environment variables | `Microsoft.Extensions.Configuration.EnvironmentVariables` | Yes |
| Command line | `Microsoft.Extensions.Configuration.CommandLine` | Yes |
| User secrets | `Microsoft.Extensions.Configuration.UserSecrets` | Yes (Development) |
| INI | `Microsoft.Extensions.Configuration.Ini` | No |
| XML | `Microsoft.Extensions.Configuration.Xml` | No |
| Key-per-file | `Microsoft.Extensions.Configuration.KeyPerFile` | No |
| In-memory | `Microsoft.Extensions.Configuration` | No |
| Azure Key Vault | `Azure.Extensions.AspNetCore.Configuration.Secrets` + `Azure.Identity` | No |
| Azure App Configuration | `Microsoft.Extensions.Configuration.AzureAppConfiguration` | No |
| Azure App Config refresh middleware | `Microsoft.Azure.AppConfiguration.AspNetCore` | No |
| Custom | yours | No |

---

## 4. Providers, one at a time

### 4.1 `appsettings.json` and environments

```jsonc
// appsettings.json — safe defaults, committed
{
  "Weather": {
    "ApiBaseUrl": "https://api.example.com",
    "TimeoutSeconds": 30,
    "Retries": 3,
    "ApiKey": ""            // placeholder only — never a real value
  },
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=Demo;Trusted_Connection=True"
  }
}
```

```jsonc
// appsettings.Development.json — overrides only, committed
{
  "Weather": {
    "ApiBaseUrl": "https://localhost:7104",
    "TimeoutSeconds": 120
  }
}
```

Points to make:

- The environment file **merges over** the base file key by key. It is not a replacement.
- Arrays do **not** merge cleanly — index keys overlay individually, so `["a","b","c"]` overridden
  by `["x"]` yields `["x","b","c"]`. This surprises everyone.
  Prefer an object keyed by name, or replace the section deliberately.
- `EnvironmentName` is arbitrary. `Development` / `Staging` / `Production` are just the framework's
  well-known values; `appsettings.QA-East.json` works if `ASPNETCORE_ENVIRONMENT=QA-East`.
- Build Action `Content` + Copy `PreserveNewest`, or the file isn't next to the DLL at runtime.

Explicit control, clearing the defaults:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);
```

### 4.2 Environment variables

The workhorse in containers and every PaaS.

```bash
# ':' is not portable in env var names. '__' is, and maps to ':'.
export Weather__ApiBaseUrl="https://api.prod.example.com"
export Weather__TimeoutSeconds="10"
export ConnectionStrings__Default="Server=sql;Database=Demo;..."
```

```powershell
$env:Weather__ApiBaseUrl = "https://api.prod.example.com"
$env:Weather__TimeoutSeconds = "10"
```

Arrays use the index as a segment:

```bash
export Weather__AllowedOrigins__0="https://a.example.com"
export Weather__AllowedOrigins__1="https://b.example.com"
```

**Prefixes.** `AddEnvironmentVariables(prefix: "MYAPP_")` loads only matching variables and
*strips the prefix* from the key. Useful for isolating one app on a shared host.

```csharp
builder.Configuration.AddEnvironmentVariables(prefix: "MYAPP_");
// MYAPP_Weather__TimeoutSeconds  ->  Weather:TimeoutSeconds
```

`DOTNET_` and `ASPNETCORE_` are reserved for host settings — don't reuse them for app config.

**Connection-string prefixes (changed in .NET 10).** Certain prefixed variables are rewritten into
the `ConnectionStrings:` section — an App Service compatibility behavior. .NET 9 recognized four;
**.NET 10 recognizes eleven**:

| Prefix | Becomes | `_ProviderName` entry |
| --- | --- | --- |
| `CUSTOMCONNSTR_{KEY}` | `ConnectionStrings:{KEY}` | none |
| `MYSQLCONNSTR_{KEY}` | `ConnectionStrings:{KEY}` | `MySql.Data.MySqlClient` |
| `SQLCONNSTR_{KEY}` | `ConnectionStrings:{KEY}` | `System.Data.SqlClient` |
| `SQLAZURECONNSTR_{KEY}` | `ConnectionStrings:{KEY}` | `System.Data.SqlClient` |
| `POSTGRESQLCONNSTR_{KEY}` *(new in 10)* | `ConnectionStrings:{KEY}` | `Npgsql` |
| `DOCDBCONNSTR_{KEY}` *(new in 10)* | `ConnectionStrings:{KEY}` (Cosmos DB) | none |
| `REDISCACHECONNSTR_{KEY}` *(new in 10)* | `ConnectionStrings:{KEY}` | none |
| `SERVICEBUSCONNSTR_{KEY}` *(new in 10)* | `ConnectionStrings:{KEY}` | none |
| `EVENTHUBCONNSTR_{KEY}` *(new in 10)* | `ConnectionStrings:{KEY}` | none |
| `NOTIFICATIONHUBCONNSTR_{KEY}` *(new in 10)* | `ConnectionStrings:{KEY}` | none |
| `APIHUBCONNSTR_{KEY}` *(new in 10)* | `ConnectionStrings:{KEY}` | none |

Demo: set `POSTGRESQLCONNSTR_Default`, then show `Configuration.GetConnectionString("Default")`
resolving on .NET 10 where it wouldn't have on .NET 9.

**Gotcha:** `launchSettings.json` environment variables override machine and user environment
variables during local F5 / `dotnet run`, and `launchSettings.json` is a development-only file that
never deploys. This is the #1 "works on my machine" configuration story.

### 4.3 Command line

```bash
dotnet run --Weather:TimeoutSeconds 5
dotnet run /Weather:TimeoutSeconds 5
dotnet run Weather:TimeoutSeconds=5
```

Rules: the value follows `=` with no space, or the key carries a `--` / `/` prefix when separated by
a space. Don't mix both styles in one command.

Switch mappings give short aliases:

```csharp
var switchMappings = new Dictionary<string, string>
{
    ["-t"] = "Weather:TimeoutSeconds",
    ["--timeout"] = "Weather:TimeoutSeconds",
};

builder.Configuration.AddCommandLine(args, switchMappings);
// dotnet run -t 5
```

### 4.4 User secrets (development only)

Not encrypted. Not a vault. The single purpose is *keeping secrets out of the repo* on a dev box.

```bash
dotnet user-secrets init                       # adds <UserSecretsId> to the .csproj
dotnet user-secrets set "Weather:ApiKey" "dev-key-12345"
dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Password=..."
dotnet user-secrets list
dotnet user-secrets remove "Weather:ApiKey"
dotnet user-secrets clear
```

Bulk load:

```bash
cat ./secrets-input.json | dotnet user-secrets set     # Linux/macOS
type .\secrets-input.json | dotnet user-secrets set    # Windows
```

Storage location (don't write code against this):

- Windows — `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- Linux/macOS — `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <UserSecretsId>0000a1a1-b2b2-c3c3-d4d4-eeeeee555555</UserSecretsId>
</PropertyGroup>
```

Registered automatically by the default builders **only in `Development`**. It sits *above* the JSON
files and *below* environment variables and the command line.

**.NET 10 bonus — file-based apps.** `dotnet run app.cs` programs get a stable `UserSecretsId`
derived from a hash of the file path:

```bash
dotnet user-secrets set "ApiKey" "value" --file app.cs
dotnet user-secrets list --file app.cs
```

Nice five-line demo: a single-file app that reads configuration with no `.csproj` anywhere.

### 4.5 Azure Key Vault

Packages: `Azure.Extensions.AspNetCore.Configuration.Secrets`, `Azure.Identity`.

```csharp
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
}
```

- **Secret naming.** Key Vault forbids `:` in secret names. The default `KeyVaultSecretManager`
  maps `--` to the configuration delimiter, so the secret **`Weather--ApiKey`** becomes the key
  **`Weather:ApiKey`**.
- **Register it last** so it overrides the file-based providers it's meant to replace.
- **No reload by default.** Secrets are cached for the life of the process. Opt into polling:

```csharp
builder.Configuration.AddAzureKeyVault(
    vaultUri,
    new DefaultAzureCredential(),
    new AzureKeyVaultConfigurationOptions
    {
        ReloadInterval = TimeSpan.FromMinutes(30)   // default is null == never
    });
```

- **Expired secrets are still loaded** by default; disabled secrets never are. Filter with a custom
  manager:

```csharp
internal sealed class ActiveSecretsOnly : KeyVaultSecretManager
{
    public override bool Load(SecretProperties properties) =>
        properties.ExpiresOn is null || properties.ExpiresOn > DateTimeOffset.UtcNow;
}
```

- **Credentials.** `DefaultAzureCredential` is the dev-convenience chain (Azure CLI, Visual Studio,
  environment variables, managed identity). In production prefer `ManagedIdentityCredential`
  explicitly — narrower, and it fails fast instead of silently walking the chain. For a
  user-assigned identity, set `AZURE_CLIENT_ID` or
  `DefaultAzureCredentialOptions.ManagedIdentityClientId`.
- **Isolation.** One vault per app *and* per environment. Don't multiplex with name prefixes.

### 4.6 Azure App Configuration

The centralized, mostly-non-secret, changes-without-redeploy story. Packages:
`Microsoft.Extensions.Configuration.AzureAppConfiguration`, plus
`Microsoft.Azure.AppConfiguration.AspNetCore` for the refresh middleware.

```csharp
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

var endpoint = builder.Configuration["Endpoints:AppConfiguration"]
    ?? throw new InvalidOperationException("Endpoints:AppConfiguration is not configured.");

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(endpoint), new DefaultAzureCredential())
           // Load a slice, not the whole store.
           .Select("Weather:*", LabelFilter.Null)
           .Select("Weather:*", builder.Environment.EnvironmentName)   // env overlay
           .ConfigureRefresh(refresh =>
           {
               refresh.RegisterAll()                                   // reload all on any change
                      .SetRefreshInterval(TimeSpan.FromSeconds(60));   // default 30s
           })
           // Resolve Key Vault references stored in App Configuration.
           .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()))
           .UseFeatureFlags(ff => ff.SetRefreshInterval(TimeSpan.FromSeconds(30)));
});

builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();

var app = builder.Build();

// Early in the pipeline, or another middleware short-circuits before refresh runs.
app.UseAzureAppConfiguration();
```

Worth stressing:

- **Labels are the environment axis.** One store, `Development` / `Staging` / `Production` labels,
  selected in order so labeled values overlay the unlabeled defaults.
- **Refresh is not automatic.** You must call `ConfigureRefresh` and then `RegisterAll()` or
  `Register(key)`. Feature flags are the exception — `UseFeatureFlags` self-registers.
- **Refresh is activity-driven** in ASP.NET Core: the middleware checks on an incoming request once
  the interval has elapsed. A completely idle app never refreshes. Background services inject
  `IConfigurationRefresherProvider` and call `TryRefreshAsync()` themselves.
- **Sentinel key pattern.** Instead of `RegisterAll()`, watch one key you bump *after* every other
  edit lands, so a multi-key change is picked up atomically rather than half-applied:

```csharp
.ConfigureRefresh(refresh =>
{
    refresh.Register("Weather:Sentinel", refreshAll: true)
           .SetRefreshInterval(TimeSpan.FromSeconds(30));
})
```

- **Failure mode is graceful.** A failed refresh keeps the last known-good configuration and retries
  later. A failed connection *at startup* is not graceful — that's a crash unless you handle it.
- **Key Vault references** live in the store as pointers with a distinct content type; the provider
  dereferences them via `ConfigureKeyVault`. Resolution order: registered `SecretClient` →
  credential → `SetSecretResolver` fallback. Give them their own refresh cadence with
  `SetSecretRefreshInterval(key, TimeSpan)` — a rotated secret is otherwise cached forever.
- **`Map`** rewrites keys on the way in, e.g. flattening `App__Settings__X` to `App:Settings:X`.

### 4.7 Key-per-file (containers and Kubernetes)

Docker secrets and Kubernetes mounted secrets are one file per value. This is how you consume them
without an Azure round trip.

```csharp
// Does NOT reload — the 3-argument overload is
//   => builder.AddKeyPerFile(directoryPath, optional, reloadOnChange: false);
builder.Configuration.AddKeyPerFile(directoryPath: "/run/secrets", optional: true);

// Reloads. Reach for the 4-argument overload deliberately.
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true, reloadOnChange: true);
```

File name is the key, file contents are the value, `__` is the delimiter — a file named
`Weather__ApiKey` containing `abc123` becomes `Weather:ApiKey = abc123`. The path must be absolute.

### 4.8 In-memory (tests, and defaults)

```csharp
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Weather:TimeoutSeconds"] = "1",
    ["Weather:ApiKey"] = "test-key",
});
```

The right tool for `WebApplicationFactory` integration tests — added last, it beats everything.

```csharp
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Weather:ApiBaseUrl"] = "http://localhost/stub",
            }));
}
```

### 4.9 INI and XML — explicitly cut

**No slide, and no spoken line.** `AddIniFile` and `AddXmlFile` exist, are built in, and reload like
any other file provider. Since .NET 6 the XML provider auto-indexes repeating elements rather than
requiring a `name` attribute.

They are cut on purpose rather than forgotten. Neither teaches anything the JSON provider has not
already taught — the provider model and the precedence rules are identical — and a drive-by mention
buys completeness at the cost of a detour. Almost nobody is reaching for INI in 2026.

They stay in the repo's provider catalogue as legacy-interop options. If it comes up in Q&A:
*"Yes, `AddIniFile` and `AddXmlFile` exist. Same provider model, same precedence rules."*

### 4.10 Custom provider

The extensibility point: `IConfigurationSource` + `ConfigurationProvider`. Good demo targets are a
database-backed provider or a `.env` file reader.

```csharp
public sealed class SqlConfigurationSource(string connectionString) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new SqlConfigurationProvider(connectionString);
}

public sealed class SqlConfigurationProvider(string connectionString) : ConfigurationProvider
{
    public override void Load()
    {
        // Data is the protected IDictionary<string, string?> on the base class.
        Data = QuerySettings(connectionString);

        // Call OnReload() after a background refresh to fire change tokens.
    }
}

public static class SqlConfigurationExtensions
{
    public static IConfigurationBuilder AddSqlConfiguration(
        this IConfigurationBuilder builder, string connectionString) =>
        builder.Add(new SqlConfigurationSource(connectionString));
}
```

Points to land: `Load()` runs once at build time; `OnReload()` is what makes `IOptionsMonitor` fire;
a provider that throws in `Load()` takes the app down at startup — sometimes exactly what you want.

### 4.11 Non-web hosts: console, worker, WinForms, WPF, MAUI

> **Structural note — desktop falls off the spine.** §1.4's three stages assume a server: local dev →
> deployment → shared. A desktop app has local dev, has "deployment" as an *install*, and has no
> stage three at all — there is no orchestrator setting environment variables, no central store it
> should authenticate to, and no instance fleet. Name this explicitly rather than letting the
> structure quietly contradict the abstract's "or desktop apps." The line to use: *"the spine assumes
> something that gets deployed; here's what changes when it gets installed on someone's laptop
> instead."* Feature flags are the interesting exception — a desktop app can absolutely consume flags
> from a file or from your own backend, it just can't hold the credential to a flag store directly.


The abstract promises "cloud-native microservices **or desktop apps**," and this is the section that
pays it off. The configuration model is identical — what differs is what's wired up for you.

**Console apps and workers** get the full default chain from the generic host:

```csharp
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptionsWithValidateOnStart<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddHostedService<ForecastPoller>();

using var host = builder.Build();
await host.RunAsync();
```

Same JSON + user secrets + env vars + command line, same precedence, same `DOTNET_ENVIRONMENT`
(note: `DOTNET_`, not `ASPNETCORE_`, outside ASP.NET Core).

**Desktop apps (WinForms / WPF)** don't have a host by default. Two options:

```csharp
// Option A — no host: build IConfiguration by hand.
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();
```

```csharp
// Option B — host the desktop app. Preferred: you get DI, logging, and options for free.
var builder = Host.CreateApplicationBuilder();
builder.Services.Configure<WeatherOptions>(
    builder.Configuration.GetSection(WeatherOptions.SectionName));
builder.Services.AddSingleton<MainForm>();

using var host = builder.Build();
Application.Run(host.Services.GetRequiredService<MainForm>());
```

Desktop-specific realities to call out:

- **`SetBasePath(AppContext.BaseDirectory)`** — the working directory of a double-clicked EXE is not
  the install directory. `Directory.GetCurrentDirectory()` will bite you.
- **Per-user, writable settings are a different problem.** `IConfiguration` is read-optimized and
  has no write API. User preferences belong in a JSON file under
  `Environment.SpecialFolder.ApplicationData` that you load as an extra provider and save yourself.
- **`app.config` is not gone**, but `ConfigurationManager.AppSettings` and `IConfiguration` are two
  unrelated systems. Migrating means moving keys into `appsettings.json`, not bridging them.
- **Installed apps can't rely on environment variables.** No orchestrator is setting them.
  Files plus per-machine registry/install-time values are the realistic sources.
- **Client apps must not hold cloud secrets.** No Key Vault credential ships in a desktop binary —
  the app authenticates the *user* and calls a backend that holds the secret. Say this out loud;
  someone always asks.

---

---

## 5. Binding and the options pattern

### 5.1 Stop injecting `IConfiguration`

```csharp
// Don't: stringly typed, unvalidated, untestable, re-parsed on every call.
public sealed class WeatherClient(IConfiguration config)
{
    public async Task<Forecast> GetAsync() =>
        await CallAsync(config["Weather:ApiBaseUrl"]!, int.Parse(config["Weather:TimeoutSeconds"]!));
}
```

```csharp
// Do: bound, validated, and the class declares exactly what it needs.
public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    [Required, Url]
    public required string ApiBaseUrl { get; init; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    [Range(0, 10)]
    public int Retries { get; init; } = 3;

    [Required, MinLength(8)]
    public required string ApiKey { get; init; }
}

public sealed class WeatherClient(IOptions<WeatherOptions> options)
{
    private readonly WeatherOptions _options = options.Value;
}
```

### 5.2 Registration

```csharp
builder.Services
    .AddOptions<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(o => o.Retries == 0 || o.TimeoutSeconds >= 5,
              "TimeoutSeconds must be >= 5 when retries are enabled.")
    .ValidateOnStart();
```

Or the shorthand that makes start-time validation the default posture:

```csharp
builder.Services
    .AddOptionsWithValidateOnStart<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .ValidateDataAnnotations();
```

`ValidateDataAnnotations` lives in `Microsoft.Extensions.Options.DataAnnotations` (referenced
implicitly by the web SDK).

### 5.3 The three interfaces — pick deliberately

| Interface | Lifetime | Re-reads config? | Use when |
| --- | --- | --- | --- |
| `IOptions<T>` | Singleton | No — bound once, forever | The value can't change at runtime |
| `IOptionsSnapshot<T>` | **Scoped** | Once per request/scope | Per-request consistency in a web app |
| `IOptionsMonitor<T>` | Singleton | Yes, with `OnChange` callbacks | Singletons and background services |

```csharp
public sealed class ForecastPoller : BackgroundService
{
    private readonly IOptionsMonitor<WeatherOptions> _monitor;
    private readonly ILogger<ForecastPoller> _logger;
    private readonly IDisposable? _onChange;

    public ForecastPoller(IOptionsMonitor<WeatherOptions> monitor, ILogger<ForecastPoller> logger)
    {
        _monitor = monitor;
        _logger = logger;

        // OnChange returns the registration. Hold it and dispose it, or you leak the subscription.
        _onChange = _monitor.OnChange((options, name) =>
            _logger.LogInformation(
                "Weather options changed: timeout now {TimeoutSeconds}s", options.TimeoutSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var current = _monitor.CurrentValue;   // re-read every iteration
            await Task.Delay(TimeSpan.FromSeconds(current.TimeoutSeconds), stoppingToken);
        }
    }

    public override void Dispose()
    {
        _onChange?.Dispose();
        base.Dispose();
    }
}
```

Two conventions doing work here: the change registration is an `IDisposable` that must be held and
disposed, and the log message is a **template with named placeholders**, never an interpolated
string — structured logging is the house rule, and a demo people copy from should model it.

**The classic bug:** injecting `IOptionsSnapshot<T>` into a singleton. It's scoped — the container
either throws at validation or you capture the first scope's value forever.
**The other classic bug:** caching `_monitor.CurrentValue` in a field at construction, which quietly
turns a monitor back into an `IOptions<T>`.

### 5.4 Named options

For "same shape, several times" — multiple API clients, multiple queues.

```csharp
builder.Services.Configure<EndpointOptions>("primary",
    builder.Configuration.GetSection("Endpoints:Primary"));
builder.Services.Configure<EndpointOptions>("secondary",
    builder.Configuration.GetSection("Endpoints:Secondary"));

public sealed class Router(IOptionsMonitor<EndpointOptions> monitor)
{
    private EndpointOptions Primary => monitor.Get("primary");
}
```

`Configure<T>(section)` with no name is shorthand for `Options.DefaultName` (`""`).

### 5.5 Validation, deeper

Nested objects and collections are **not** validated by default. Opt in:

```csharp
public sealed class AppOptions
{
    [Required, ValidateObjectMembers]            // recurse into the nested object
    public DatabaseOptions Database { get; init; } = new();

    [ValidateEnumeratedItems]                    // validate each item
    public List<ServerOptions> Servers { get; init; } = [];
}
```

Complex, cross-field, or service-dependent rules belong in a class:

```csharp
public sealed class WeatherOptionsValidator : IValidateOptions<WeatherOptions>
{
    public ValidateOptionsResult Validate(string? name, WeatherOptions options)
    {
        if (options.ApiBaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail("ApiBaseUrl must use HTTPS.");
        }

        return ValidateOptionsResult.Success;
    }
}

builder.Services.TryAddEnumerable(
    ServiceDescriptor.Singleton<IValidateOptions<WeatherOptions>, WeatherOptionsValidator>());
```

**Fail fast.** Without `ValidateOnStart()`, validation runs lazily on first `.Value` access — a bad
deploy looks healthy until the first request reaches the affected code path. With it, the process
refuses to start. That's the behavior you want during a rolling deployment.

### 5.6 Source generators (AOT, trimming, and speed)

Both binding and validation default to reflection. Two opt-ins remove it:

```xml
<PropertyGroup>
  <EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>
</PropertyGroup>
```

```csharp
using Microsoft.Extensions.Options;

// Empty partial class; the generator writes the IValidateOptions<T> implementation.
[OptionsValidator]
public sealed partial class ValidateWeatherOptions : IValidateOptions<WeatherOptions>;

builder.Services.AddSingleton<IValidateOptions<WeatherOptions>, ValidateWeatherOptions>();
```

The options validation generator is on by default when the project references
`Microsoft.Extensions.Options` 8+ or builds an ASP.NET Core app. It rewrites reflection-based
attributes (`[Range]`, `[MinLength]`, `[MaxLength]`, `[Length]`) into generated equivalents. With
`[OptionsValidator]` in play you don't also call `ValidateDataAnnotations()`.

Demo idea: set `PublishAot=true`, show the `IL2026` / `IL3050` warnings from reflection binding,
then turn on both generators and watch them disappear.

---

## 6. Feature flags

**Roughly a quarter of the talk.** Not an appendix to Azure App Configuration — this is the second
half of the thesis in §1.1, and the only part of the configuration story where a value changes while
someone is watching.

### 6.1 A feature flag is just configuration

The most important slide in this section: `Microsoft.FeatureManagement` is built on
`IConfiguration`. **Any** configuration provider can back a feature flag. No new infrastructure, no
new file format, no service dependency.

That means flags inherit everything from §2–§4 — the provider chain, precedence, reload semantics,
and the debug view. A flag in `appsettings.Development.json` is overridden by the same flag in an
environment variable, by the same last-wins rule as any other key.

It also means flags live at all three stages of the spine:

| Stage | Where the flag lives | What you get |
| --- | --- | --- |
| **Local dev** | `appsettings.json` / user secrets | Branch in code without branching in git. Zero cloud. |
| **Deployment** | Environment variable per environment | Ship dark, enable per environment at deploy time |
| **Shared** | Azure App Configuration | Flip at runtime, target a cohort, roll back in seconds |

Demo the first row before anyone sees Azure. It's the row that makes flags feel free.

### 6.2 Flags with no cloud at all

```jsonc
{
  "feature_management": {
    "feature_flags": [
      { "id": "NewCheckout", "enabled": false },
      { "id": "BetaBanner",  "enabled": true },
      {
        "id": "HolidayPricing",
        "enabled": true,
        "conditions": {
          "client_filters": [
            {
              "name": "Microsoft.TimeWindow",
              "parameters": {
                "Start": "Sun, 01 Dec 2025 00:00:00 GMT",
                "End":   "Fri, 26 Dec 2025 00:00:00 GMT"
              }
            }
          ]
        }
      }
    ]
  }
}
```

```csharp
builder.Services.AddFeatureManagement();
```

Schema notes worth saying out loud:

- `feature_management` / `feature_flags` is the **Microsoft schema**, shared across the .NET, Go,
  Python, and JavaScript libraries. The older .NET-only `FeatureManagement` section still works; when
  both are present, `feature_management` wins.
- No `conditions` means the flag is simply `enabled`. With `conditions`, `enabled: true` means
  "eligible to be evaluated," not "on."
- `requirement_type` defaults to `Any`; set `All` when every filter must pass.
- **A colon is forbidden in a flag name** — flag names are configuration keys.
- Point the library at a custom section with
  `AddFeatureManagement(configuration.GetSection("MyFlags"))`.

### 6.3 Consuming a flag

```csharp
// The general case. IVariantFeatureManager is the modern interface.
public sealed class CheckoutService(IVariantFeatureManager features)
{
    public async Task<Receipt> CheckoutAsync(Cart cart, CancellationToken ct)
    {
        if (await features.IsEnabledAsync("NewCheckout", ct))
        {
            return await NewFlowAsync(cart, ct);
        }

        return await LegacyFlowAsync(cart, ct);
    }
}
```

```csharp
// MVC: gate a controller or a single action.
// Blocked requests get 404 by default; override with IDisabledFeaturesHandler.
[FeatureGate("NewCheckout")]
public sealed class CheckoutController : Controller;
```

```html
<!-- Razor views: the tag helper, including negation. -->
<feature name="BetaBanner">
  <p>Try the new checkout.</p>
</feature>

<feature negate="true" name="BetaBanner">
  <p>Classic checkout.</p>
</feature>
```

```csharp
// Branch the entire pipeline on a flag.
app.UseForFeature("NewCheckout", branch => branch.UseMiddleware<NewCheckoutMiddleware>());
```

Registration detail that bites people: `AddFeatureManagement()` registers feature management as a
**singleton**. If a filter needs scoped services (the current user, a `DbContext`), use
`AddScopedFeatureManagement()` instead.

`FeatureGate`, the tag helper, and `UseForFeature` need
`Microsoft.FeatureManagement.AspNetCore`; the core `Microsoft.FeatureManagement` package covers
everything else.

### 6.4 Filters: flags that decide for themselves

Built in, and registered automatically by `AddFeatureManagement()` — except targeting, which needs
`WithTargeting`:

| Filter | Alias | Use |
| --- | --- | --- |
| Percentage | `Microsoft.Percentage` | Progressive rollout: 5% → 25% → 100% |
| Time window | `Microsoft.TimeWindow` | Scheduled enable/disable, with `Recurrence` for daily/weekly |
| Targeting | `Microsoft.Targeting` | Named users, groups, per-group percentages, exclusions |

```jsonc
{
  "id": "NewCheckout",
  "enabled": true,
  "conditions": {
    "requirement_type": "All",
    "client_filters": [
      { "name": "Microsoft.Percentage", "parameters": { "Value": 25 } },
      {
        "name": "Microsoft.TimeWindow",
        "parameters": { "Start": "Mon, 01 Sep 2025 00:00:00 GMT" }
      }
    ]
  }
}
```

Targeting needs an `ITargetingContextAccessor` so the library knows who the current user is:

```csharp
builder.Services.AddFeatureManagement().WithTargeting<HttpContextTargetingContextAccessor>();
```

**The percentage-filter gotcha — worth its own slide.** Plain `Microsoft.Percentage` is evaluated
*per call*, not per user. A 50% flag checked twice in one request can answer differently each time,
and a user can watch the feature flicker between page loads. Targeting, or variant allocation with a
`seed`, is what gives a *stable* per-user assignment. This is the most common feature-flag bug in the
wild.

Custom filters implement `IFeatureFilter` (one method, `EvaluateAsync`) and register with
`.AddFeatureFilter<TenantFilter>()`. Good demo: a flag enabled per tenant.

### 6.5 Variants: past on/off

A variant flag returns a *value* — string, number, bool, or a whole configuration object — instead of
a boolean. This is where feature flags and the options pattern meet.

```csharp
Variant variant = await features.GetVariantAsync("CheckoutLayout", ct);

var settings = new CheckoutLayoutSettings();
variant.Configuration.Bind(settings);   // it's an IConfigurationSection — bind it like anything else
```

Allocation is evaluated in order — `user` → `group` → `percentile` — falling back to
`default_when_enabled`, or `default_when_disabled` when the flag is off. A `seed` makes percentile
assignment stable, and consistent across flags that share the same seed.

`status_override` (`None` / `Enabled` / `Disabled`) lets a variant flag also answer `IsEnabledAsync`,
so you can adopt variants without rewriting existing call sites. You can't override a flag whose
`enabled` is `false`.

### 6.6 Flag debt — the part nobody covers

Every flag is a permanent `if` statement with an owner and an expiry date. This section is what
separates the talk from the documentation.

- **A flag doubles your code paths.** Ten live flags is up to 1,024 nominal combinations; you test
  maybe three of them.
- **Name flags for their removal.** `NewCheckout` never gets deleted.
  `Checkout_V2_Rollout_2026Q1` files its own expiry.
- **Categorize on creation** — lifetime differs by an order of magnitude:
  *release* toggles (days to weeks, delete after rollout) · *experiment* toggles (weeks, delete after
  the decision) · *ops* toggles and kill switches (permanent, and that's fine) · *permission*
  toggles (permanent, and arguably not a flag at all — that's authorization).
- **Deleting a flag is a code change, not a config change.** Turning it off in the portal and walking
  away leaves the dead branch compiling forever.
- **Inventory them.** `GetFeatureNamesAsync()` enumerates every flag the app knows about. A health
  endpoint listing flags and their current state is a five-minute build and the demo people will
  actually steal.

### 6.7 Testing with flags

Flags are configuration, so the §4.8 in-memory provider is the whole story:

```csharp
using Microsoft.AspNetCore.Hosting;   // required: without it the one-arg
                                      // ConfigureAppConfiguration overload
                                      // isn't in scope and you get CS1593

protected override void ConfigureWebHost(IWebHostBuilder builder) =>
    builder.ConfigureAppConfiguration(config =>
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["feature_management:feature_flags:0:id"] = "NewCheckout",
            ["feature_management:feature_flags:0:enabled"] = "true",
        }));
```

Or substitute `IVariantFeatureManager` outright. The rule: **test both sides of every live flag**, or
you're shipping an untested branch behind a switch someone can flip at 2am.

### 6.8 The honest counterargument

Someone in the room is going to think it, so say it first: flags convert a deployment problem into a
runtime problem. You gain instant rollback and lose "the code that ran is the code in the commit." A
flag flipped in a portal is a production change with no PR, no review, and no diff — unless the store
keeps history and you treat flag changes as deploys. Azure App Configuration keeps revisions; use
them.

That's the same trade as §1.2's build-time-to-runtime move, one level up — which is the through-line
of the entire talk.

---

## 7. Choosing a strategy

The abstract promises "how to choose the right configuration strategy for your app." This is the
decision slide — the one people photograph.

### 7.1 Sort every setting into three buckets

| Bucket | Examples | Where it belongs |
| --- | --- | --- |
| **Non-secret, per-environment** | base URLs, timeouts, retry counts, log levels, feature toggles | `appsettings.{Env}.json` in the repo, or App Configuration |
| **Secret** | API keys, connection strings with passwords, client secrets | User secrets (dev) → Key Vault or the platform's secret store (prod) |
| **Per-instance / per-deployment** | environment name, instance id, port, region | Environment variables, set by the platform |

If a value is in the wrong bucket, no amount of provider tuning fixes it.

### 7.2 By application shape

| App shape | Baseline | Secrets | Change without redeploy? |
| --- | --- | --- | --- |
| ASP.NET Core on App Service | JSON + App Service settings (env vars) | Key Vault refs in App Service settings, or Key Vault provider | App Service settings restart the app; App Configuration doesn't |
| Container / AKS | JSON baked in + env vars from the orchestrator | Key-per-file mount, or Key Vault via workload identity | Restart the pod, or App Configuration for hot values |
| Cloud-native microservices, many apps | App Configuration with labels + a per-app key prefix | Key Vault references from App Configuration | Yes — sentinel key + refresh |
| Worker / background service | JSON + env vars | Key Vault (managed identity) | `IOptionsMonitor` + explicit `TryRefreshAsync` |
| Console / CLI tool | JSON + command line + env vars | User secrets in dev; a credential the user already has in prod | No — it's a short-lived process |
| Desktop (WinForms/WPF/MAUI) | JSON next to the EXE + per-user AppData file | **None in the client** — authenticate the user, call a backend | Restart, or fetch from your own API |

### 7.3 Rules of thumb

1. **One artifact, many environments.** If you build a different binary per environment, you've
   reintroduced `Web.Release.config`. Configuration is a deployment-time input, not a build input.
2. **Commit defaults, never secrets.** `appsettings.json` should be safe to publish. If leaking it
   would matter, something is in the wrong bucket.
3. **Reach for cloud configuration when you have a real reason:** more than one app sharing values,
   or a need to change a value without a deploy. One app that redeploys in five minutes does not
   need App Configuration.
4. **Prefer restart to hot reload** unless hot reload is a requirement someone asked for. Hot reload
   is a distributed-systems problem wearing a config hat.
5. **Validate at startup, always.** It's the cheapest possible guardrail on every strategy above.
6. **Secrets get a rotation story or they aren't secure.** "It's in Key Vault" plus a process that
   caches it forever is a secret you can't rotate.

---

## 8. Reload semantics — what actually changes at runtime

| Source | Reloads? | Mechanism |
| --- | --- | --- |
| `appsettings*.json` | Yes, when `reloadOnChange: true` (the host builders' default) | `FileSystemWatcher` |
| User secrets | Yes | file watcher |
| Environment variables | **No** | read once at startup |
| Command line | **No** | read once at startup |
| Azure Key Vault | Only if `ReloadInterval` is set (default: never) | polling |
| Azure App Configuration | Yes, with `ConfigureRefresh` plus a refresh trigger | polling on activity |
| Key-per-file | **Only with the 4-argument overload.** `AddKeyPerFile(dir, optional)` passes `reloadOnChange: false` | file watcher, once enabled |
| In-memory | No | — |
| Custom | Your call — `OnReload()` | yours |

Consequences to say out loud:

- Changing an environment variable on a running container does **nothing**. Restart the container.
- `reloadOnChange` on a Kubernetes ConfigMap mount is unreliable — those are symlink swaps, not
  in-place writes, and the watcher may or may not see them. Treat restart as the contract.
- A single file save often fires the watcher **twice** (write + metadata). Debounce anything
  expensive hanging off `OnChange`.
- Reloading configuration does not reconfigure things that read it once at startup: Kestrel
  endpoints, the DI graph, `HttpClient` handler pipelines. "Config reloaded" ≠ "app reconfigured."

---

## 9. What changed in .NET 10

Talk-relevant deltas from .NET 9:

1. **Null values are preserved** *(breaking change)*. Previously a `null` in configuration was
   treated as missing and skipped by the binder, and the JSON provider converted `null` to `""`.
   In .NET 10 the JSON provider reports `null` unchanged and the binder binds it like any other
   value. Binding of `null` array elements and empty arrays now works.
   → The payoff differs **by type**, so do not state it as one rule:
   a `string` with an initializer was *already* overwritten — with `""`;
   an `int?` **kept** its initializer, because `""` could not be parsed;
   a non-nullable value type **threw** `InvalidOperationException`.
   In .NET 10 those become `null`, `null`, and `default(T)` respectively, and
   empty arrays now bind as empty arrays instead of being ignored.
   The honest one-liner is `d06`'s own observation: *"Retries was 3 on .NET 9 and
   is null on .NET 10."* Show at least `string?`, `int?` and a non-nullable type.
   [Docs](https://learn.microsoft.com/dotnet/core/compatibility/extensions/10.0/configuration-null-values-preserved)
2. **Seven new connection-string environment prefixes** (11 total) — Postgres, Cosmos, Redis,
   Service Bus, Event Hubs, Notification Hubs, API Hubs. See §4.2.
3. **AOT-safe `ValidationContext` constructor** taking an explicit `displayName`, removing AOT
   warnings from options validation.
4. **File-based apps** (`dotnet run app.cs`) participate in user secrets via a path-hash-derived
   `UserSecretsId` and `dotnet user-secrets --file`.

Optional forward-looking closer, clearly labeled as .NET 11 preview: a generic
`OptionsBuilder<T>.Validate<TValidator>()` overload, `Func<Task>` overloads on
`ChangeToken.OnChange`, and async DataAnnotations validation.

---

## 10. Anti-patterns (the "don't do this" slide)

1. Secrets committed in `appsettings.json` — or worse, `appsettings.Production.json`.
2. `IConfiguration` injected into business classes instead of bound options.
3. `config["Some:Key"]!` with a null-forgiving operator and no validation.
4. `IOptionsSnapshot<T>` in a singleton.
5. `IOptionsMonitor<T>.CurrentValue` captured in a constructor field.
6. Building a throwaway `ConfigurationBuilder` mid-request to "get a fresh value."
7. Calling `WebApplication.CreateBuilder()` a second time just to read configuration.
8. `if (env.IsProduction())` branches in code instead of environment-specific configuration values.
9. One Key Vault for every app and environment, separated by secret-name prefixes.
10. Assuming an array in `appsettings.Production.json` replaces the base array. It doesn't.
11. Logging `GetDebugView()` without redaction.

---

## 11. Snippet plan

**The talk has no live demos.** Every idea below is a code snippet or a captured terminal
output on a slide, walked through out loud. The `/demos` projects still ship — they are where
the snippets come from and they are the audience's takeaway, but nothing is executed on stage.

Why: sixteen scheduled demo transitions in a 90-minute room is the single largest risk to the
talk landing, and every live cloud demo adds a failure mode that costs minutes you cannot get
back. Snippets cost nothing at runtime and the repo still proves the code runs.

Numbered to match `/demos`. Each one is still one idea.

| # | Demo | Shows | Payoff |
| --- | --- | --- | --- |
| 01 | `GetDebugView` dump | Provider chain, precedence, redaction | "Where did this value come from?" |
| 02 | Layer JSON → env file → env var → CLI arg | Last-wins ordering | Same key, four winners |
| 03 | Array override surprise | Index-wise merge | Audience gasp |
| 04 | User secrets, incl. `dotnet run app.cs --file` | Dev secrets, .NET 10 file-based apps | No secrets in the repo |
| 05 | Options binding + `ValidateOnStart` | Fail-fast startup | Bad config = no start |
| 06 | `IOptions` vs `IOptionsSnapshot` vs `IOptionsMonitor` | Live-edit `appsettings.json` while running | Three answers on one screen |
| 07 | Key Vault with `DefaultAzureCredential` | `--` → `:`, managed identity | No connection strings anywhere |
| 08 | App Configuration + sentinel key + feature flag | Change without redeploy | A flag flipped without a deploy |
| 09 | Custom provider (SQL or `.env`) | `IConfigurationSource`, `OnReload` | The extensibility point |
| 10 | Source generators + `PublishAot` | Warnings before and after | Trim-safe configuration |

The authoritative list lives in Solo (project `modern-dotnet-configuration`, 33 items tagged
`demo`), split one idea per project. The flag projects are 28–33; see §6.

**No Azure dependency.** `d18` (Key Vault) and `d20` (App Configuration) were the two that
needed a live subscription. As snippets they need nothing, which removes the talk's only
external dependency on show day.

**Capture rules.** Terminal output on a slide is real output from the matching project, not a
mock-up — rendered as text so it stays legible from the back row rather than screenshotted with
window chrome. Screenshots are for cases where the tooling itself is the point: the Azure portal,
a flag flipping in App Configuration.

### 11.1 Running order — 90 minutes (Cloud & AI Summit)

Feature flags get 20 of the 90 (~22%). Budget 84 minutes of content; questions ride along rather
than queueing to the end. **Minutes are advisory** — spend them where the room is engaged.

| Minutes | Block | Content | Snippets |
| --- | --- | --- | --- |
| 0–04 | **Cold open** | §1.0 two slides: the file says 120, the app says 10 — then the provider dump, unexplained | d01 |
| 04–10 | **Why** | §1.1 five reasons — hard-coding, environments, on the fly, **trust**, **ownership** | — |
| 10–13 | Cold open pt 2 | §1.2 constants → `web.config` → Generic Host. The build-time/runtime trade | — |
| 13–16 | **Thesis + spine** | §1.3 config for everything, flags for everything · §1.4 local dev → deployment → shared | — |
| 16–23 | The model | §2 flat dictionary, strings, `:` and `__`, arrays as keys. **Pays off the cold open** | — |
| 23–31 | Order | §3 host vs app config, why `ASPNETCORE_ENVIRONMENT` picks your other inputs | d02, d03 |
| 31–39 | **Stage 1 — local dev** | §4.1 environment files, §4.4 user secrets + .NET 10 file-based apps, the `launchSettings` trap. Close on its signature failure | d09, d07 |
| 39–51 | Options | §5 binding, three interfaces, `ValidateOnStart` | d11, d12, d13 |
| 51–59 | **Stage 2 — deployment** | §4.2–4.3 env vars, .NET 10 connstr prefixes, §4.7 key-per-file, §4.5 Key Vault. Close on its signature failure | d04, d05, d23, d18 |
| 59–64 | **Stage 3 — shared** | §4.6 App Configuration: labels, sentinel key, KV references | d20 |
| 64–84 | **Feature flags** | §6 — see the breakdown below | d28, d29, d30, d32 |
| 84–87 | Choosing | §7 three buckets + by-app-shape matrix. **The photograph slide** — where to stop | — |
| 87–90 | Close | §10 anti-patterns rapid-fire, §4.11 desktop aside if time, repo QR, contact | — |

The 20-minute feature-flag block, in detail:

| Minutes | Content | Snippets |
| --- | --- | --- |
| 64–67 | §6.1 a flag is just configuration — the provider chain already does this | — |
| 67–72 | §6.2–6.3 flags with zero cloud: JSON schema, `IVariantFeatureManager`, `FeatureGate`, tag helper | d28 |
| 72–77 | §6.4 filters: percentage, time window, targeting — **and the per-call vs per-user gotcha** | d29, d30 |
| 77–80 | §6.5 variants: flags that return values, bound like any other config section | d31 (optional) |
| 80–84 | §6.6 flag debt + §6.8 the honest counterargument. Ends the talk on judgment, not tooling | d32 |

Notes:
- The three options interfaces (`d13`) are a **before/after slide pair** — the same three values
  before and after `appsettings.json` changes underneath the running app. `IOptions` doesn't move;
  the other two do. It is the longest beat in the talk. Don't rush it.
- Stage 3 shrank to 5 minutes because §6 now carries App Configuration's most interesting behavior.
  Show the sentinel key there and let flags do the rest.
- §6.6 flag debt is the closing argument. It's the thing nobody else in the room will say.

### 11.2 Running order — 60 minutes

Flags get 10 of the 51 content minutes (~20%) — the ratio holds. Budget 51, not 56, and let
questions ride along; a packed 56 overruns the moment a hand goes up during Options.

| Minutes | Block | Content | Snippets |
| --- | --- | --- | --- |
| 0–03 | **Cold open** | Wrong value, then the unexplained dump | d01 |
| 03–07 | **Why + thesis** | §1.1 five reasons (trust and ownership get one line each), §1.3 thesis | — |
| 07–09 | **Spine** | §1.4 local dev → deployment → shared, with the three signature failures | — |
| 09–14 | The model | Flat dictionary, `:` / `__`, precedence. Pays off the cold open | — |
| 14–19 | Order | Host vs app config, layering | 02, 03 |
| 19–23 | **Stage 1 — local dev** | Environment files, user secrets; mention file-based apps, don't demo | 09 |
| 23–31 | Options | Binding, three interfaces, `ValidateOnStart` | 12, 13 |
| 31–35 | **Stage 2 — deployment** | Env vars, .NET 10 connstr prefixes, Key Vault in one slide | 05 |
| 35–38 | **Stage 3 — shared** | App Configuration: labels + sentinel key | d20 |
| 38–48 | **Feature flags** | §6.1–6.4 local flags + filters + the per-call gotcha; §6.6 flag debt | 28, 29 |
| 48–51 | Choosing + close | §7 matrix, anti-patterns, repo QR | — |

The nine minutes of slack are the point. Take questions at three breath points — after the model
(14), after Options (31), after flags (48) — never during the cold open, which only works while it
stays unexplained.

Running long? Cut in this order. It buys six minutes without touching the never-cut list:

1. Demo 03 (array merge) — a gasp, not a load-bearing idea.
2. Stage 3 down to the sentinel-key slide alone.
3. Demo 29 — describe the per-call gotcha instead of showing it.

Stretching past 60 is not a re-pace: switch to §11.1, which is a different talk. The extra 30
minutes are real content — Key Vault, key-per-file, variants, flag debt at full length, the
desktop aside — not padding.

**Cut from the 90 for the 60:**

| Cut | Why it survives being cut |
| --- | --- |
| Demo 18 (Key Vault, live) | One slide covers `--` → `:` and managed identity; the mechanics aren't visual |
| Demo 23 (key-per-file) | Named in the deployment slide, not demoed |
| Demo 25 (custom provider) | Interesting to few; point at the repo |
| Demo 17 (AOT + source generators) | Niche; a §5.6 slide is enough |
| Demo 31 (variants) | Mention that flags can return values, move on |
| §4.11 desktop deep dive | Compress to one "same model, no host, no client secrets" slide |
| §8 reload table | Show the table, skip the walk-through |
| §1.2 history anecdote | Compress three slides to two, drop the storytelling |

**Never cut, at any length:** demo 01 (`GetDebugView`), demo 13 (three interfaces),
`ValidateOnStart`, §6.4's per-call-vs-per-user gotcha, §6.6 flag debt, and §7.3 rule 2
(commit defaults, never secrets).

### 11.3 If you get 45 minutes

Keep the snippets from `d01`, `d02`, `d13` and `d28`; everything else becomes a claim slide.
Keep the §1.4 spine slide — at 45 minutes it does more organizing work than anything else on
offer. Flags drop to 8 minutes: §6.1 (it's just configuration), §6.4's gotcha, §6.6 debt.

At this length the cold open earns its keep more than anywhere else: it is two slides and it buys
you the attention to get through §2 and §3 quickly.

### 11.4 Demo layout and naming

#### The rule that drives everything

**The demo number is an identity, not a running order.** There are three running orders (§11.1–11.3)
and only one filesystem, so the folder number can't encode sequence. `d13` is *the options-lifetimes
demo* forever — in the 90 it runs eighth, in the 45 it runs third, and the folder never moves. Cut
demos leave gaps. Nothing is ever renumbered.

That number is also the join key across three places: the folder, the Solo todo, and the
running-order tables. Renumber once and all three drift.

#### Layout

```text
demos/
  README.md                     index table: id · title · one idea · stage · run command
  ASSIGNMENTS.md                folder -> todo -> spec map, and the documented exceptions
  WORKER-BRIEF.md               the contract demos are built against
  verify.ps1                    walks every demo: build + format check
  d01-provider-dump/
    README.md                   one idea · commands · expected output · stage
    D01.ProviderDump.csproj
    Program.cs
    appsettings.json
  d02-precedence/
  ...
```

#### Naming rules

| Thing | Pattern | Example |
| --- | --- | --- |
| Folder | `d{NN}-{kebab-slug}` | `d29-percentage-trap` |
| Project file | `D{NN}.{PascalSlug}.csproj` | `D29.PercentageTrap.csproj` |
| Assembly / root namespace | same as the project file stem | `D29.PercentageTrap` |
| Test project | `d{NN}-{slug}/tests/D{NN}.{PascalSlug}.Tests.csproj` | `D33.FlagTesting.Tests.csproj` |
| Multi-project demo | role subfolders, each its own project | `d26-non-web-hosts/worker/`, `/desktop/` |
| Multi-project project name | `D{NN}.{PascalSlug}.{Role}` | `D26.NonWebHosts.Worker`, `D26.NonWebHosts.Desktop` |
| Reset script | `d{NN}-{slug}/reset.ps1` | resets user secrets, env vars, edited JSON |
| Shared helper | none — the dump helper is **copied** into each demo that needs it | self-contained beats DRY here |

- **Lowercase kebab for folders, PascalCase for projects.** The folder is how you find the source of a
  snippet; the
  project name is what appears in build errors and IDE tabs. Both carry the id so a stack trace on
  the projector names the demo.
- **Zero-pad to two digits** so `ls` sorts correctly, and keep the `d` prefix — a bare `01-` folder
  gets treated as a number by enough tools to be annoying, and `d13` is greppable in a way `13` is
  not.
- **Slug = the one idea, 2–4 words, noun phrase, no verbs.** `percentage-trap`, not
  `show-percentage-filter-problem`. It should match the distinctive phrase in the Solo todo title so
  the two are greppable together.
- **Top-level statements, no namespace declaration**, for demos that are a single `Program.cs`. Add a
  file-scoped namespace only where a demo declares types worth naming.

#### Running a demo

Every demo runs the same way, and the solution is never involved:

```bash
cd demos/d29-percentage-trap
dotnet run
```

There is no solution file and no shared build props: each folder stands alone (§11.5). Opening the
repo root in an IDE still works; nothing about running a demo depends on it.

#### The 33 folders

| Id | Folder | Stage |
| --- | --- | --- |
| d01 | `d01-provider-dump` | model |
| d02 | `d02-precedence` | model |
| d03 | `d03-array-merge` | local dev |
| d04 | `d04-env-var-keys` | deployment |
| d05 | `d05-connstr-prefixes` | deployment |
| d06 | `d06-null-preserved` | model |
| d07 | `d07-launchsettings-trap` | local dev |
| d08 | `d08-command-line` | deployment |
| d09 | `d09-user-secrets` | local dev |
| d10 | `d10-file-based-app` | local dev |
| d11 | `d11-options-binding` | options |
| d12 | `d12-validate-on-start` | options |
| d13 | `d13-options-lifetimes` | options |
| d14 | `d14-named-options` | options |
| d15 | `d15-recursive-validation` | options |
| d16 | `d16-validate-options-class` | options |
| d17 | `d17-source-generators` | options |
| d18 | `d18-key-vault` | deployment |
| d19 | `d19-appconfig-labels` | shared |
| d20 | `d20-appconfig-sentinel` | shared |
| d21 | `d21-appconfig-flag-flip` | shared / flags |
| d22 | `d22-appconfig-keyvault-refs` | shared |
| d23 | `d23-key-per-file` | deployment |
| d24 | `d24-in-memory-tests` | options |
| d25 | `d25-custom-provider` | extensibility |
| d26 | `d26-non-web-hosts` | reach |
| d27 | `d27-reload-on-change` | shared |
| d28 | `d28-flags-no-cloud` | flags |
| d29 | `d29-percentage-trap` | flags |
| d30 | `d30-flag-filters` | flags |
| d31 | `d31-flag-variants` | flags |
| d32 | `d32-flag-inventory` | flags |
| d33 | `d33-flag-testing` | flags |

`d21` and `d28` are deliberately separate: `d28` is the same flag with no cloud at all, and it runs
*first* so the room sees that flags cost nothing before Azure enters the picture. `d21` is the
portal flip.

#### Slide ↔ project cue

The demo id belongs in the **speaker notes** of the slide whose snippet came from it, not on the
slide face — the room does not care that a snippet came from `d29`. It matters for two things:
answering "can you go back to the percentage one" with a folder name rather than a scroll, and
keeping each snippet traceable to code that actually compiles.

Every snippet on a slide is lifted verbatim from its project. If a snippet and its project ever
disagree, the project is right and the slide is stale — `verify.ps1` builds them all.

### 11.5 Demo project conventions

**Every demo is fully self-contained.** No `Directory.Build.props`, no `Directory.Packages.props`, no
shared helper project, no solution file, no `global.json`. A demo folder is copy-pasteable: someone
takes `d29-percentage-trap/`, drops it anywhere, runs `dotnet run`, and it works.

That costs a little duplication — the ~15-line `GetDebugView` dump helper is copied into the demos
that use it, and every `.csproj` repeats the same property block. Both are deliberate. Duplication is
cheaper than a build graph an audience member can't reproduce from one folder, and the repeated
property block is itself a teaching artifact.

Each `.csproj` carries its own settings and its own pinned package versions:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <RootNamespace>D29.PercentageTrap</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.FeatureManagement.AspNetCore" Version="4.7.0" />
  </ItemGroup>

</Project>
```

Rules the demo code follows:

- **Nullable enabled, warnings as errors.** A demo that builds with warnings teaches the warning.
  `d17` is the one exception — its "before" state exists to show IL2026/IL3050, so it sets
  `TreatWarningsAsErrors=false` with a comment saying why.
- **`d06` is the only multi-target** (`net9.0;net10.0`), for the null-binding behavior change.
- **`sealed` by default** on classes; file-scoped namespaces; one type per file. Top-level statements
  with no namespace for single-file demos.
- **Primary constructors** for dependency capture; `required` + `init` on options classes.
- **Structured logging only** — `logger.LogInformation("Timeout is {Timeout}s", timeout)`, never an
  interpolated string. `Console.WriteLine` is fine in console demos with no host.
- **`IDisposable` registrations are held and disposed** — `IOptionsMonitor.OnChange` in particular.
- **No `!` null-forgiving** except in the deliberately-bad "before" samples in `d11`.
- **Async all the way** where any I/O appears; `CancellationToken` flowed and honored.
- **No secrets in any committed file.** Placeholders in `appsettings.json`, real values via user
  secrets or environment variables at runtime.

**Restore note:** if a machine-level private NuGet feed interferes, a demo may carry its own
`nuget.config` pinning nuget.org — inside the demo folder, keeping it self-contained.

Verification before the talk: from each demo folder, `dotnet build` clean with zero warnings and
`dotnet format --verify-no-changes` clean. `demos/verify.ps1` walks every folder and does both.

---

## 12. Open questions

- Cover .NET Aspire's configuration story (parameters, connection-string injection into
  `IConfiguration`), or leave it out for time?
- How deep on feature management — variant flags and targeting, or one flag and move on?
- Include a `web.config` → `IConfiguration` migration slide for the legacy crowd?
- ~~Session length~~ — resolved: the Cloud & AI Summit slot is 90 minutes. Both running orders are in §11.1 and §11.2.

---

## 13. References

- [Configuration in .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration)
- [Configuration providers in .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration-providers)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0)
- [Options pattern in .NET](https://learn.microsoft.com/dotnet/core/extensions/options)
- [Options pattern in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options?view=aspnetcore-10.0)
- [Compile-time options validation source generation](https://learn.microsoft.com/dotnet/core/extensions/options-validation-generator)
- [Custom configuration provider](https://learn.microsoft.com/dotnet/core/extensions/custom-configuration-provider)
- [Safe storage of app secrets in development](https://learn.microsoft.com/aspnet/core/security/app-secrets?view=aspnetcore-10.0)
- [Azure Key Vault configuration provider](https://learn.microsoft.com/aspnet/core/security/key-vault-configuration?view=aspnetcore-10.0)
- [Azure App Configuration .NET provider reference](https://learn.microsoft.com/azure/azure-app-configuration/reference-dotnet-provider)
- [Azure App Configuration best practices](https://learn.microsoft.com/azure/azure-app-configuration/howto-best-practices)
- [Breaking change: null values preserved in configuration](https://learn.microsoft.com/dotnet/core/compatibility/extensions/10.0/configuration-null-values-preserved)
- [File-based apps](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps)
