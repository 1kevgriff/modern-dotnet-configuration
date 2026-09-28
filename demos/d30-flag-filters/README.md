# d30 — Filters: time window, recurrence, and a custom one

**One idea:** A flag can decide for itself, from data you supply.

**Run:**

```powershell
cd demos/d30-flag-filters
dotnet run
dotnet run -- --clock 2026-12-10T02:00:00Z --tenant contoso
dotnet run -- --clock 2026-01-05T09:00:00Z --tenant contoso
```

**What you should see:**

The same four flags, the same `appsettings.json`, three different sets of answers — driven only by
the clock and the tenant.

```text
  D30 - a flag decides for itself, from data you supply

    clock: 2026-12-10 09:00 UTC      tenant: fabrikam

      HolidayPricing            ON    Microsoft.TimeWindow   1 Dec - 26 Dec 2026
      NightlyBatch              OFF   Microsoft.TimeWindow   daily 01:00 - 03:00
      TenantBeta                OFF   Tenant (custom)        tenant = contoso
      HolidayPricingForContoso  OFF   All of both            window AND tenant
```

Move the clock into the nightly window and switch tenant, and all four turn on:

```text
    clock: 2026-12-10 02:00 UTC      tenant: contoso

      HolidayPricing            ON    ...
      NightlyBatch              ON    ...
      TenantBeta                ON    ...
      HolidayPricingForContoso  ON    ...
```

Move the clock to January and only the tenant flag survives — `requirement_type: All` means the
combined flag needs both, so it goes off with the window.

**Talk notes:**

- SPEC §6.4. Filters are registered automatically by `AddFeatureManagement()` — except targeting,
  which needs `.WithTargeting()`, and anything you write yourself.
- `TimeWindowFilter.SystemClock` is a `TimeProvider` you can set. That is the whole reason this
  demo is deterministic instead of "trust me, it's December". The same handle is what makes a
  time-windowed flag testable.
- `Recurrence` turns one window into a schedule — `Pattern` (Daily/Weekly) plus `Range`
  (NoEnd/EndDate/Numbered). `NightlyBatch` is a two-hour window that repeats every day forever.
- `requirement_type` defaults to `Any`. `All` is how you intersect filters.
- The custom filter is `TenantFilter.cs` — one method, `EvaluateAsync`, plus `[FilterAlias]` to
  name it in JSON, registered with `.AddFeatureFilter<TenantFilter>()`. `context.Parameters` is an
  `IConfiguration`, so the filter's own settings bind exactly like §5 options.
- Targeting (users, groups, per-group percentages, exclusions) is the payoff of d29 — that is
  where the users/groups shape gets its own screen rather than a fourth row here.
