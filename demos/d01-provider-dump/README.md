# d01 — Where did this value come from?

**One idea:** Configuration knows which provider supplied every key, and `GetDebugView()` will tell
you.

## Run

```powershell
cd demos/d01-provider-dump
dotnet run
```

Optional follow-up, to watch a key change hands:

```powershell
dotnet run --Weather:TimeoutSeconds=5
```

## What you should see

```text
PROVIDERS   (added first to last - the last one to supply a key wins)

   1.  JsonConfigurationProvider for 'appsettings.json' (Required)
   2.  JsonConfigurationProvider for 'appsettings.Development.json' (Optional)
   3.  EnvironmentVariablesConfigurationProvider Prefix: 'DEMO_'
   4.  CommandLineConfigurationProvider

GetDebugView()   key = value   (the provider that supplied it)

ConnectionStrings:
  Default=Server=localhost;Database=Demo;Trusted_Connection=True (JsonConfigurationProvider for 'appsettings.json' (Required))
Weather:
  ApiBaseUrl=https://localhost:7104 (JsonConfigurationProvider for 'appsettings.Development.json' (Optional))
  ApiKey=*** (JsonConfigurationProvider for 'appsettings.json' (Required))
  TimeoutSeconds=30 (JsonConfigurationProvider for 'appsettings.json' (Required))
```

`ApiBaseUrl` is set in both JSON files and the environment file wins — named, in parentheses, with
no guessing. `ApiKey` prints as `***`. With the optional follow-up, `TimeoutSeconds` becomes `5` and
its provider changes to `CommandLineConfigurationProvider`.

## Talk notes

- SPEC §2.1. Never cut — this is the single most useful configuration diagnostic in .NET, and the
  rest of the talk reuses it as the answer to "how do you know?".
- The `processValue` callback takes a `ConfigurationDebugViewContext` carrying `Path`, `Key`,
  `Value`, and `ConfigurationProvider`. Redact before printing — anti-pattern #11 is logging a debug
  view raw.
- The provider chain here is built by hand rather than by `Host.CreateApplicationBuilder` for two
  reasons: the four providers are visible in `Program.cs`, and the environment variables provider
  carries a `DEMO_` prefix so the dump stays four keys long instead of listing every variable on the
  machine. On a real app, expect the unprefixed dump to be enormous — one more reason to redact.
- `config.Providers` enumerates the chain directly when you want the list without the values.
