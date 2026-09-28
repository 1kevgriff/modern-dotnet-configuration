# d02 — Precedence: same key, four winners

**One idea:** The last provider added wins — one key, four sources, peeled back one layer at a time.

## Run

```powershell
cd demos/d02-precedence
dotnet run                                            # 30  - appsettings.json
$env:DOTNET_ENVIRONMENT = "Development"; dotnet run   # 120 - appsettings.Development.json
$env:Weather__TimeoutSeconds = "99"; dotnet run       # 99  - environment variable
dotnet run --Weather:TimeoutSeconds=5                 # 5   - command line
./reset.ps1
```

## What you should see

Four runs, four values, each naming the provider that supplied it:

```text
   ENVIRONMENT   Production

   Weather:TimeoutSeconds   =   30

   supplied by   JsonConfigurationProvider for 'appsettings.json' (Required)
```

then `120` from `appsettings.Development.json`, then `99` from
`EnvironmentVariablesConfigurationProvider`, then `5` from `CommandLineConfigurationProvider`. Each
layer stays in place; the newer one just wins. `./reset.ps1` clears both environment variables so the
demo starts clean next time.

## Talk notes

- SPEC §3.1. Never cut. Script the four commands so nothing is typed live.
- `Add` appends to a list. Reads walk that list **in reverse** and take the first hit — that is the
  whole of "last provider wins". `WinningProvider` in `Program.cs` is that loop, seven lines.
- `Weather__TimeoutSeconds` — `:` is not portable in environment variable names, `__` is, and maps
  to `:`.
- Command-line syntax: `--Key=Value`, `--Key Value`, `/Key Value`, or `Key=Value`. Don't mix styles
  in one command.
- The environment name is only a string. `appsettings.QA-East.json` works the same way if
  `DOTNET_ENVIRONMENT=QA-East`.
- Reload note for later (§8): the two JSON layers can reload at runtime; the environment variable and
  the command-line argument are read once at startup and never change.
