# d14 — Named options for "same shape, several times"

**One idea:** One options class, several configured instances, retrieved by name.

## Run

```bash
cd demos/d14-named-options
dotnet run
```

## What you should see

```text
  d14 — one options class, several configured instances


  monitor.Get("primary")            https://primary.api.example       5 s

  monitor.Get("secondary")          https://secondary.api.example    60 s

  monitor.Get(Options.DefaultName)  (never configured)               30 s


  One class, one injection. No EndpointOptionsPrimary, no EndpointOptionsSecondary.
```

Two endpoints resolved from one type, plus the unnamed instance sitting at its declared defaults.

## Talk notes

Maps to **SPEC §5.4**. Cut in the 60 if pressed.

- The real-world shape is multiple API clients, multiple queues, multiple database targets — same
  settings, different values. Without named options people copy the class and suffix it, and now
  the two drift.
- `Configure<T>(name, section)` registers the named instance; `IOptionsMonitor<T>.Get(name)` reads
  it. `IOptionsSnapshot<T>` has `Get(name)` too. `IOptions<T>` does **not** — it only ever hands
  back the default instance, which is why `Router` takes a monitor.
- The third line is the point about `Options.DefaultName`: the unnamed instance is not a special
  case, it is the name `""`. `Configure<T>(section)` with no name is exactly
  `Configure<T>(Options.DefaultName, section)`. Nothing configured it here, so it comes back with
  the defaults declared on the class.
- Option names are **case-sensitive strings**, and `Get("Primary")` on a registration made as
  `"primary"` returns an unconfigured instance rather than throwing — the same silent failure the
  third line is showing. That is why the names are `const` on `EndpointOptions`.
- Worth saying out loud: `BaseUrl` is `required`, and the unnamed instance still came back with it
  unset. `required` is a promise to the C# compiler, not to the configuration binder — d12's
  `ValidateOnStart()` is what turns that into a failure instead of a null.
