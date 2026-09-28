# d11 — Bound options instead of `IConfiguration` injection

**One idea:** Stop passing `IConfiguration` around — bind a typed class and inject that.

## Run

```bash
cd demos/d11-options-binding
dotnet run
```

## What you should see

Two readings of the same setting, side by side:

```text
  d11 — bind a class, stop injecting IConfiguration


  BEFORE    WeatherClientBefore(IConfiguration config)

            config["Weather:TimeoutSeconds"]   ->   "45"      a string, re-read every call


  AFTER     WeatherClient(IOptions<WeatherOptions> options)

            options.Value.TimeoutSeconds       ->    45       an int, bound once

            ApiBaseUrl   https://api.weather.example
            Retries      3
            ApiKey       ************   (placeholder — real value lives in user secrets)


  Same value twice. Only one of them breaks the build when the key is renamed.
```

## Talk notes

Maps to **SPEC §5.1–5.2**. This is the setup for d12–d14, so keep it short and do not
demo validation here.

- Open `WeatherClientBefore.cs` on stage. Its constructor says `IConfiguration`, which tells a
  reader nothing: the class could be reading one key or forty. Every read carries a magic string, a
  `!`, and a parse — the only `!` operators in the whole demo set live in this file on purpose.
- Then open `WeatherClient.cs`. The constructor names exactly one dependency, and `WeatherOptions`
  is the complete list of what this feature can be configured with. There are zero strings in it.
- `WeatherOptions.SectionName` is the one place the string `"Weather"` appears, and
  `.Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))` is the only line that
  knows about JSON at all.
- `required` + `init` is the shape to copy: the binder still populates it, and nothing else can
  mutate it after startup.
- The rename argument is the one that lands: change `TimeoutSeconds` in `appsettings.json` and the
  "before" class compiles fine and breaks in production; the "after" class simply falls back to the
  default it declares. Validation (d12) is what turns that silence into a startup failure.
