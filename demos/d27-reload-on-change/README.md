# d27 — reloadOnChange

**One idea:** File edits reload a running app; environment variables and command-line args never do.

## Run

```bash
cd demos/d27-reload-on-change
dotnet run
```

The demo drives itself — it edits `appsettings.json`, waits for the watcher, then changes the
environment variable and waits the same amount again. Nothing to type, about four seconds.

## What you should see

```
  STEP 1    as the app started

      Greeting:FromFile    v1 - from appsettings.json
      Greeting:FromEnv     v1 - from the environment variable

  STEP 2    appsettings.json edited on disk

      Greeting:FromFile    v2 - after the file was edited
                           CHANGED after 0.3s - no restart

      OnChange fired 2 time(s) for that one edit.

  STEP 3    GREETING__FROMENV changed, waited 2.0s

      Greeting:FromEnv     v1 - from the environment variable
                           UNCHANGED - still the startup value
```

Two changes, one takes effect and one doesn't.

`appsettings.json` is rewritten and restored on exit. If you Ctrl-C mid-run, `./reset.ps1` puts it
back.

## Talk notes

Maps to **SPEC §8**. Can be merged into d13 if time is tight — same setup, and d13 already has an
`IOptionsMonitor` on screen.

Two keys, deliberately, so precedence doesn't muddy the point: `Greeting:FromFile` only ever comes
from JSON and `Greeting:FromEnv` only from the environment. Put them on one key and the environment
variable wins, and the file edit becomes invisible for the wrong reason.

The mechanism: a file change reloads **that provider** and raises the root's reload token. Nothing
re-runs the environment variables provider, which read `Environment.GetEnvironmentVariables()` once
during startup and has held the same dictionary ever since. Hence the container rule — **changing an
environment variable on a running container does nothing. Restart it.**

**The double fire.** One `File.WriteAllText` fires `OnChange` twice here, near enough every run —
the write and the metadata update are two events to a `FileSystemWatcher`. Usually two, occasionally
one, and that variance is the point: anything expensive hanging off `OnChange` needs debouncing, and
anything non-idempotent needs it badly. Note the subscription is held in a `using` — `OnChange`
returns a registration and dropping it on the floor leaks the callback.

Three more things to say out loud, all of them consequences rather than bugs:

- `reloadOnChange` on a Kubernetes ConfigMap mount is unreliable. Those are symlink swaps, not
  in-place writes, and the watcher may or may not see them. Treat restart as the contract.
- Key Vault only reloads if you set `ReloadInterval`. The default is never.
- **Config reloaded ≠ app reconfigured.** Kestrel endpoints, the DI graph, and `HttpClient` handler
  pipelines all read their configuration once at startup. Reloading the file changes what
  `IOptionsMonitor` hands you and nothing else.
