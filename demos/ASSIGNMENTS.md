# Worker → folder assignments

Authoritative. If your prompt was truncated, read this file instead of guessing or asking.
Work **only** the folders on your row. Do not create, edit, or delete anything outside them.

| Worker process | Folders |
| --- | --- |
| `demo-w1-model-b` | `d01-provider-dump` `d02-precedence` `d03-array-merge` `d06-null-preserved` `d07-launchsettings-trap` |
| `demo-w2-sources` | `d04-env-var-keys` `d05-connstr-prefixes` `d08-command-line` `d09-user-secrets` `d10-file-based-app` |
| `demo-w3-options-core` | `d11-options-binding` `d12-validate-on-start` `d13-options-lifetimes` `d14-named-options` |
| `demo-w4-options-adv` | `d15-recursive-validation` `d16-validate-options-class` `d17-source-generators` `d24-in-memory-tests` `d27-reload-on-change` |
| `demo-w5-cloud` | `d18-key-vault` `d19-appconfig-labels` `d20-appconfig-sentinel` `d21-appconfig-flag-flip` `d22-appconfig-keyvault-refs` `d23-key-per-file` |
| `demo-w6-flags` | `d28-flags-no-cloud` `d29-percentage-trap` `d30-flag-filters` `d31-flag-variants` `d32-flag-inventory` `d33-flag-testing` |
| `demo-w7-reach` | `d25-custom-provider` `d26-non-web-hosts` |

Your Solo process name is in `$SOLO_PROCESS_ID`'s process entry — or just run `whoami()`.

## Per-demo intent

The full intent for each demo is in its **Solo todo** (project 24). Map:

| Folder | Todo | Spec |
| --- | --- | --- |
| d01-provider-dump | 71 | §2.1 |
| d02-precedence | 72 | §3.1 |
| d03-array-merge | 73 | §4.1 |
| d04-env-var-keys | 74 | §4.2 |
| d05-connstr-prefixes | 75 | §4.2 |
| d06-null-preserved | 76 | §9.1 |
| d07-launchsettings-trap | 77 | §4.2 |
| d08-command-line | 78 | §4.3 |
| d09-user-secrets | 79 | §4.4 |
| d10-file-based-app | 80 | §4.4 |
| d11-options-binding | 81 | §5.1 |
| d12-validate-on-start | 82 | §5.2 |
| d13-options-lifetimes | 83 | §5.3 |
| d14-named-options | 84 | §5.4 |
| d15-recursive-validation | 85 | §5.5 |
| d16-validate-options-class | 86 | §5.5 |
| d17-source-generators | 87 | §5.6 |
| d18-key-vault | 88 | §4.5 |
| d19-appconfig-labels | 89 | §4.6 |
| d20-appconfig-sentinel | 90 | §4.6 |
| d21-appconfig-flag-flip | 91 | §4.6 |
| d22-appconfig-keyvault-refs | 92 | §4.6 |
| d23-key-per-file | 93 | §4.7 |
| d24-in-memory-tests | 94 | §4.8 |
| d25-custom-provider | 95 | §4.10 |
| d26-non-web-hosts | 96 | §4.11 |
| d27-reload-on-change | 97 | §8 |
| d28-flags-no-cloud | 99 | §6.1–6.2 |
| d29-percentage-trap | 100 | §6.4 |
| d30-flag-filters | 101 | §6.4 |
| d31-flag-variants | 102 | §6.5 |
| d32-flag-inventory | 103 | §6.6 |
| d33-flag-testing | 104 | §6.7 |

## Documented exceptions to the house rules

- **d06** is the only multi-target demo: `<TargetFrameworks>net9.0;net10.0</TargetFrameworks>`.
- **d17** sets `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>` with a comment saying why —
  its "before" state exists to *show* IL2026/IL3050.
- **d10** has no `.csproj` at all — it is a .NET 10 file-based app (`dotnet run config.cs`).
- **d11** is the only place the null-forgiving `!` operator is allowed, in the deliberately-bad
  "before" sample, commented `// DON'T:`.
- **d18–d22** require Azure. Definition of done is *compiles cleanly and degrades gracefully*
  (print "not configured — see README" and exit 0 when the endpoint env var is missing). Do not
  create Azure resources.
