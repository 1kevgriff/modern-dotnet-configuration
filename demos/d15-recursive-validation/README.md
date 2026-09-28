# d15 — Recursive validation

**One idea:** DataAnnotations stop at the top level unless you tell them to recurse.

## Run

```bash
cd demos/d15-recursive-validation
dotnet run                       # invalid nested config starts anyway
dotnet run -- --Recursive true   # identical config now refuses to start
```

## What you should see

The first run prints:

```
  Options class:   FlatAppOptions        no recursion attributes

  STARTED.  Three broken values, zero complaints.
```

The second run, against the **same** `appsettings.json`, prints:

```
  Options class:   RecursiveAppOptions   [ValidateObjectMembers] + [ValidateEnumeratedItems]

  REFUSED TO START.  3 failures:

      • DataAnnotation validation failed for 'RecursiveAppOptions.Database' members: 'ConnectionString' ...
      • DataAnnotation validation failed for 'RecursiveAppOptions.Database' members: 'CommandTimeoutSeconds' ...
      • DataAnnotation validation failed for 'RecursiveAppOptions.Servers[0]' members: 'Port' ...
```

## Talk notes

Maps to **SPEC §5.5**. Pairs with d12 — d12 shows that validation can stop a bad deploy, d15 shows
how much of your config that validation never actually looked at.

`[Required]` on a nested object only asserts the object is non-null. `Validator.TryValidateObject`
does one level and stops, so every `[Required]` and `[Range]` *inside* `DatabaseOptions` and inside
each `ServerOptions` is dead weight. Two attributes fix it:

- `[ValidateObjectMembers]` — recurse into the nested object.
- `[ValidateEnumeratedItems]` — validate each item of the collection.

`FlatAppOptions` and `RecursiveAppOptions` are identical apart from those two attributes; put them
side by side on the slide.

Worth saying out loud: these attributes are honoured **at runtime** by `ValidateDataAnnotations()`,
not only by the `[OptionsValidator]` source generator. The generator (d17) produces the same
recursion without reflection, but you do not need it to get this behaviour.

The failure mode this prevents is the quiet one — a connection string that binds to `""` and a
timeout of 999 both look fine at startup and fail hours later in whichever code path touches them
first.
