# d16 — Validation in a class

**One idea:** Cross-field and service-dependent rules belong in an `IValidateOptions<T>`, not in an
attribute.

## Run

```bash
cd demos/d16-validate-options-class
dotnet run                                  # 3 failures, app refuses to start
dotnet run -- --Fix true                    # same rules, good config, starts
dotnet run -- --Registration tryaddsingleton  # the registration mistake: one validator vanishes
```

## What you should see

The first run:

```
  Registration:   tryaddenumerable   →  2 validator(s) will run

  REFUSED TO START.  3 failures:

      • Weather:ApiBaseUrl must use HTTPS (found 'http').
      • Weather:ApiBaseUrl points at loopback ('localhost') in Production.
      • Weather timeout budget is 80s (20s x 4 attempts); the limit is 30s.
```

`--Fix true` prints `STARTED.  Every rule satisfied.`

`--Registration tryaddsingleton` prints `1 validator(s) will run` and only **2** failures — the
timeout-budget rule is never evaluated.

## Talk notes

Maps to **SPEC §5.5**.

None of these three rules is expressible as an attribute:

- **HTTPS-only** could almost be a `[RegularExpression]`, but the message would be useless.
- **No loopback in Production** depends on `IHostEnvironment` — a service. An attribute cannot
  reach the container. `WeatherEndpointValidator` takes it as a primary-constructor parameter.
- **Timeout budget** is cross-field: 20 seconds is fine, 3 retries is fine, `20 × 4 = 80s` is not.
  An attribute sees one property at a time.

**Why `TryAddEnumerable`.** Options resolves `IEnumerable<IValidateOptions<T>>`, so validators are a
collection and all of them run, with the failures aggregated into one
`OptionsValidationException`. `TryAddEnumerable` appends, and dedupes on the *implementation* type
so a double `AddOptions` call cannot make the same validator run twice.

The trap the third command shows is `TryAddSingleton`: it keeps the first registration for the
service type and silently drops every later one. You get a green-looking app with half its rules
switched off, and nothing anywhere says so. Plain `AddSingleton` does append correctly — it just
gives you no protection against registering the same validator twice.

Pair with d12: `ValidateOnStart()` is what turns any of this into a startup failure instead of a
surprise on the first request that happens to touch `.Value`.
