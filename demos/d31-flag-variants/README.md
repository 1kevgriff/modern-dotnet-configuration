# d31 — Variants: flags that return values, not booleans

**One idea:** A variant flag returns a bound configuration object — this is where feature flags and
the options pattern meet.

**Run:**

```powershell
cd demos/d31-flag-variants
dotnet run
dotnet run -- --user sam
```

**What you should see:**

`marsha` is named in the flag's `user` allocation, so she gets `Big`:

```text
  D31 - a variant returns a configuration section, not a bool

    user: marsha        flag: CheckoutLayout

      variant                  Big

      CheckoutLayoutSettings   bound from variant.Configuration
        ButtonSize             large
        Columns                1
        ShowUpsell             True

      IsEnabledAsync()         True      status_override on the variant
```

`sam` is not named, so he falls through to the seeded percentile allocation and lands on `Small` —
whose `status_override: "Disabled"` also flips `IsEnabledAsync` to `False`:

```text
    user: sam        flag: CheckoutLayout

      variant                  Small
        ButtonSize             small
        Columns                3
        ShowUpsell             False

      IsEnabledAsync()         False     status_override on the variant
```

Run either one repeatedly: the same user gets the same variant every time.

**Talk notes:**

- SPEC §6.5. The payoff line: *a variant is a feature flag that returns a configuration section —
  which means everything from §5 still applies.* `variant.Configuration` is an
  `IConfigurationSection`; `Get<T>()` binds it into a `required`/`init` options class unchanged.
- Allocation is evaluated in order — `user` → `group` → `percentile` — then
  `default_when_enabled`, or `default_when_disabled` when the flag is off.
- `seed` is what makes the percentile assignment stable, and consistent across flags that share it.
  Same fix as d29, one level up.
- `status_override` (`None` / `Enabled` / `Disabled`) lets a variant flag also answer
  `IsEnabledAsync`, so you can adopt variants without rewriting existing `if` call sites. You
  cannot override a flag whose `enabled` is `false`.
- Cut in the 60-minute running order — mention it in one sentence instead.
