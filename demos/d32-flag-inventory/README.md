# d32 — Flag inventory: the flag-debt demo

**One idea:** You can't manage flag debt you can't see.

**Run:**

```powershell
cd demos/d32-flag-inventory
dotnet run
dotnet run -- --today 2027-01-15
```

**What you should see:**

Every flag the app knows about, with the two overdue ones in red:

```text
  D32 - you can't manage flag debt you can't see

    today: 2026-08-30

    FLAG                              STATE  CATEGORY     OWNER      EXPIRES
    Checkout_V2_Rollout_2026Q1        ON     release      payments   2026-03-31   OVERDUE 152d
    Search_Ranking_Experiment_2026Q2  OFF    experiment   discovery  2026-06-30   OVERDUE 61d
    Banner_Holiday_Rollout_2026Q4     OFF    release      marketing  2026-12-31   123d left
    Admin_BulkExport_Permission       OFF    permission   platform   never
    Payments_Provider_KillSwitch      ON     ops          payments   never

    2 of 5 flags are past their expiry date.
    Deleting a flag is a code change, not a config change.
```

`--today` drives the clock, so the overdue count is whatever you need it to be on stage. Without
it the demo uses the real date, which is the honest version — and the count grows on its own.

**Talk notes:**

- SPEC §6.6. The closing demo and the closing argument: the thing nobody else in the room will say,
  and a five-minute build the audience will actually steal.
- `GetFeatureNamesAsync()` is the whole trick — the app can enumerate every flag it knows about,
  because the flags are configuration and configuration is enumerable.
- **Name flags for their removal.** `NewCheckout` never gets deleted;
  `Checkout_V2_Rollout_2026Q1` files its own expiry. `FlagNaming.cs` reads category and expiry
  straight out of the name — no registry to keep in sync, because the name *is* the registry.
- The four categories, and how long each should live: *release* (days to weeks, delete after
  rollout) · *experiment* (weeks, delete after the decision) · *ops* / kill switches (permanent,
  and that's fine) · *permission* (permanent, and arguably authorization rather than a flag).
- Owner comes from a `flag_owners` section next to the flags — the annotation that a name can't
  carry.
- Ten live flags is up to 1,024 nominal combinations. You test maybe three. That is d33.

**Making it an endpoint:** this is a console app so it runs in one command with nothing to curl,
but the inventory loop is unchanged in a web app — add `Microsoft.FeatureManagement.AspNetCore`
and wrap it:

```csharp
app.MapGet("/flags", async (IVariantFeatureManager features, CancellationToken ct) =>
{
    List<FlagRecord> inventory = [];

    await foreach (string name in features.GetFeatureNamesAsync(ct))
    {
        inventory.Add(new FlagRecord(
            name,
            await features.IsEnabledAsync(name, ct),
            FlagNaming.CategoryOf(name),
            owners[name] ?? "unowned",
            FlagNaming.ExpiryOf(name)));
    }

    return TypedResults.Ok(inventory);
});
```
