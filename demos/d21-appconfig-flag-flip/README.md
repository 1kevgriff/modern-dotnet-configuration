# d21 — Flip the flag in the portal, no redeploy

**One idea:** The same flag from d28, moved into App Configuration, so someone can change
it while the process is running — and the running process changes its mind.

## Run

```bash
az login
cd demos/d21-appconfig-flag-flip

# PowerShell
$env:APPCONFIG_ENDPOINT = "https://<store>.azconfig.io"
dotnet run           # watches for 180s
dotnet run -- 60     # or a shorter window
```

Ctrl+C stops it early. The signed-in identity needs the **App Configuration Data Reader**
role. Create one feature flag named `Beta`, starting disabled:

```bash
az appconfig feature set -n <store> --feature Beta --yes
```

## What you should see

Start the app, then toggle `Beta` in the portal's **Feature manager** blade:

```text
  STORE   https://kg-config-demo.azconfig.io/
  FLAG    Beta   refreshing every 5s for 180s   (Ctrl+C to stop)

  14:03:22   Beta = OFF

  Now toggle the flag in the portal. The process keeps running.

  14:03:41   Beta = ON    <-- flipped in the portal

  14:04:02   Beta = OFF   <-- flipped in the portal

  2 flip(s), 0 deployments.
```

The last line is the whole demo. Toggle it back and forth a couple of times — the rollback
being as fast as the rollout is the part that matters operationally.

## Talk notes

- **SPEC §6.** Run **d28 first**, always. The room should see that a feature flag is just
  configuration and costs nothing before Azure enters the picture; this demo is only about
  where the value lives, not about what a flag *is*.
- **`UseFeatureFlags` self-registers for refresh.** Ordinary keys need an explicit
  `ConfigureRefresh` (d20); flags do not. They still need something to *ask* — hence
  `TryRefreshAsync` in the loop. In ASP.NET Core `app.UseAzureAppConfiguration()` middleware
  does the asking, which is why an idle web app never picks up a flip.
- Flag flips are deploys with no deploy pipeline attached. Say the uncomfortable part out
  loud: this is a production change with no PR, no review, and no test run. App
  Configuration keeps revisions — use them, and d32 is about the debt you accrue if you
  don't.
- A flag the store doesn't have evaluates to `false` rather than throwing. Convenient, and
  a trap: a typo'd flag name is a silently-off feature.

## Deviation from the demo note

The Solo todo describes a web page you refresh after toggling the flag. This is a console
app printing a line each time the value changes, per the stage rule preferring a console
over a web app. The flip is more legible this way — a timestamped ON/OFF line reads from
the back of a room better than a page whose content changed — but the browser-refresh
framing, and with it the visible fact that a web app only refreshes on request activity,
is lost. Same trade as d20.
