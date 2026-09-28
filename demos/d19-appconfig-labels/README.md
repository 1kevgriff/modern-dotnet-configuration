# d19 — Labels are the environment axis

**One idea:** One App Configuration store holds every environment. `Select` the unlabelled
defaults, then `Select` the same pattern again with the environment's label — the second
overlays the first, key by key, and anything the environment doesn't override is inherited.

## Run

```bash
az login
cd demos/d19-appconfig-labels

# PowerShell
$env:APPCONFIG_ENDPOINT = "https://<store>.azconfig.io"
$env:DOTNET_ENVIRONMENT = "Production"
dotnet run
```

The signed-in identity needs the **App Configuration Data Reader** role. Seed the store:

```bash
S=<store>
az appconfig kv set -n $S --key "Weather:Endpoint"       --value "https://weather.test"     --yes
az appconfig kv set -n $S --key "Weather:TimeoutSeconds" --value "30"                       --yes
az appconfig kv set -n $S --key "Weather:Retries"        --value "3"                        --yes
az appconfig kv set -n $S --key "Weather:Endpoint"       --value "https://weather.internal" --label Production --yes
az appconfig kv set -n $S --key "Weather:TimeoutSeconds" --value "5"                        --label Production --yes
```

Note that `Weather:Retries` deliberately has **no** `Production` value — that's the
inheritance half of the point.

## What you should see

```text
  STORE    https://kg-config-demo.azconfig.io/
  SELECT   Weather:*  with no label, then again with label "Production"

  Weather:Endpoint
        (no label)   https://weather.test
     -> Production    https://weather.internal

  Weather:Retries
     -> (no label)   3
        Production    (not set)

  Weather:TimeoutSeconds
        (no label)   30
     -> Production    5

  3 keys, one store, Production overlaid on the defaults
```

`->` marks the value the app actually gets. Re-run with
`$env:DOTNET_ENVIRONMENT = "Staging"` and the arrows move.

## Talk notes

- **SPEC §4.6.** Labels are not a naming convention you invent — they're a first-class
  dimension of every key/value in the store, and `Select` order is what turns them into
  precedence. Same mental model as `appsettings.json` → `appsettings.Production.json`,
  moved into a service where it can change without a redeploy.
- **Order matters and it is not alphabetical.** `LabelFilter.Null` first, environment
  second. Reverse them and Production quietly loses to the defaults.
- One store, not one store per environment: that's the whole argument for labels. The
  counter-argument — blast radius — is why you still use separate stores for prod
  *secrets* (d18, d22) even when non-secret config shares a store.
- **If the first run stalls for several seconds**, that's `DefaultAzureCredential` probing
  the instance metadata endpoint before it reaches your Azure CLI login. Set
  `AZURE_TOKEN_CREDENTIALS=dev` to skip straight to the developer credentials — worth
  doing before you go on stage.
