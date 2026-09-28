# d17 — Source generators

**One idea:** Binding and validation default to reflection; two opt-ins remove it.

## Run

```bash
cd demos/d17-source-generators
dotnet build --no-incremental                        # 3 warnings: IL2026, IL3050
dotnet build --no-incremental -p:UseGenerators=true  # 0 warnings
```

`--no-incremental` matters: analyzer warnings are only reported on a compile that actually happens.

To see the app run either way:

```bash
dotnet run                        # generators OFF
dotnet run -p:UseGenerators=true  # generators ON
```

The full AOT proof, if you have the native toolchain and the time, is
`dotnet publish -c Release` — same warnings, ninety seconds instead of two.

## What you should see

```
BEFORE:  3 Warning(s)   — IL2026, IL3050
AFTER:   0 Warning(s)
```

Both runs print the same bound values. The payoff is the warning count, not the output.

## Verifying this demo

`dotnet format --verify-no-changes` **fails here (exit 2), and that is correct.** `dotnet format`
runs the analyzers and counts every reported diagnostic as a change it wants — which for d17 means
the three IL2026/IL3050 warnings the demo exists to produce. Check formatting with the two
subcommands that exclude analyzers:

```bash
dotnet format whitespace --verify-no-changes   # clean
dotnet format style --verify-no-changes        # clean
```

Any repo-wide verify script needs that exception for d17, the same way the csproj needs its
`TreatWarningsAsErrors=false` exception.

## Talk notes

Maps to **SPEC §5.6**. This is the first demo to cut in the 60 — the punchline is a number in build
output, which does not carry a room the way d13 or d29 do.

Two opt-ins, and in .NET 10 they are no longer symmetric:

- **Binding** — `<EnableConfigurationBindingGenerator>`. .NET 10 already turns this **on for you**
  whenever `PublishAot` is set, so the "before" state in this csproj has to switch it off
  explicitly to show what reflection binding was costing. Worth saying out loud: the spec's framing
  of this as a manual opt-in is now out of date for AOT apps.
- **Validation** — `[OptionsValidator]` on an empty partial class. Still opt-in, still the source of
  the remaining IL2026. `ValidateDataAnnotations()` is reflection and stays reflection.

With `[OptionsValidator]` in play you do not also call `ValidateDataAnnotations()`.

### The trap worth its own slide

`WeatherOptions` uses `required string ApiBaseUrl { get; set; }` — **`set`, not `init`** — and that
is not a style choice. The configuration binding source generator **does not write to `init`-only
properties**. It does not warn, it does not error; it produces an object with every property left at
its default.

Turn the generator on for an options class written the idiomatic `required` + `init` way and the
result is a silent empty bind, caught only by whatever validation you happen to have. Here that
looked like:

```
OptionsValidationException: ApiBaseUrl: The WeatherOptions.ApiBaseUrl field is required.
```

...on config that is present and correct in `appsettings.json`. The reflection binder handles `init`
fine, which is why this only appears once you opt into AOT — the exact moment you are least able to
debug it.
