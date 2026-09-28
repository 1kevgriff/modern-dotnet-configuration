# d26 — The same model, with no web in sight

**One idea:** Off the web the configuration model does not change — only who assembles the chain
does, and a desktop app has to say where "here" is.

## Run

```powershell
cd demos/d26-non-web-hosts/worker
dotnet run
$env:DOTNET_ENVIRONMENT = 'Staging'; dotnet run

cd ../desktop
dotnet run
cd ..
dotnet run --project desktop      # same binary, different working directory

./reset.ps1
```

`reset.ps1` clears `DOTNET_ENVIRONMENT` so the next demo does not inherit `Staging`.

## What you should see

The worker, with the full default chain built for it:

```text
info: worker[1] Host           Host.CreateApplicationBuilder - no ASP.NET Core
info: worker[2] Environment    Production (DOTNET_ENVIRONMENT, not ASPNETCORE_)
info: worker[3] Endpoint       https://api.example.com/weather
info: worker[4] Timeout        30s
info: worker[5] Provider       MemoryConfigurationProvider
info: worker[5] Provider       MemoryConfigurationProvider
info: worker[5] Provider       EnvironmentVariablesConfigurationProvider Prefix: 'DOTNET_'
info: worker[5] Provider       JsonConfigurationProvider for 'appsettings.json' (Optional)
info: worker[5] Provider       JsonConfigurationProvider for 'appsettings.Production.json' (Optional)
info: worker[5] Provider       JsonConfigurationProvider for 'D26.NonWebHosts.Worker.settings.json' (Optional)
info: worker[5] Provider       JsonConfigurationProvider for 'D26.NonWebHosts.Worker.settings.Production.json' (Optional)
info: worker[5] Provider       EnvironmentVariablesConfigurationProvider
```

With `DOTNET_ENVIRONMENT=Staging`, two lines move: `Environment` becomes `Staging`, `Endpoint`
becomes `https://staging.example.com/weather`, and the `appsettings.Production.json` provider is
replaced by `appsettings.Staging.json`. Nothing in the code changed.

The desktop app, run from its own folder — both base paths happen to work:

```text
DESKTOP APP   no host, IConfiguration built by hand

    AppContext.BaseDirectory          ...\desktop\bin\Debug\net10.0\
    Directory.GetCurrentDirectory()   ...\desktop

Weather:Endpoint, resolved from each of those two base paths

    from base directory      https://api.example.com/weather
    from working directory   https://api.example.com/weather

PROVIDERS   (the same chain a worker gets, just assembled by hand)

    JsonConfigurationProvider for 'appsettings.json' (Required)
    JsonConfigurationProvider for 'secrets.json' (Optional)
    EnvironmentVariablesConfigurationProvider

IConfiguration has no write API. Per-user settings would live in
    C:\Users\<you>\AppData\Roaming\D26.NonWebHosts.Desktop\user.json
```

Then run the **same binary** from the parent folder and one line changes:

```text
    from base directory      https://api.example.com/weather
    from working directory   (no appsettings.json on this path)
```

That is the whole desktop lesson in one line. The app did not move; the working directory did.

## Talk notes

- **SPEC §4.11.** Repo-only — compress to one slide in the 60 and the 45.
- **The model is identical, the assembly is not.** `Host.CreateApplicationBuilder` gives a worker the
  same JSON → user secrets → environment → command line chain, in the same order, that a web app
  gets. A desktop app has no host, so somebody has to write the `ConfigurationBuilder` by hand — the
  chain is the same chain.
- **`DOTNET_ENVIRONMENT`, not `ASPNETCORE_ENVIRONMENT`.** Outside ASP.NET Core the `ASPNETCORE_`
  prefix does nothing, and this is the single most common way a worker ends up silently running in
  `Production` on a developer's laptop. Note the worker's own dump names the prefix it reads.
- **`SetBasePath(AppContext.BaseDirectory)` is the desktop line to remember.** A double-clicked EXE
  inherits its working directory from whatever launched it — Explorer, a shortcut, a scheduled task.
  `dotnet run` hides this by starting you in the project folder, which is exactly why the second run
  from a different directory is worth doing live.
- **`IConfiguration` has no write API.** It is read-optimized, and per-user preferences are a
  different problem: a JSON file under `ApplicationData` that you load as an extra provider and save
  yourself. Say this before someone asks how to "save a setting."
- **A client binary holds no cloud secrets.** Stage three of the spine does not exist here — there is
  no orchestrator setting environment variables and no central store the app should authenticate to.
  A desktop app authenticates the *user* and calls a backend that holds the secret. Feature flags are
  the interesting exception: a desktop app can consume flags from a file or from your own backend.
- **The two `MemoryConfigurationProvider` entries** are the host's own doing — one is empty, the
  other carries `contentRoot`. Worth a sentence only if someone asks why the list is longer than the
  five sources in SPEC §3.1.
- **`{ApplicationName}.settings.json` is real**, not a typo in the output.
  `HostApplicationBuilder` probes it alongside `appsettings.json` whenever `ApplicationName` is not
  empty. SPEC §3.1's table does not list it.
