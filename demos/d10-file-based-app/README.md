# d10 — File-based app

**One idea:** a single `.cs` file with no project can still read configuration and user secrets.

> This demo is the deliberate exception to the repo's project-file template. There is **no `.csproj`
> in this folder** — that absence is the demo. Everything else here follows the house rules; the
> compiler settings that normally live in the `.csproj` are `#:property` lines at the top of
> `config.cs`.

## Run

```bash
cd demos/d10-file-based-app
dotnet run config.cs                                              # ApiKey: (not set)

dotnet user-secrets set "ApiKey" "from-user-secrets" --file config.cs
dotnet user-secrets list --file config.cs
dotnet run config.cs                                              # ApiKey: from-user-secrets

./reset.ps1
```

The same four commands work unchanged in PowerShell.

## What you should see

```text
  d10 — one .cs file, no .csproj, real configuration

  UserSecretsId   config-cb81bcddb597e6afc2341e289c67cb87c7fcb84a21b4e6eecaffe56535e9db48
  ApiKey          from-user-secrets
```

Then `ls` the folder on stage: `config.cs`, `README.md`, `reset.ps1`. No project, no `bin`, no `obj`
— the build output goes to a temp directory keyed by the same hash.

## Talk notes

Maps to [SPEC §4.4](../../SPEC.md). Pure .NET 10 novelty, and a good palate cleanser between the
heavier demos.

- **The `UserSecretsId` is derived, not declared.** `config-cb81bcd…` is a hash of the file's full
  path, which is what lets `dotnet user-secrets --file config.cs` and the running app agree on a
  store with no project file to write an id into. Move the file and it becomes a different app with
  a different store — worth saying, because it is the one sharp edge.
- **`#:package` is how a file-based app takes a NuGet dependency**, pinned to an explicit version
  exactly like a `PackageReference`. `#:property` sets MSBuild properties — here, warnings-as-errors.
- **`AddUserSecrets(assembly, optional: true)` is the ordinary API.** Nothing in the file knows it is
  a file-based app; the SDK stamps the assembly with the id and the normal provider finds it.
- Where this earns its keep: a one-file utility, a scratch reproduction, or a script that needs a
  token without becoming a project.
- **Verification note:** `dotnet build config.cs` is clean with warnings-as-errors on.
  `dotnet format` cannot run here — it needs an MSBuild workspace and there isn't one — so this demo
  is exempt from the format check by construction.
