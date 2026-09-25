# d06 — .NET 10: JSON `null` is preserved

**One idea:** A JSON `null` used to arrive as an empty string and get skipped; in .NET 10 it arrives
as `null` and binds, overwriting the default your class had.

## Run

```powershell
cd demos/d06-null-preserved
dotnet run -f net10.0
dotnet run -f net9.0
```

This is the only multi-targeted demo in the repo (`net9.0;net10.0`), so the contrast is one flag
apart. Plain `dotnet run` asks you which framework you meant.

## What you should see

Same JSON, same class, two answers:

```text
   RUNNING ON   .NET 10.0.11                  RUNNING ON   .NET 9.0.19

   WHAT THE JSON PROVIDER STORES              WHAT THE JSON PROVIDER STORES

     Weather:ApiBaseUrl   (null)                Weather:ApiBaseUrl   (empty)
     Weather:Tags:1       (null)                Weather:Tags:1       (empty)

   WHAT THE BINDER DOES WITH IT               WHAT THE BINDER DOES WITH IT

     Retries        (null)      default 3       Retries        3           default 3
     ApiBaseUrl     (null)                      ApiBaseUrl     (empty)
     Tags   alpha, (null), charlie              Tags   alpha, (empty), charlie
```

`Retries` is the line to point at: a nullable `int` that was `3` on .NET 9 and is `null` on .NET 10,
with nothing in the JSON changed. `ApiBaseUrl` is the other half of it — a **non-nullable** `string`
property holding `null` at runtime, which no compiler warning will ever tell you about.

## Talk notes

- SPEC §9, item 1 — the breaking change most likely to bite an upgrade.
  [Docs](https://learn.microsoft.com/dotnet/core/compatibility/extensions/10.0/configuration-null-values-preserved)
- Two changes, one line apart: the JSON **provider** stops converting `null` to `""`, and the
  **binder** stops skipping null values. The first shows up in `GetDebugView` (d01), the second in
  your options class.
- The .NET 9 story is not "the default survives", it's "the default survives *when the empty string
  can't convert*". `int?` kept its `3`; `string` quietly took the empty string. Both are wrong in
  different directions, which is why .NET 10 changed it.
- Where this bites: a shared `appsettings.json` that writes `"ApiKey": null` to mean "not set here,
  the environment supplies it". After the upgrade that `null` wins over the earlier provider it used
  to defer to, and validation (d12) is what catches it.
- The package version is what carries the behavior, not the runtime — see the two conditional
  `ItemGroup`s in the `.csproj`. Upgrading `Microsoft.Extensions.Configuration.*` to 10.x on a
  `net9.0` app moves you too.
