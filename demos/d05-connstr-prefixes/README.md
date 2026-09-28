# d05 — Connection-string prefixes

**One idea:** .NET 10 rewrites eleven `*CONNSTR_` environment variable prefixes into
`ConnectionStrings:`, up from four.

## Run

PowerShell:

```powershell
cd demos/d05-connstr-prefixes
$env:POSTGRESQLCONNSTR_Default = "Host=localhost;Database=demo"
$env:SQLCONNSTR_Legacy         = "Server=sql;Database=Old"
dotnet run
./reset.ps1
```

bash:

```bash
cd demos/d05-connstr-prefixes
export POSTGRESQLCONNSTR_Default="Host=localhost;Database=demo"
export SQLCONNSTR_Legacy="Server=sql;Database=Old"
dotnet run
unset POSTGRESQLCONNSTR_Default SQLCONNSTR_Legacy
```

## What you should see

```text
  d05 — *CONNSTR_ environment variables become connection strings

  POSTGRESQLCONNSTR_Default     (new in .NET 10)
      GetConnectionString("Default")              Host=localhost;Database=demo
      ConnectionStrings:Default_ProviderName      Npgsql

  SQLCONNSTR_Legacy             (recognized since .NET Core 1.0)
      GetConnectionString("Legacy")               Server=sql;Database=Old
      ConnectionStrings:Legacy_ProviderName       System.Data.SqlClient

  The eleven prefixes (* = added in .NET 10):

     CUSTOMCONNSTR_              MYSQLCONNSTR_               SQLCONNSTR_
     SQLAZURECONNSTR_           *POSTGRESQLCONNSTR_         *DOCDBCONNSTR_
    *REDISCACHECONNSTR_         *SERVICEBUSCONNSTR_         *EVENTHUBCONNSTR_
    *NOTIFICATIONHUBCONNSTR_    *APIHUBCONNSTR_
```

Neither variable was set as `ConnectionStrings__Default`, and nothing in the code mentions Postgres.
The provider did both rewrites.

**Optional — show the .NET 9 behavior.** Change `<TargetFramework>` to `net9.0` and the package
version to `9.0.0`, then run again. `GetConnectionString("Legacy")` still resolves;
`GetConnectionString("Default")` returns null. Put the file back afterwards.

## Talk notes

Maps to [SPEC §4.2](../../SPEC.md), and it is a .NET 10 headline item nobody has heard about.

- **This is App Service compatibility, and it is why the prefixes look the way they do.** Setting a
  connection string in the App Service portal writes `{TYPE}CONNSTR_{NAME}` into the environment; the
  configuration provider has always translated the handful of types that existed then.
- **`_ProviderName` is generated, not set.** `ConnectionStrings:Default_ProviderName = Npgsql` came
  from the prefix, not from any variable — the same way `SQLCONNSTR_` has always produced
  `System.Data.SqlClient`. Four of the seven new prefixes produce no provider name at all.
- **The behavior ships in the `Microsoft.Extensions.Configuration.EnvironmentVariables` package**, so
  what matters is the package (or shared framework) version, not just the TFM you compiled against.
- Cheap, offline, no Azure account needed — this one records well.
