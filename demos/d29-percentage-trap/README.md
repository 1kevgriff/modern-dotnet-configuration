# d29 — The percentage-filter trap

**One idea:** `Microsoft.Percentage` re-rolls the dice on every evaluation — it is not a stable
per-user assignment.

**Run:**

```powershell
cd demos/d29-percentage-trap
dotnet run
dotnet run -- --user marsha
```

**What you should see:**

One user, one process, ten checks. The percentage row flickers; the targeting row does not.

```text
  D29 - Microsoft.Percentage is per-call, not per-user

    user: alex      10 checks, one process, one second

    HalfOn        Microsoft.Percentage  50%
      ON   OFF  OFF  OFF  ON   OFF  OFF  ON   OFF  ON

    HalfOnStable  Microsoft.Targeting   50%
      ON   ON   ON   ON   ON   ON   ON   ON   ON   ON

    Percentage re-rolls the dice every evaluation. Targeting hashes the user,
    so one user gets one answer - and the crowd still splits about 50/50:

      alex ON    marsha OFF   sam OFF   jo OFF   kim ON    raj OFF   lee ON    ana OFF
```

`--user marsha` gives a different — but equally stable — targeting answer. The percentage row is
random every run, so the exact ON/OFF pattern will differ; that is the demo.

**Talk notes:**

- SPEC §6.4. The single most common feature-flag bug in the wild. Never cut.
- Both flags are 50%. Both are evaluated by the same call, `IsEnabledAsync(flag, user, ct)`. The
  **only** difference is the filter named in `appsettings.json` — so the bug is a config-shaped
  bug, not a code-shaped one.
- The real-world symptom is a user watching a feature flicker between page loads, or one request
  taking the new path in one service and the old path in the next.
- The fix is `Microsoft.Targeting`, which hashes user + flag name. Stable across calls, across
  processes, and across restarts — the crowd row proves it is still a real 50% rollout, not
  everyone pinned on.
- Variant allocation with a `seed` (d31) gives the same stability, and makes the assignment
  consistent across flags that share the seed.
- `ContextualTargetingFilter` is registered here instead of `TargetingFilter` so the user is passed
  at the call site. In a web app you register `.WithTargeting()` and an `ITargetingContextAccessor`
  pulls the user off `HttpContext` instead.
