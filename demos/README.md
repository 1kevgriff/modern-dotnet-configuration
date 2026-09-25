# Demos — Modern .NET Configuration

33 demos, one idea each, every one runnable with a single command:

```bash
cd demos/d29-percentage-trap
dotnet run
```

**The number is an identity, not a running order.** `d13` is the options-lifetimes demo forever.
Running orders live in [SPEC §11.1–11.3](../SPEC.md); layout and naming rules in §11.4–11.5.
Solo project `modern-dotnet-configuration` holds the working notes for each.

Legend — **90 / 60 / 45**: appears in that running order · ★ never cut · ☁ needs Azure (record it)
· ✂ first to cut · 📦 repo only (not in any running order; it exists so people can read it later)

## The model — how configuration actually resolves

| Id | Folder | One idea | 90 | 60 | 45 | |
| --- | --- | --- | :-: | :-: | :-: | --- |
| d01 | `d01-provider-dump` | `GetDebugView` names the provider behind every key | ● | ● | ● | ★ cold open |
| d02 | `d02-precedence` | Last provider wins — same key, four winners | ● | ● | ● | ★ |
| d03 | `d03-array-merge` | Arrays overlay index-by-index, they don't replace | ● | ● | | |
| d06 | `d06-null-preserved` | .NET 10: JSON `null` now binds instead of being skipped | | | | 📦 breaking change |

## Stage 1 — local dev

| Id | Folder | One idea | 90 | 60 | 45 | |
| --- | --- | --- | :-: | :-: | :-: | --- |
| d07 | `d07-launchsettings-trap` | `launchSettings.json` beats your machine env vars | ● | | | |
| d09 | `d09-user-secrets` | Secrets live outside the project tree, Development only | ● | ● | | ★ |
| d10 | `d10-file-based-app` | .NET 10: user secrets in `dotnet run app.cs` | | | | 📦 |

## Options — binding and validation

| Id | Folder | One idea | 90 | 60 | 45 | |
| --- | --- | --- | :-: | :-: | :-: | --- |
| d11 | `d11-options-binding` | Bind a typed class; stop injecting `IConfiguration` | ● | | | |
| d12 | `d12-validate-on-start` | Bad config means the app refuses to start | ● | ● | | ★ |
| d13 | `d13-options-lifetimes` | `IOptions` vs `Snapshot` vs `Monitor`, live-edited | ● | ● | ● | ★ centerpiece |
| d14 | `d14-named-options` | One options class, several configured instances | | | | 📦 |
| d15 | `d15-recursive-validation` | Nested objects and collections aren't validated by default | | | | 📦 |
| d16 | `d16-validate-options-class` | `IValidateOptions` for rules attributes can't express | | | | 📦 |
| d17 | `d17-source-generators` | Source generators + `PublishAot` remove reflection | | | | ✂ 📦 |
| d24 | `d24-in-memory-tests` | In-memory provider is the test story | | | | 📦 |

## Stage 2 — deployment

| Id | Folder | One idea | 90 | 60 | 45 | |
| --- | --- | --- | :-: | :-: | :-: | --- |
| d04 | `d04-env-var-keys` | `__` maps to `:`; prefixes are stripped | | | | 📦 |
| d05 | `d05-connstr-prefixes` | .NET 10: 11 connection-string env prefixes, up from 4 | ● | ● | | |
| d08 | `d08-command-line` | Three arg syntaxes plus switch mappings | | | | 📦 |
| d18 | `d18-key-vault` | Key Vault + `DefaultAzureCredential`; `--` becomes `:` | ● | | | ☁ |
| d23 | `d23-key-per-file` | Docker/K8s mounted secrets, one file per value | ● | | | |

## Stage 3 — shared

| Id | Folder | One idea | 90 | 60 | 45 | |
| --- | --- | --- | :-: | :-: | :-: | --- |
| d19 | `d19-appconfig-labels` | One store, labels as the environment axis | | | | ☁ 📦 |
| d20 | `d20-appconfig-sentinel` | Sentinel key: multi-key changes land atomically | ● | ● | | ☁ |
| d21 | `d21-appconfig-flag-flip` | Flip a flag in the portal, no redeploy | | | | ☁ 📦 |
| d22 | `d22-appconfig-keyvault-refs` | App Config dereferences Key Vault secrets | | | | ☁ 📦 |
| d27 | `d27-reload-on-change` | Files reload; env vars and CLI args never do | | | | 📦 |

## Feature flags — ~25% of the talk

| Id | Folder | One idea | 90 | 60 | 45 | |
| --- | --- | --- | :-: | :-: | :-: | --- |
| d28 | `d28-flags-no-cloud` | A flag is just configuration — zero cloud required | ● | ● | ● | ★ |
| d29 | `d29-percentage-trap` | `Microsoft.Percentage` is per-call, not per-user | ● | ● | | ★ |
| d30 | `d30-flag-filters` | Time window, targeting, and a custom filter | ● | | | |
| d31 | `d31-flag-variants` | Flags that return a bound config section, not a bool | ○ | | | optional |
| d32 | `d32-flag-inventory` | You can't manage flag debt you can't see | ● | | | ★ closer |
| d33 | `d33-flag-testing` | Test both sides of every live flag | | | | 📦 |

## Reach and extensibility

| Id | Folder | One idea | 90 | 60 | 45 | |
| --- | --- | --- | :-: | :-: | :-: | --- |
| d25 | `d25-custom-provider` | `IConfigurationSource` + `OnReload` in ~30 lines | | | | ✂ 📦 |
| d26 | `d26-non-web-hosts` | Same model in a worker and on the desktop | | | | 📦 |

---

## Counts

- **33 demos** · 16 in the 90-minute cut (plus d31 optional) · 10 in the 60 · 4 in the 45
- **6 need Azure** (d18–d22) — all recorded to `recordings/` before the talk
- **7 never cut**: d01, d02, d09, d12, d13, d28, d29, d32
- **16 are repo-only** — they exist because the spec references them and people read this repo after
  the talk. They still have to build and run.

## Before the talk

```powershell
./verify.ps1        # dotnet build (zero warnings) + dotnet format --verify-no-changes
```

Pre-warm every project so no demo pays first-run restore on conference wifi.
