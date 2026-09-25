# d03 — Arrays don't merge the way you think

**One idea:** An environment file overlays an array index by index; it does not replace it.

## Run

```powershell
cd demos/d03-array-merge
dotnet run
```

## What you should see

`["a","b","c"]` in the base file, `["x"]` in the environment file, and `x, b, c` in the app:

```text
   appsettings.json               https://a.example.com, https://b.example.com, https://c.example.com
   appsettings.Development.json   https://x.example.com

   WHAT YOUR APP BINDS

     Weather:AllowedOrigins:0        = https://x.example.com    from appsettings.Development.json
     Weather:AllowedOrigins:1        = https://b.example.com    from appsettings.json
     Weather:AllowedOrigins:2        = https://c.example.com    from appsettings.json
```

Then the same section keyed by name, where the overlay replaces `primary` and leaves `partner` and
`admin` alone.

## Talk notes

- SPEC §4.1, and anti-pattern #10 — assuming an array in `appsettings.Production.json` replaces the
  base array. It doesn't, and the two stale entries you inherited are still allowed origins.
- The reason is §2's mental model, not a special array rule: an array is stored as the keys
  `...:0`, `...:1`, `...:2`, and the overlay only set `:0`. Precedence then runs per key.
- The fix on the slide is the second block: key the section by name. `primary` overriding `primary`
  is a statement of intent; `0` overriding `0` is an accident waiting for someone to reorder the
  base file.
- An empty array in the overlay does not clear anything either: `[]` contributes no keys, so there
  is nothing to overlay and all three base entries survive. There is no "replace this section"
  operation in the configuration model — only per-key writes.
