# Worker brief — building the demos

Read this fully before writing anything. It is the contract every demo must satisfy.

## Non-negotiables

1. **Every demo is fully self-contained.** No `Directory.Build.props`, no `Directory.Packages.props`,
   no shared helper project, no solution file, no `global.json`. Someone must be able to copy your
   demo folder anywhere on disk and run it. If two demos need the same 15-line helper, **copy it**.
   Duplication is correct here.
2. **Do not create, edit, or delete anything outside the demo folders assigned to you.** Not
   `SPEC.md`, not `demos/README.md`, not another worker's folder. If you believe a shared file is
   needed, stop and report it instead.
3. **One idea per demo.** If a demo needs two paragraphs to explain what it shows, it's two demos —
   report that instead of merging concerns.
4. **It must actually run.** `cd` into the folder, `dotnet run`, and see the intended output. A demo
   that only compiles is not done.

## Folder and naming

- Folder: `demos/d{NN}-{kebab-slug}` — exactly the name given in your assignment. Never rename.
- Project: `D{NN}.{PascalSlug}.csproj`, `RootNamespace` and `AssemblyName` matching the stem.
- Multi-project demos use role subfolders: `d26-non-web-hosts/worker/`, `d26-non-web-hosts/desktop/`.
- Test projects: `d{NN}-{slug}/tests/D{NN}.{PascalSlug}.Tests.csproj`.

## Project file template

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
    <RootNamespace>D01.ProviderDump</RootNamespace>
    <AssemblyName>D01.ProviderDump</AssemblyName>
  </PropertyGroup>

</Project>
```

Use `Microsoft.NET.Sdk.Web` where the demo is a web app. Pin every `PackageReference` to an explicit
version — resolve versions with `dotnet package add <id>`, which writes the current version for you.

**If NuGet restore fails with NU1507 or tries to hit a private feed**, drop a `nuget.config` *inside
your demo folder* pinning nuget.org only. That keeps the demo self-contained:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

## Code conventions (house rules — these are enforced)

- Nullable enabled, **warnings as errors**. A demo that builds with warnings teaches the warning.
- `sealed` by default; file-scoped namespaces; one type per file. Top-level statements with no
  namespace are fine for a single-`Program.cs` demo.
- Primary constructors for dependency capture. `required` + `init` on options classes.
- **Structured logging only**: `logger.LogInformation("Timeout is {Timeout}s", timeout)`. Never
  interpolate into a log message. `Console.WriteLine` is fine in console demos with no host.
- Hold and dispose `IDisposable` registrations — `IOptionsMonitor.OnChange` returns one.
- No `!` null-forgiving operator, except where a demo deliberately shows bad code (clearly commented
  `// DON'T:`).
- Async all the way where I/O appears; flow and honor `CancellationToken`.
- Inject `TimeProvider` rather than reading the system clock, wherever a demo depends on time.
- **No secrets in any committed file.** Placeholders in `appsettings.json`; real values come from
  user secrets or environment variables at runtime.

## Required files per demo

- `README.md` — this exact shape:
  - `# dNN — <title>`
  - **One idea:** a single sentence.
  - **Run:** a fenced block with the exact commands, in order.
  - **What you should see:** the expected output, concretely.
  - **Talk notes:** the point being made, and the SPEC section it maps to.
- `reset.ps1` — only if the demo mutates state (user secrets, environment variables, edited JSON).
  It must restore the demo to its committed starting state.
- The demo code itself.

## Stage presentation rules

These are demos performed live on a projector, not sample apps:

- **Output must be readable from the back of a room.** Few lines, wide spacing, clear labels. Print
  the thing the demo is about and little else.
- **The interesting value should be visually obvious** — label it, or print it alone.
- Prefer a console app over a web app unless the demo genuinely needs HTTP.
- No interactive prompts and no waiting on input. Everything is driven by arguments or environment.
- Keep total runtime under ~5 seconds unless the demo is specifically about reload behavior.

## Definition of done, per demo

1. `dotnet build` from the demo folder: **zero warnings, zero errors**.
2. `dotnet run` (with the documented commands) produces the documented output.
3. `dotnet format --verify-no-changes` is clean.
4. `README.md` is complete and accurate.
5. Nothing outside your assigned folders changed.

## Reporting back

When all your demos are done, print a plain-text summary to stdout:

- One line per demo: `dNN <status> — <what it prints>`
- Any demo you could not complete and exactly why.
- Anything you think is wrong with the spec's plan for your demos (disagreement is useful — say so
  rather than silently working around it).

Do not mark a demo done if it does not build and run. Report the failure instead.
