# d20 — The sentinel key: multi-key changes land atomically

**One idea:** Register one sentinel key instead of `RegisterAll()`, and bump it *after*
every other edit lands — the app sees nothing until the sentinel moves, then sees the whole
change at once, instead of catching it half-applied.

## Run

```bash
az login
cd demos/d20-appconfig-sentinel

# PowerShell
$env:APPCONFIG_ENDPOINT = "https://<store>.azconfig.io"
dotnet run           # watches for 180s
dotnet run -- 60     # or a shorter window
```

Ctrl+C stops it early. The signed-in identity needs the **App Configuration Data Reader**
role. Seed the store:

```bash
S=<store>
az appconfig kv set -n $S --key "Weather:Endpoint"       --value "https://weather.test" --yes
az appconfig kv set -n $S --key "Weather:Retries"        --value "3"                    --yes
az appconfig kv set -n $S --key "Weather:TimeoutSeconds" --value "30"                   --yes
az appconfig kv set -n $S --key "Weather:Sentinel"       --value "1"                    --yes
```

## What you should see

Start the app, then in the portal edit **all three** `Weather` values — and watch nothing
happen. Then bump `Weather:Sentinel` to `2`:

```text
  STORE      https://kg-config-demo.azconfig.io/
  WATCHING   Weather:*   through the sentinel Weather:Sentinel
  POLLING    every 5s for 180s   (Ctrl+C to stop)

  #1   14:03:22   sentinel = 1
         Weather:Endpoint         https://weather.test
         Weather:Retries          3
         Weather:TimeoutSeconds   30

  Now edit Endpoint, Retries and TimeoutSeconds in the portal.
  Nothing will happen here. Then bump the sentinel.

  #2   14:03:58   sentinel = 2
         Weather:Endpoint         https://weather.internal
         Weather:Retries          9
         Weather:TimeoutSeconds   5

  2 revision(s) observed.
```

The demo prints **only when the values change**, so the gap between the three portal edits
and revision `#2` is the point. Three keys, one revision.

## Talk notes

- **SPEC §4.6.** The failure this prevents is specific and it is not hypothetical: an
  operator edits an endpoint and the credential that goes with it, and for a few seconds a
  refreshed instance holds the new endpoint and the old credential. `RegisterAll()` makes
  that window real. A sentinel key makes it zero.
- **Refresh is not automatic and it is not push.** You call `ConfigureRefresh`, and then
  something has to *ask*. In ASP.NET Core the middleware asks on an incoming request — so
  an idle app never refreshes. This is a console app, so it asks for itself via
  `TryRefreshAsync`, which is exactly what a worker or background service must do (§4.11).
- `SetRefreshInterval` is a floor on how often the provider will call Azure, not a poll
  schedule — asking more often than the interval is free and does nothing.
- **A failed refresh is graceful**: the last known-good configuration stays, and it retries.
  A failed *initial* load is not graceful — that's a startup crash, and it should be.
- Contrast with d18: Key Vault has no refresh at all unless you set `ReloadInterval`.

## Deviation from the demo note

The Solo todo describes this as a web app — edit keys, refresh the browser, see nothing;
bump the sentinel, refresh, see everything — driven by `app.UseAzureAppConfiguration()`
middleware. This is a **console app** instead, for two reasons: the stage rules prefer a
console over a web app unless the demo needs HTTP, and alt-tabbing to a browser costs more
attention than the point is worth. The trade is real and worth knowing: the middleware's
*activity-driven* refresh — an idle web app never refreshing — can only be **said** here,
not shown, because this app pumps `TryRefreshAsync` on a timer itself. If that behaviour
needs to be demonstrated rather than asserted, this demo has to become a web app.
