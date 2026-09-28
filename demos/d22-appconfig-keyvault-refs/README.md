# d22 — App Configuration dereferences Key Vault

**One idea:** A Key Vault reference is just a key in the store whose value is a vault URI.
`ConfigureKeyVault` follows it, so the app reads an ordinary configuration key and gets a
secret — without naming Key Vault, holding a `SecretClient`, or knowing which keys are
secrets.

## Run

```bash
az login
cd demos/d22-appconfig-keyvault-refs

# PowerShell
$env:APPCONFIG_ENDPOINT = "https://<store>.azconfig.io"
dotnet run
```

The signed-in identity needs **App Configuration Data Reader** *and* **Key Vault Secrets
User**. Seed a plain key, a secret, and a reference to it:

```bash
S=<store>
V=<vault>

az appconfig kv set -n $S --key "Weather:Endpoint" --value "https://weather.internal" --yes
az keyvault secret set --vault-name $V --name "weather-api-key" --value "wk_test_not_a_real_key"

SECRET_ID=$(az keyvault secret show --vault-name $V --name "weather-api-key" --query id -o tsv)
az appconfig kv set-keyvault -n $S --key "Weather:ApiKey" --secret-identifier "$SECRET_ID" --yes
```

## What you should see

```text
  STORE    https://kg-config-demo.azconfig.io/
  SELECT   Weather:*

  Weather:ApiKey   -- Key Vault reference
        in the store   https://kg-vault.vault.azure.net/secrets/weather-api-key
        in the app     wk_********  (16 chars)

  Weather:Endpoint   -- plain value
        in the store   https://weather.internal
        in the app     https://weather.internal

  1 of 2 keys came from Key Vault. The app never said so.
```

The demo reads the store **twice** to produce that: once with a `SetSecretResolver` that
hands the pointer straight back instead of following it, and once with a real credential
that follows it. The two rows for `Weather:ApiKey` are the same key/value in the same
store — the only difference is whether anything dereferenced it.

## Talk notes

- **SPEC §4.6.** The architectural point: App Configuration holds the *shape* of your
  configuration, Key Vault holds the values that must not be readable. Neither one has to
  know about the other at the call site — `configuration["Weather:ApiKey"]` is the same
  line either way, so a value can be promoted from plain to secret without touching code.
- **Resolution order** is registered `SecretClient` → credential → `SetSecretResolver`.
  This demo uses the resolver deliberately, to show the pointer. Real apps use a credential.
- **Rotation is the trap.** A dereferenced secret is cached for the life of the process
  unless you set `SetSecretRefreshInterval(key, TimeSpan)`. Rotate the secret in the vault
  and, by default, every running instance keeps the old one — forever. That is the single
  most expensive default in this section.
- Note the two roles. A reference that resolves in dev and 403s in production is almost
  always the app's identity having App Configuration access but not Key Vault access.
