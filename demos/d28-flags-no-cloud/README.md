# d28 — Feature flags with zero cloud

**One idea:** A feature flag is just configuration — no Azure, no service, no new file format.

**Run:**

```powershell
cd demos/d28-flags-no-cloud
dotnet run

$env:feature_management__feature_flags__0__enabled = "true"
dotnet run

./reset.ps1
```

**What you should see:**

First run — the flag is off, and `appsettings.json` is the provider that said so:

```text
  D28 - a feature flag is just configuration

      NewCheckout   OFF
      BetaBanner    ON

      feature_management:feature_flags:0:enabled
        = False   <- JsonConfigurationProvider for 'appsettings.json' (Required)
```

Second run — same code, same JSON file, flag on, and a different provider won:

```text
      NewCheckout   ON
      BetaBanner    ON

      feature_management:feature_flags:0:enabled
        = true   <- EnvironmentVariablesConfigurationProvider
```

**Talk notes:**

- SPEC §6.1–6.2. Run this **before anyone sees Azure** — it is the demo that makes flags feel free.
- `Microsoft.FeatureManagement` is built on `IConfiguration`. `feature_management` /
  `feature_flags` is the Microsoft schema, shared with the Go, Python, and JavaScript libraries.
- The env var name is the punchline: `feature_management__feature_flags__0__enabled`. `__` maps to
  `:`, the array index is just a key segment, and last-provider-wins applies exactly as it does to
  a connection string. A flag inherits the whole provider chain — precedence, reload, debug view.
- The `WinningProvider` helper at the bottom of `Program.cs` is the same provider-walk as d01.
- No `conditions` on a flag means it is simply `enabled`. Filters arrive in d30.
