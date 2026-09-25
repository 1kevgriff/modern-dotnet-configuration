# d23 — Key-per-file: the mount is the configuration

**One idea:** Docker and Kubernetes hand you secrets as one file per value, and
`AddKeyPerFile` turns that directory straight into configuration keys — file name is the
key, `__` is the section delimiter, file contents are the value.

## Run

In a container, where the mount really is `/run/secrets`:

```bash
cd demos/d23-key-per-file
docker compose up --build
```

Or with no Docker at all — the conference-wifi fallback:

```bash
cd demos/d23-key-per-file
dotnet run
```

Both run the same code against the same four files. `./reset.ps1` removes the container,
network and locally-built image afterwards.

## What you should see

Identical output either way, apart from the mount path:

```text
  MOUNT  /run/secrets   —   4 files, one value each

  ConnectionStrings__Primary
      ->  ConnectionStrings:Primary  =  Server=db;Database=weather

  Weather__ApiKey
      ->  Weather:ApiKey  =  wk_live_7f3a91c4

  Weather__Endpoint
      ->  Weather:Endpoint  =  https://weather.internal

  Weather__TimeoutSeconds
      ->  Weather:TimeoutSeconds  =  30

  every key above came from   KeyPerFileConfigurationProvider
```

`dotnet run` prints `./secrets (standing in for /run/secrets)` instead — the provider
requires an **absolute** path and `/run/secrets` doesn't exist on a laptop, so the demo
falls back to the copy of `./secrets` next to the built assembly. `SECRETS_PATH`
overrides it; the Dockerfile sets it to `/run/secrets`.

The compose file uses real **Compose secrets**, not a bind mount that imitates them — so
the mechanism on screen is the mechanism in production.

## Talk notes

- **SPEC §4.7.** This is the container answer to "where do secrets come from in
  production" — no Azure round trip, no SDK, no credential. The orchestrator writes files;
  the app reads configuration.
- The `__` → `:` mapping is the same translation environment variables use (§4.2), for the
  same reason: `:` is illegal in a filename on some platforms and in an environment
  variable name on all of them.
- Point at the last line. The value arrived as a *configuration key*, so everything
  downstream — `IOptions` binding, validation, `GetConnectionString()` — works unchanged.
  Swapping Key Vault (d18) for a K8s mount is a one-line change at the composition root.
- **`optional: false` here on purpose**, and this is a deliberate departure from the
  `optional: true` in the spec snippet: if the mount is missing in production you want a
  startup crash, not an app serving traffic with no credentials. `true` is right only when
  the mount is genuinely an optional overlay.
- Kubernetes projects a `Secret` into a volume exactly this way — one key per file — so
  the same three lines cover Docker Compose, Swarm and K8s.
