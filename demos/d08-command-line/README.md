# d08 — Command line

**One idea:** the command line is the last provider — it wins — and switch mappings give a long key
a short alias.

## Run

```bash
cd demos/d08-command-line
dotnet run                                  # no args: the default, 30
dotnet run --Weather:TimeoutSeconds 5       # --Key value
dotnet run /Weather:TimeoutSeconds 5        # /Key value
dotnet run Weather:TimeoutSeconds=5         # Key=value
dotnet run -t 5                             # switch mapping
dotnet run --timeout 5                      # switch mapping
```

The same six lines work in PowerShell unchanged.

## What you should see

Every one of the last five prints `5`; the bare `dotnet run` prints `30`.

```text
  d08 — command line arguments

  args              -t 5

  switch mappings   -t         ->  Weather:TimeoutSeconds
                    --timeout  ->  Weather:TimeoutSeconds

  Weather:TimeoutSeconds    =  5          (default 30)
```

The `args` line is the raw `string[]` the app received, so the room can see the spelling change while
the answer stays the same.

## Talk notes

Maps to [SPEC §4.3](../../SPEC.md).

- **Three spellings, one result.** `--Key value`, `/Key value`, `Key=value`. The value follows `=`
  with no space, or the key carries a `--` / `/` prefix when a space separates them. `--Key=value`
  also works; pick one style per command and stay in it.
- **`AddCommandLine` is registered last by the default host builders**, which is what "the command
  line wins" actually means — it is ordinary last-provider-wins precedence (`d02`), not a special
  case.
- **Switch mappings are just aliases**, and the mapped key must start with `-` or `--`. Both `-t` and
  `--timeout` can point at the same key.
- **Gotcha worth saying out loud:** an unmapped short switch is silently ignored, not an error.
  `dotnet run -x 5` prints `30` and tells you nothing. If a flag seems to do nothing on stage, check
  the mapping before you check the code.
- No `--` separator is needed before these arguments; `dotnet run` forwards what it does not
  recognize.
