# d25 — Write your own configuration provider

**One idea:** The provider chain is open — a source, a provider, and an `AddXxx` method are all it
takes to make your own file format a first-class configuration source.

## Run

```powershell
cd demos/d25-custom-provider
dotnet run
./reset.ps1
```

`reset.ps1` matters: the demo rewrites `settings.env` on every run so the reload is guaranteed to
happen on stage instead of depending on you typing into an editor.

## What you should see

```text
PROVIDERS

    JsonConfigurationProvider for 'appsettings.json' (Required)
    .env file: settings.env

GetDebugView()   key = value   (the provider that supplied it)

Greeting:
  Audience=everyone (JsonConfigurationProvider for 'appsettings.json' (Required))
  Message=Hello from the .env file (.env file: settings.env)

LIVE EDIT   (this demo rewrites settings.env for you)

    before   Hello from the .env file
    after    Reloaded at 21:27:31

settings.env is now edited - run ./reset.ps1 to put it back.
```

Two things to point at. First, `.env file: settings.env` appears in the `d01` dump next to
`Greeting:Message` — a hand-written provider is indistinguishable from a built-in one, and it wins
the key because it was added last. Second, `after` shows a value that did not exist when the process
started: `OnReload()` fired, `IOptionsMonitor` heard it.

The timestamp changes every run; everything else is stable.

## Talk notes

- **SPEC §4.10.** Cut first in the 60 and the 45 — point at the repo instead.
- **Three types, about 30 lines.** `DotEnvConfigurationSource` is registration (settings plus
  `Build`), `DotEnvConfigurationProvider` does the work, `AddDotEnvFile` makes it look built-in.
  That is the entire extensibility contract.
- **`Data` is the whole API.** It is a protected `IDictionary<string, string?>` on
  `ConfigurationProvider`. Populate it in `Load()` and you are done. Replacing the dictionary rather
  than mutating it is deliberate — it means a key deleted from the file actually disappears.
- **`OnReload()` is the line that matters.** `Load()` alone updates `Data` and tells nobody.
  `OnReload()` fires the change token, which is what wakes `IOptionsMonitor`. Comment it out and the
  demo prints the same value twice — a good thing to do live.
- **`ToString()` is not cosmetic.** It is what `GetDebugView()` prints in parentheses, so a provider
  without one shows up as a bare type name in the diagnostic the whole talk depends on.
- **The 250 ms reload delay is real.** The built-in `FileConfigurationSource` has the same thing:
  the file-changed event arrives before the writer has flushed, so reloading immediately reads a
  half-written file. Anyone who has written a file watcher recognises this.
- **`Load()` runs at build time, on the calling thread.** A provider that throws in `Load()` takes
  the app down at startup — sometimes exactly what you want (see `d12`).
- **Why `.env` and not SQL.** The spec offers a database-backed provider as the alternative. `.env`
  needs no database on stage and the parsing is four lines, which keeps attention on the contract
  rather than on `SqlCommand`. The shape is identical: swap `File.ReadAllLines` for a query and add
  a timer that calls `Load(); OnReload();`.
- **Base path here is the working directory**, so the file you edit on stage is the one in the repo.
  A published app wants `AppContext.BaseDirectory` — `d26` is where that distinction gets made.
