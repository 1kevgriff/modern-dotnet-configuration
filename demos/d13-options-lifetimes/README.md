# d13 — `IOptions` vs `IOptionsSnapshot` vs `IOptionsMonitor`

**One idea:** Three interfaces, three different answers, at the same moment.

## Run

```bash
cd demos/d13-options-lifetimes
dotnet run
```

Then, in a second terminal:

```bash
curl http://localhost:5000/options

# edit appsettings.json — change Weather:TimeoutSeconds from 30 to 45 — and save
curl http://localhost:5000/options
```

Afterwards:

```powershell
./reset.ps1        # puts Weather:TimeoutSeconds back to 30
```

## What you should see

Before the edit, all four lines agree:

```text
  d13 — three interfaces, one moment in time      21:29:09


  IOptions<T>              30 s     singleton    bound once at startup, never again

  IOptionsSnapshot<T>      30 s     scoped       re-bound once per request

  IOptionsMonitor<T>       30 s     singleton    re-bound when appsettings.json changes


  DON'T: cached field      30 s     singleton    CurrentValue read once — a monitor that stopped moving
```

Save the file, and the app logs the change in the first terminal:

```text
info: d13[807367355] appsettings.json changed — timeout is now 45s
```

The very next request splits the four lines in two:

```text
  IOptions<T>              30 s     singleton    bound once at startup, never again

  IOptionsSnapshot<T>      45 s     scoped       re-bound once per request

  IOptionsMonitor<T>       45 s     singleton    re-bound when appsettings.json changes


  DON'T: cached field      30 s     singleton    CurrentValue read once — a monitor that stopped moving
```

`IOptions<T>` never moves again for the life of the process. Ever.

## Talk notes

Maps to **SPEC §5.3**. THE centerpiece — budget 6 minutes, and put the editor and the terminal
side by side on a second monitor so the save and the refresh are one gesture. Never cut.

- The table is the takeaway: `IOptions<T>` is a singleton bound once; `IOptionsSnapshot<T>` is
  **scoped** and re-binds per request; `IOptionsMonitor<T>` is a singleton that re-binds on change
  and can call you back.
- Do the curl twice *before* editing, so the room sees the values are stable and the demo isn't
  just a clock.
- **Classic bug #1** — injecting `IOptionsSnapshot<T>` into a singleton. It's scoped, so the
  container either throws during scope validation or you silently capture the first scope's value
  forever. There is no line for it on screen because the app wouldn't start; say it out loud.
- **Classic bug #2** is the fourth line, live. `CachedTimeout` holds a real `IOptionsMonitor<T>`
  and still never moves, because it read `CurrentValue` once at construction. That is a monitor
  demoted back to an `IOptions<T>` by one field initializer.
- The `OnChange` registration in `Program.cs` is an `IDisposable`, held in a `using` and disposed
  on shutdown. Skipping that leaks the subscription — the point the spec makes in §5.3.
- Expect the change callback to fire **twice** on a single save. That is the file watcher seeing
  two writes from the editor, not a bug in your code. Mention it before someone in the front row
  does.
- The log line is source generated (`Log.cs`) rather than a `logger.LogInformation(...)` call —
  the web SDK's analyzers reject the direct call under warnings-as-errors (CA1848), and the
  generated form is the structured-logging shape worth copying anyway.
