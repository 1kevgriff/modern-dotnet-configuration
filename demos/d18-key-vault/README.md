# d18 — Key Vault: `--` becomes `:`

**One idea:** Key Vault secret names can't contain `:`, so `Weather--ApiKey` in the vault
arrives as `Weather:ApiKey` in configuration — and `DefaultAzureCredential` means there is
no connection string anywhere to get it.

## Run

```bash
az login
cd demos/d18-key-vault

# PowerShell
$env:KEYVAULT_NAME = "<your-vault-name>"
dotnet run
```

The vault needs three secrets and the signed-in identity needs the **Key Vault Secrets
User** role:

```bash
az keyvault secret set --vault-name <v> --name "Weather--ApiKey"          --value "wk_live_7f3a91c4"
az keyvault secret set --vault-name <v> --name "Weather--Endpoint"        --value "https://weather.internal"
az keyvault secret set --vault-name <v> --name "Weather--TimeoutSeconds"  --value "30"
```

## What you should see

```text
  VAULT        https://kg-config-demo.vault.azure.net/
  CREDENTIAL   DefaultAzureCredential

  Weather--ApiKey
      ->  Weather:ApiKey  =  wk_********  (16 chars)

  Weather--Endpoint
      ->  Weather:Endpoint  =  htt********  (24 chars)

  Weather--TimeoutSeconds
      ->  Weather:TimeoutSeconds  =  30********  (2 chars)

  3 secrets loaded — none of them in the repo, none on the projector
```

With `KEYVAULT_NAME` unset, the demo prints the setup block and exits 1. If the vault name
is wrong or `az login` has expired, it prints one line and a checklist — never a stack
trace. That matters more on a projector than it does in CI.

## Talk notes

- **SPEC §4.5.** Two things to land: the `--` → `:` mapping, and that no credential ships
  in the app. `DefaultAzureCredential` walks a chain (Azure CLI → Visual Studio →
  environment → managed identity), which is why the same code runs on a laptop and on App
  Service. In production prefer `ManagedIdentityCredential` explicitly — narrower, and it
  fails fast instead of silently walking down the chain to something that works.
- **Register it last.** Key Vault is meant to override the JSON files it replaces, and
  the last provider wins (d02).
- **No reload by default.** Secrets are cached for the process lifetime. `ReloadInterval`
  opts into polling; without it, a rotated secret needs a restart.
- **Expired secrets still load** — only *disabled* ones are skipped. If expiry should mean
  "stop using this", override `KeyVaultSecretManager.Load` yourself.
- Compare with d23: same keys, same binding, a completely different secret store. The
  composition root is the only thing that changed.
