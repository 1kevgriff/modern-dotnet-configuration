# d33 — Testing both sides of a flag

**One idea:** A flag someone can flip at 2am is a branch you must have tested — both sides, every
time.

**Run:**

```powershell
cd demos/d33-flag-testing
dotnet run --project tests
```

Not `dotnet test` — see the note below.

**What you should see:**

```text
xUnit.net v3 In-Process Runner v3.2.2+728c1dce01 (64-bit .NET 10.0.11)
  Discovering: D33.FlagTesting.Tests
  Discovered:  D33.FlagTesting.Tests
  Starting:    D33.FlagTesting.Tests
  Finished:    D33.FlagTesting.Tests
=== TEST EXECUTION SUMMARY ===
   D33.FlagTesting.Tests  Total: 2, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0, Time: 0.719s
```

Two tests over one flag — on and off — with no configuration file involved on either side.

**Layout:**

```text
d33-flag-testing/
  app/     D33.FlagTesting.csproj        one endpoint, one flag, two branches
  tests/   D33.FlagTesting.Tests.csproj  one test per branch
           FlagFactory.cs                boots the app with the flag forced on or off
           CheckoutFlagTests.cs          the two tests
```

**Talk notes:**

- SPEC §6.7. Reinforces that flags are configuration — and **the flat-key form of the schema is the
  punchline**:

  ```csharp
  ["feature_management:feature_flags:0:id"] = "NewCheckout",
  ["feature_management:feature_flags:0:enabled"] = "true",
  ```

  That is the same JSON from d28, written as configuration keys, handed to
  `AddInMemoryCollection` from a `WebApplicationFactory<Program>` subclass that overrides
  `ConfigureWebHost` — the shape in SPEC §6.7. The in-memory provider is added last, so it wins by
  the ordinary last-provider-wins rule: the flag is overridden by precedence, not by a mock.
- `ConfigureAppConfiguration(config => …)` — the one-argument form the spec shows — needs
  `using Microsoft.AspNetCore.Hosting;`. Without it `IWebHostBuilder` binds the two-argument
  overload and you get CS1593.
- The alternative is substituting `IVariantFeatureManager` outright. Prefer the in-memory provider:
  it tests the flag *and* the wiring that reads it.
- Ten live flags is up to 1,024 nominal combinations and you test maybe three of them (d32). The
  rule that keeps this honest: **test both sides of every live flag**, or you are shipping an
  untested branch behind a switch someone can flip at 2am.
- `public sealed partial class Program;` in `app/Program.cs` is what lets `WebApplicationFactory`
  find the entry point of a top-level-statements app.
- **Run it with `dotnet run --project tests`, not `dotnet test`.** These are xunit v3 tests on
  Microsoft.Testing.Platform, and an MTP test project is an ordinary executable that hosts its own
  runner. On the .NET 10 SDK `dotnet test` will not execute them: without a `global.json` opting
  the repo into the MTP runner it fails outright, and with that opt-in it has been observed to
  exit 0 having run zero tests — a silent green, which is the worst possible failure mode for a
  test demo. Running the project directly reports the count every time (`Total: 2`) and exits
  non-zero when a test fails. Demos here are self-contained and carry no `global.json`, so direct
  execution is also the only option that keeps this folder copy-pasteable.
- The runner's output is a weak visual. Keep it to 60 seconds, or make it a slide in the
  60-minute cut. This demo is repo-only in every running order.
