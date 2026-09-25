# d04 — Environment variable keys

**One idea:** `:` is not portable in an environment variable name — `__` is, and it maps to `:`.

## Run

PowerShell:

```powershell
cd demos/d04-env-var-keys
$env:Weather__ApiBaseUrl      = "https://api.prod.example.com"
$env:Weather__AllowedOrigins__0 = "https://a.example.com"
$env:Weather__AllowedOrigins__1 = "https://b.example.com"
$env:MYAPP_Weather__TimeoutSeconds = "7"
dotnet run
./reset.ps1
```

bash:

```bash
cd demos/d04-env-var-keys
export Weather__ApiBaseUrl="https://api.prod.example.com"
export Weather__AllowedOrigins__0="https://a.example.com"
export Weather__AllowedOrigins__1="https://b.example.com"
export MYAPP_Weather__TimeoutSeconds="7"
dotnet run
unset Weather__ApiBaseUrl Weather__AllowedOrigins__0 Weather__AllowedOrigins__1 MYAPP_Weather__TimeoutSeconds
```

## What you should see

```text
  d04 — environment variables -> configuration keys

  ENVIRONMENT VARIABLE              CONFIGURATION KEY           VALUE
  --------------------------------  --------------------------  --------------------------
  Weather__AllowedOrigins__0        Weather:AllowedOrigins:0    https://a.example.com
  Weather__AllowedOrigins__1        Weather:AllowedOrigins:1    https://b.example.com
  Weather__ApiBaseUrl               Weather:ApiBaseUrl          https://api.prod.example.com
  MYAPP_Weather__TimeoutSeconds     Weather:TimeoutSeconds      7

  One variable, two providers:   MYAPP_Weather__TimeoutSeconds

    AddEnvironmentVariables()             ->  MYAPP_Weather:TimeoutSeconds    7
    AddEnvironmentVariables("MYAPP_")     ->  Weather:TimeoutSeconds          7   <- prefix stripped
```

With no `Weather__*` variables set it prints a single line saying so, and exits.

## Talk notes

Maps to [SPEC §4.2](../../SPEC.md).

- **`__` is the portable spelling of `:`.** Colons are legal in a Windows environment variable name
  and illegal in a POSIX shell one, so `__` is what actually survives a Dockerfile, a Helm chart, and
  an App Service app setting. Write `__` everywhere and stop thinking about it.
- **Array indexes are just another key segment.** `Weather__AllowedOrigins__0` becomes
  `Weather:AllowedOrigins:0`. There is no array type in configuration — only keys that happen to be
  numbers, which is why arrays overlay index-by-index instead of replacing (`d03`).
- **A prefix isolates one app on a shared host and is stripped from the key.** The last two lines are
  the same variable read twice: the plain provider keeps `MYAPP_` in the key, the prefixed one does
  not. That is the whole feature.
- `DOTNET_` and `ASPNETCORE_` are reserved for host settings — don't reuse them for app config.
