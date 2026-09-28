# d12 — `ValidateOnStart`: bad config means the app won't start

**One idea:** Fail at startup, not on the first request that happens to touch the bad value.

`appsettings.json` ships a deliberately invalid `Weather:TimeoutSeconds` of `900`, against a
`[Range(1, 300)]`. Nothing else changes between the runs below.

## Run

```bash
cd demos/d12-validate-on-start

dotnet run                                    # lazy validation: starts fine, dies later
dotnet run --Validation:OnStart true          # same config: refuses to start

dotnet run --Validation:OnStart true --Weather:TimeoutSeconds 30   # valid: starts and stays up
```

## What you should see

Run 1 — the process boots, reports itself healthy, and only fails when something reads the value
(exit code `0`, because the host started):

```text
  d12 — ValidateOnStart is OFF


  STARTED — health checks green, load balancer sending traffic.


  ... first request reaches the code that reads the value ...


  REQUEST FAILED

      DataAnnotation validation failed for 'WeatherOptions' members: 'TimeoutSeconds' with the error: 'The field TimeoutSeconds must be between 1 and 300.'.

  Same bad config. Found by a customer instead of by the deployment.
```

Run 2 — the same config never gets a chance to serve anything (exit code `1`):

```text
  d12 — ValidateOnStart is ON


  REFUSED TO START

      DataAnnotation validation failed for 'WeatherOptions' members: 'TimeoutSeconds' with the error: 'The field TimeoutSeconds must be between 1 and 300.'.

  The rolling deployment stops here. No instance ever takes traffic.
```

Run 3 prints `Timeout is 30s.` — the switch is not what fixes anything, the value is.

## Talk notes

Maps to **SPEC §5.2 and §5.5**. Never cut.

- Frame it as the rolling-deployment argument, not a correctness argument. Both runs have the same
  broken config; the only difference is whether the fleet finds out during the deploy or during
  someone's checkout. Without `ValidateOnStart()`, validation runs lazily on the first `.Value`
  access, so a bad deploy looks healthy right up until traffic reaches that code path.
- Point at the exit codes. Run 1 exits `0`, which is exactly what an orchestrator reads as "this
  instance is fine, roll the next one." Run 2 exits `1` and the rollout halts.
- The message names the offending key. That is the whole reason to put the rules on the options
  class rather than writing `if (timeout > 300) throw` somewhere in the request path.
- Reach for the shorthand in real code — `AddOptionsWithValidateOnStart<T>()` makes start-time
  validation the default posture. The `if (validateOnStart)` branch here exists only so one project
  can show both behaviors on stage.
- `ValidateDataAnnotations()` comes from `Microsoft.Extensions.Options.DataAnnotations`, referenced
  explicitly here because this is a console host; the web SDK pulls it in implicitly.
- Attributes stop at the top level: nested objects and collections need `[ValidateObjectMembers]`
  and `[ValidateEnumeratedItems]` (d15), and rules attributes can't express belong in an
  `IValidateOptions<T>` (d16).
