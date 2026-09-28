# d09 — User secrets

**One idea:** a secret lives outside the project tree, and it only loads in Development.

## Run

PowerShell:

```powershell
cd demos/d09-user-secrets
dotnet user-secrets set "Weather:ApiKey" "dev-key-12345"
dotnet user-secrets list

$env:DOTNET_ENVIRONMENT = "Development"
dotnet run                                   # key present

$env:DOTNET_ENVIRONMENT = "Production"
dotnet run                                   # same binary, key gone

./reset.ps1
```

bash:

```bash
cd demos/d09-user-secrets
dotnet user-secrets set "Weather:ApiKey" "dev-key-12345"
dotnet user-secrets list

DOTNET_ENVIRONMENT=Development dotnet run    # key present
DOTNET_ENVIRONMENT=Production  dotnet run    # same binary, key gone

dotnet user-secrets clear
```

## What you should see

Development — the provider is registered, the key resolves, and the path is on screen:

```text
  d09 — user secrets

  Environment            Development
  Secrets provider       registered

  Weather:ApiBaseUrl     https://api.example.com  <- appsettings.json
  Weather:ApiKey         dev-key-12345            <- user secrets

  secrets.json           C:\Users\you\AppData\Roaming\Microsoft\UserSecrets\d09a1b2c-...\
```

Production — nothing was rebuilt, nothing was deleted, and the key is simply gone:

```text
  Environment            Production
  Secrets provider       NOT REGISTERED

  Weather:ApiBaseUrl     https://api.example.com  <- appsettings.json
  Weather:ApiKey         (not set)

  secrets.json           (no provider, nothing to read)
```

`git status` stays clean the whole time. Nothing you typed touched the repo.

## Talk notes

Maps to [SPEC §4.4](../../SPEC.md).

- **Say it out loud: this is not encryption and not a vault.** `secrets.json` is plaintext in your
  profile directory. Its single job is keeping secrets out of the repository on a dev box. Production
  secrets come from Key Vault (`d18`) or App Configuration (`d22`).
- **The Development-only registration is in the default builders, not in your code.** Same binary,
  same file on disk, one environment variable different — and the provider isn't there. That is why
  a deployed app never accidentally reads a developer's secrets.
- **The path is printed from the registered provider, not composed by hand.** Don't write code
  against the storage location; the demo asks the provider where it is reading from.
- **`<UserSecretsId>` is committed in the `.csproj`** — it is an id, not a secret, and it is what
  `dotnet user-secrets init` writes for you. Without it the provider has nothing to look up.
- **A non-web host is `Production` by default.** There is no `launchSettings.json` making it
  Development the way an ASP.NET Core project does (`d07`), so both runs set `DOTNET_ENVIRONMENT`
  explicitly. Worth a sentence — it surprises people the first time a console worker ignores their
  secrets.
- Lifecycle for the room: `init`, `set`, `list`, `remove`, `clear`. Bulk load with
  `type .\secrets-input.json | dotnet user-secrets set` on Windows, `cat` on Linux/macOS.

`reset.ps1` clears the secret store and unsets `DOTNET_ENVIRONMENT`.
