# d24 — In-memory configuration for tests

**One idea:** Tests don't need a config file — add the last provider and win.

## Run

```bash
cd demos/d24-in-memory-tests
dotnet run --project tests
```

`dotnet test` does **not** work here without extra setup — see the note at the bottom.

## What you should see

```
=== TEST EXECUTION SUMMARY ===
   D24.InMemoryTests.Tests  Total: 2, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0
```

Two green tests. The app under test served `https://stub.invalid/weather` with a 1-second timeout —
values that appear in no file on disk. `appsettings.json` still says
`https://api.example.com/weather` and `30`, and the second test asserts exactly that: the committed
values are the ones that got beaten.

## Talk notes

Maps to **SPEC §4.8**. This is the "real-world patterns" promise in the abstract, and the payoff of
the precedence model from d02 — a test is just one more provider, added last.

`ApiFactory` is the whole idea:

```csharp
builder.ConfigureAppConfiguration(config =>
    config.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Weather:ApiBaseUrl"] = TestApiBaseUrl,
    }));
```

`ConfigureAppConfiguration` runs after the app's own providers, so the dictionary wins. No file on
disk is edited, nothing is mocked, and the app is exercised through its real configuration pipeline
rather than around it. Contrast with the two things people reach for instead: an
`appsettings.Testing.json` that has to be kept in sync, or mocking `IConfiguration`, which tests the
mock.

The visual here is weak — a green test summary is a green test summary. Keep it to ninety seconds
or make it a slide.

### Two .NET 10 details worth knowing

**`dotnet test` needs an opt-in.** xunit.v3 runs on Microsoft.Testing.Platform, and the .NET 10 SDK
no longer runs MTP projects through the VSTest-based `dotnet test`. The documented opt-in is a
`global.json` containing `{"test":{"runner":"Microsoft.Testing.Platform"}}` — which this repo's
demo conventions forbid, so this demo runs the test app directly instead. MTP test projects are
plain executables, so `dotnet run --project tests` runs the suite with no configuration at all.

**The parent project globs the tests folder.** `Microsoft.NET.Sdk.Web` picks up `**/*.cs`, which
swallows `tests/` and produces a pile of confusing errors in the *web* project. Hence
`<DefaultItemExcludes>$(DefaultItemExcludes);tests\**</DefaultItemExcludes>`. Any demo with a
`tests/` subfolder needs that line.
