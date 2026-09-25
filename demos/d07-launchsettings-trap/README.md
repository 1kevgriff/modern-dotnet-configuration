# d07 — launchSettings.json beats your machine env vars

**One idea:** `launchSettings.json` overrides the environment variables on your machine — and it
never deploys, so production sees the machine value you were never testing with.

## Run

```powershell
cd demos/d07-launchsettings-trap
$env:Weather__ApiBaseUrl = "https://machine.example.com"
dotnet run                       # launchSettings.json wins
dotnet run --no-launch-profile   # the machine environment variable wins
./reset.ps1
```

## What you should see

Same command, two answers, one flag apart:

```text
   LAUNCH PROFILE   d07

   Weather:ApiBaseUrl   =   https://launchsettings.example.com
```

```text
   LAUNCH PROFILE   (none)

   Weather:ApiBaseUrl   =   https://machine.example.com
```

`dotnet run` also prints `Using launch settings from ...\Properties\launchSettings.json` on the first
run — the file announces itself, and nobody reads it.

## Talk notes

- SPEC §4.2, the "gotcha" callout. The #1 "works on my machine" configuration story.
- The mechanism is not a provider: `dotnet run` (and F5) **injects the profile's
  `environmentVariables` into the process environment** before the app starts, replacing any value
  the machine or your shell had. Configuration then reads one environment variables provider and
  cannot tell the difference — which is why the demo prints `DOTNET_LAUNCH_PROFILE` (set by the SDK
  since .NET 8) rather than a provider name.
- `Properties/launchSettings.json` is a development-only file. It is not copied to the output
  directory, it is not published, and CI does not use it. Everything it sets is a local-only truth.
- Two habits that pay for themselves: `dotnet run --no-launch-profile` before you believe a config
  bug is fixed, and keeping launchSettings to `ASPNETCORE_ENVIRONMENT`, ports, and nothing else.
- Pair with d01 on stage — the debug view names the provider, but for this one the provider is the
  same either way and only the value moves.
