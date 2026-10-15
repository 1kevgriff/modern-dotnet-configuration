# Udemy Course Outline — Modern .NET Configuration

Built from the 90-minute talk in this repo: `OUTLINE.md` (the deck and speaker notes), `SPEC.md`
(the content spec), and the 33 demos in `/demos`. Target: **.NET 10 (LTS) / C# 14**.

---

## Part 1 — What the talk gives us, and what a course needs

### Carry over from the talk

| The talk has | How the course uses it |
| --- | --- |
| **One running example.** Every slide reads `Weather:TimeoutSeconds` and binds `WeatherOptions`. | Keep it as the course's single domain, so learners never pay to learn a new sample app. |
| **A mystery hook.** "The file says 120. The app says 10." It opens the talk and is solved 16 minutes in. | The promo video and Lecture 1.1 use it. It gets solved in Section 6, once learners can read the dump. |
| **A spine:** team defaults → my machine → deployment → shared → production. Each stage opens on the failure that forces the next one. | The spine becomes the section order. Each stage section opens on its failure. |
| **Four failure modes:** Order, Shape, Lifetime, Trust. | These become the course's diagnostic lens. Tag every bug, lab and quiz question with one of the four. |
| **Predict-then-reveal pairs:** arrays, launchSettings, the three interfaces, and the 50%-of-what flag. | Each pair becomes a **[Predict]** lecture: pause the video, commit to an answer, then run it. These pairs are ready-made quiz questions too. |
| **A through-line argument.** We traded a build-time decision for a runtime one, and flags make the same trade one level up. | This opens Section 2 and closes Section 13, so the course ends up making one argument instead of touring providers. |
| **33 one-idea demos,** verified on SDK 10.0.303. | Every demo gets a lecture (see the mapping in Appendix A). |
| **Running orders at 90, 60 and 45 minutes,** plus a "never cut" list. | These become a **★ Core track** of about 5 hours for learners in a hurry. |

### What has to change for Udemy

1. **The talk shows no live code; the course is live-coded.** The talk uses captured output only. On
   Udemy, learners code along, so every demo lecture runs the project on camera.
2. **Promote the half of the material the talk cut.** The talk cuts 16 demos marked "repo only" plus
   the appendix: named options, recursive validation, `IValidateOptions`, source generators, the custom
   provider, workers and desktop, null preservation, the command line, env-var keys, App Config labels,
   flag flips and Key Vault references, reload, and testing. In the course each one gets a full lecture.
3. **Azure needs real setup.** The talk pre-recorded d18–d22. Learners need provisioning, RBAC, cost
   and teardown lectures. They also need an Azure-free path: the Azure demos already print "not
   configured" and exit cleanly, so learners without a subscription can still follow along.
4. **One-line asides become lectures.** Key Vault rotation, reload consequences, desktop realities and
   the Q&A answers were time-boxed in the talk. Here they get room.
5. **Add practice.** The talk has no exercises. The course adds 6 quizzes, 8 labs and a capstone.

### Fix these before recording

These inconsistencies are in the source material, and a course script would copy them:

- **Slide 16's speaker notes contradict the slide.** The table (verified) puts host configuration at
  row 8: read first, loses the final read. The notes say "ROW 1 … makes host configuration the
  highest-priority source". Their row numbers are also off by one ("rows 5 and 6" should be 4 and 5;
  "row 2, command line" should be row 1). Go with the table.
- **"Atomic" survives in two places.** The deck says *do not say atomic* about sentinel keys, but
  `demos/README.md` (d20: "multi-key changes land atomically") and SPEC §4.6 still say it.
- **SPEC §6.8 still says flags give "instant rollback".** The deck corrects this on Slide 47. Script
  from the deck.
- **Three slides have no demo in `/demos`:** add-your-own-file (18a), `.local.json` (18c) and
  `Sources.Clear` (18b). They were verified in a scratch project ("loadproof"). Add a demo for them
  (proposed: d34).
- **`dotnet test` doesn't work** on the xUnit v3 test projects (d24, d33) without an MTP opt-in. The
  demo conventions forbid adding one. Learners will type `dotnet test` anyway, so either add the
  `global.json` opt-in to the course branch or teach `dotnet run --project tests` explicitly
  (Lecture 10.2).
- **Every `reset.ps1` needs PowerShell 7.** Either list `pwsh` as a prerequisite (it runs on
  macOS and Linux) or add `.sh` equivalents.

---

## Part 2 — Course landing page

**Title** (≤ 60 chars), pick one:
- `Modern .NET Configuration: From appsettings to Feature Flags` (60)
- `.NET 10 Configuration, Options and Feature Flags in Depth` (57)
- `Mastering .NET Configuration: Options, Secrets and Flags` (56)

**Subtitle** (≤ 120 chars):
`Providers, options, validation, secrets, Azure App Configuration and feature flags—patterns that hold up in production` (118)

**Level:** Intermediate · **Language:** English · **Category:** Development → Programming Languages → C#

**Length:** about 11 hours of video (650 min) across 18 sections and 111 lectures, plus 6 quizzes, 8 labs and a
capstone. The **★ Core track** is about 5 hours (see Part 3).

### What you'll learn

- Predict which value your app will read by reasoning about the provider chain, then prove it with GetDebugView
- Keep secrets out of git with user secrets locally and Azure Key Vault with managed identity in production
- Replace injected IConfiguration with typed, validated options that stop a bad deploy before it takes traffic
- Choose between IOptions, IOptionsSnapshot and IOptionsMonitor, and avoid the two classic lifetime bugs
- Supply configuration from environment variables, the command line and Docker/Kubernetes mounted secrets
- Centralize settings in Azure App Configuration with labels, refresh, sentinel keys and Key Vault references
- Ship feature flags with no cloud dependency, roll out safely with targeting, and pay down flag debt on purpose
- Test configuration-dependent code, and both sides of every feature flag, with the in-memory provider
- Pick a configuration strategy for web apps, workers, containers, microservices and desktop apps

### Requirements

- Comfortable with C# and the basics of ASP.NET Core: minimal APIs and dependency injection
- .NET 10 SDK and any editor (Visual Studio, VS Code with C# Dev Kit, or Rider)
- PowerShell 7 for the demo reset scripts (free on Windows, macOS and Linux)
- *Optional:* Docker Desktop for Section 7's container secrets
- *Optional:* an Azure subscription for Sections 7, 11 and 12. Every Azure lecture has an Azure-free path, and costs stay near zero if you run the teardown script.

### Who this course is for

- .NET developers who use `appsettings.json` every day but have never looked at the provider chain behind it
- Developers moving an app into containers, Kubernetes or Azure, where "the value didn't make it" starts happening
- Tech leads writing their team's configuration and secrets standards
- Teams that want feature flags without buying a flag platform

**Not for:** people new to C#, or anyone who needs deep coverage of .NET Framework's `ConfigurationManager`.

### Promo video (about 2 minutes)

1. **0:00** — Black screen, then `appsettings.Development.json` says `120` and `dotnet run` says `10`. Hold on it.
2. **0:15** — "By the end of Section 6 you'll read this dump like a stack trace." Show the unexplained `GetDebugView` output.
3. **0:35** — Rapid cuts to three reveals: arrays overlay (3 hosts, not 1), the three interfaces after a file edit (30/30/90), and the 50% flag flickering.
4. **1:20** — The journey graphic (my machine → deployment → shared → production) and what each stage covers.
5. **1:45** — Who it's for, and the repo with 33+ runnable projects.

**Free preview lectures:** 1.1, 1.4, 4.2, 12.5, 12.6

---

## Part 3 — Curriculum

**Legend:** ★ Core track · **[Predict]** pause and commit to an answer before the reveal ·
**[Preview]** free on Udemy · **[New]** new material the talk doesn't cover ·
Order / Shape / Lifetime / Trust = the failure mode the lecture teaches you to spot

The course keeps the talk's order on purpose. Deployment (Section 7) ends on "the value didn't make
it", and that failure is what motivates validated options (Section 8). The mystery pays off in
Section 6, after user secrets, because the dump contains a `UserSecretsConfigurationProvider` line.

### Section 1 — Welcome: The File Says 120. The App Says 10.

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 1.1 | ★ The file says 120, the app says 10 **[Preview]** | 3 | d01 | The cold open. Show the contradiction and the unexplained dump, and don't explain either. "We'll come back to this." |
| 1.2 | ★ How this course is built | 4 | — | The journey from team defaults to production, the Weather API example, the ★ Core track, and how labs and quizzes work. |
| 1.3 | ★ Set up your machine | 6 | all | .NET 10 SDK, editor, `pwsh`, clone the repo, and run `verify.ps1` to build all 35 projects. Azure is optional; this lecture says how to skip it. |
| 1.4 | ★ Four ways configuration breaks **[Preview]** | 4 | — | Order, Shape, Lifetime, Trust. Every bug in the course gets one of these labels. |

### Section 2 — Why Configuration Exists

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 2.1 | ★ What counts as configuration | 4 | — | Which database, which URL, how long to wait. The useful boundary: who owns the value and how it changes. User preferences and tenant data usually don't count. |
| 2.2 | ★ Five reasons to externalize a value | 6 | — | No hard-coding, per-environment values, change on the fly, **trust** (blast radius) and **ownership** (the dev/ops seam). The last three cost something. |
| 2.3 | Three eras: `#if DEBUG`, `web.config`, the generic host | 6 | — | Score each era against the five reasons. Transforms ran at *build* time, which is why "which build is this?" was a real question for a decade. |
| 2.4 | ★ The trade: a build-time decision for a runtime one | 3 | — | A constant can't be missing, malformed, stale, or from a forgotten source. A runtime value can be all four. |
| 2.5 | A feature flag is configuration you read at a branch | 3 | — | A timeout decides how long, a connection string decides where, and `NewCheckout` decides *which code runs*. |
| 2.6 | ★ The journey, stage by stage | 4 | — | Stages are cumulative, not alternatives. Each is introduced by its signature failure. Know where to stop. |
| Q1 | Quiz: why configuration | — | — | 6 questions |

### Section 3 — How Configuration Loads

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 3.1 | ★ What `CreateBuilder` loads for you | 5 | d01 | The default sources, including .NET 10's `{ApplicationName}.settings.json`. A bare `ConfigurationBuilder` adds none of them. |
| 3.2 | ★ Your first setting: `appsettings.json` and `GetValue<int>` | 6 | d02 | Nested JSON becomes the key `Weather:TimeoutSeconds`. Why the demos use `--no-launch-profile` (a setup for Section 5). |
| 3.3 | Everything is a string | 5 | d02 | Indexer vs `GetValue<T>`. `"thirty"` fails to convert, but `"-1"` converts fine. That gap is why Section 8 exists. |
| 3.4 | ★ The environment name picks the second file | 7 | d02 | Where the name comes from: launchSettings, `ASPNETCORE_`, `DOTNET_`, or the `Production` fallback. `DOTNET_ENVIRONMENT` wins when both are set. Names are arbitrary, so `appsettings.QA-East.json` works. |
| 3.5 | Environment files override keys, not files | 4 | d02 | Base plus Development gives a merged result, and `ApiKey` survives from the base file. |
| 3.6 | ★ Same key, four sources, four answers | 7 | d02 | 30 → 120 → 10 → 5. Last provider wins: `Add` appends, and reads walk the list in reverse. **Order** |
| 3.7 | Add your own file, and watch it beat the command line | 6 | d34 **[New]** | `weather.json` = 77 beats env 10 and CLI 5. `optional: false` and `reloadOnChange`. Nothing scans the folder for you. |
| 3.8 | The `.local.json` convention nobody loads for you | 5 | d34 **[New]** | Register it last, mark it optional, git-ignore it. Otherwise it silently does nothing. **Order** |
| 3.9 | Take control with `Sources.Clear()` | 5 | d34 **[New]** | Rebuild the chain yourself. Host decisions like the environment name and content root are already made by then. |
| L1 | Lab: predict the value | — | d02, d34 | 8 scenarios. Write down the answer, then run it. About 30 min. |

### Section 4 — The Mental Model: One Flat Dictionary

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 4.1 | ★ One flat, case-insensitive dictionary of strings | 7 | d01 | `:` is the delimiter, and four reads return the same value. The binder projects slices of the dictionary onto types. |
| 4.2 | ★ Arrays overlay, they don't replace **[Predict] [Preview]** | 6 | d03 | "Base has three hosts, Development has one. How many does the app see?" Three. Prefer objects keyed by name. Anti-pattern #10. **Shape** |
| 4.3 | Duplicate keys: an error in one file, the whole point across files | 3 | — | A duplicate inside one file throws `FormatException`; a duplicate across providers is how layering works. |
| 4.4 | .NET 10 breaking change: null is preserved | 8 | d06 | The same code on net9.0 and net10.0. The effect differs by type: `string?`, `int?`, non-nullable, and arrays. "Retries was 3 on .NET 9 and is null on .NET 10." |
| Q2 | Quiz: the model | — | — | 8 questions, several with code snippets |

### Section 5 — Stage 1: Your Development Machine

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 5.1 | ★ User secrets in five commands | 7 | d09 | `init`, `set`, `list`, `remove`, `clear`. Keys use the colon path form. Bulk-load new teammates by piping in JSON. |
| 5.2 | ★ Not encrypted. Not a vault. | 5 | d09 | The repo holds a GUID; your laptop holds plaintext. Development-only, and where it sits in the chain. **Trust** |
| 5.3 | .NET 10 file-based apps get user secrets too | 6 | d10 | `dotnet run config.cs` with no `.csproj`. The `--file` flag, and the path-hash ID: move the file and it sees different secrets. |
| 5.4 | ★ Why does `dotnet run` change my value? **[Predict]** | 4 | d07 | Shell env var = 10, launch profile = 45. Which one wins? |
| 5.5 | ★ The launchSettings trap, explained | 6 | d07 | The tooling sets process env vars, and `dotnet publish` doesn't ship the file. The failure cuts both ways: works locally, missing in prod. **Order** |
| 5.6 | Personal overrides vs secrets: which tool when | 3 | d34 | `.local.json` is for ordinary values; user secrets are for credentials. |
| L2 | Lab: onboard a new developer | — | d09 | Clone, set secrets, run, and `git status` stays clean. About 20 min. |

### Section 6 — Which Provider Won? Diagnostics

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 6.1 | ★ The default provider order, highest first | 7 | d01 | The 8-row table. The app-named files apply to web apps too, except `CreateSlimBuilder`. Host configuration is read first and loses. |
| 6.2 | The host reads configuration in two passes | 6 | d01 | `--environment Staging`, and the command-line provider registered at both ends of the chain. "The input that picks your inputs." |
| 6.3 | ★ `GetDebugView`: the provider behind every key | 7 | d01 | The `processValue` overload and `ConfigurationDebugViewContext`. Enumerate `IConfigurationRoot.Providers`. |
| 6.4 | ★ Dump configuration safely | 5 | d01 | A denylist misses tokens and connection strings. Gate on `IsDevelopment()` and prefer specific keys. Anti-pattern #11. **Trust** |
| 6.5 | ★ The mystery solved: why 10 won | 4 | d01 | The cold-open dump again, now marked. An environment variable sits later in the chain. You now know everything needed to read every line. |
| L3 | Lab: three broken apps | — | new | Find the winning provider and the failure mode for each. About 30 min. |

### Section 7 — Stage 2: Deployment. Let the Platform Supply the Values.

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 7.1 | ★ One artifact, many environments | 3 | — | Rule of thumb #1: a binary per environment brings back `Web.Release.config`. This stage's signature failure is "the value didn't make it". |
| 7.2 | ★ Environment variables: `__`, arrays and prefixes | 8 | d04 | The prefix is stripped, then `__` becomes `:`. `DOTNET_` and `ASPNETCORE_` are reserved. A single underscore is the most common bug. Values are read once at startup. **Shape** |
| 7.3 | The command line: three syntaxes and switch mappings | 6 | d08 | `--Key value`, `/Key value`, `Key=value`, and `-t` / `--timeout` aliases. |
| 7.4 | ★ One key, four spellings | 4 | — | JSON nesting, `:`, `__` and `--`. "There is one key. The colon is the real delimiter. The rest is spelling." **Shape** |
| 7.5 | Connection strings and .NET 10's eleven env prefixes | 6 | d05 | From 4 prefixes to 11. `POSTGRESQLCONNSTR_Default` → `GetConnectionString("Default")`, and `_ProviderName`. |
| 7.6 | Key-per-file: Docker and Kubernetes secrets | 9 | d23 | `compose.yaml`: the file name is the key and the contents are the value. The path must be absolute. The 3-argument overload doesn't reload, and K8s symlink swaps can defeat the watcher even when it does. |
| 7.7 | Azure setup: resource group, Key Vault, RBAC **[New]** | 7 | script | `az` / Bicep provisioning, the Key Vault Secrets User role, cost, and the teardown script. |
| 7.8 | ★ Read secrets from Azure Key Vault | 8 | d18 | `AddAzureKeyVault`. `Weather--ApiKey` becomes `Weather:ApiKey`. Register it last, and guard it with `!IsDevelopment()`. **Shape** |
| 7.9 | `DefaultAzureCredential` vs `ManagedIdentityCredential` | 6 | d18 | The dev convenience chain vs a narrow credential that fails fast in prod. `AZURE_CLIENT_ID` for user-assigned identities. |
| 7.10 | Key Vault in production: rotation and isolation | 6 | d18 | `ReloadInterval` is null (never reload) by default. Expired secrets still load unless a custom `KeyVaultSecretManager` filters them. One vault per app per environment. Anti-pattern #9. **Trust** |
| L4 | Lab: one image, two environments | — | d04, d23 | Run the same container as "staging" and "production" using env vars and mounted secrets. About 40 min. |

### Section 8 — The Options Pattern: Give Configuration a Type

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 8.1 | ★ Stop injecting `IConfiguration` | 7 | d11 | `WeatherClientBefore` vs `WeatherClient`. Four problems: stringly typed, unvalidated, untestable, re-parsed. The `!` is anti-pattern #3. |
| 8.2 | ★ Design an options class | 6 | d11 | A `SectionName` const, `required init` properties and sensible defaults. The type declares the component's whole configuration surface. |
| 8.3 | ★ Register and bind | 6 | d11 | `AddOptions<T>().Bind()` vs `Configure<T>`. Make `AddOptionsWithValidateOnStart` your default. |
| 8.4 | ★ Fail fast with `ValidateOnStart` | 8 | d12 | Capture the startup `OptionsValidationException`. Lazy validation fails on the first request that reaches the code; this fails before rolling deploys send traffic. |
| 8.5 | Validation doesn't recurse | 7 | d15 | `[ValidateObjectMembers]` and `[ValidateEnumeratedItems]`: `[Required]` doesn't recurse into nested objects or collections. |
| 8.6 | Rules attributes can't express: `IValidateOptions<T>` | 7 | d16 | Cross-field and budget rules. Register with `TryAddEnumerable`, not `Add`. |
| 8.7 | Named options: one shape, several instances | 6 | d14 | `Configure<EndpointOptions>("primary", …)` and `monitor.Get("primary")`. `Options.DefaultName`. |
| 8.8 | Source generators and Native AOT | 9 | d17 | `EnableConfigurationBindingGenerator`, `[OptionsValidator]`, and `PublishAot` with IL2026/IL3050 warnings that disappear once generated. .NET 10's AOT-safe `ValidationContext`. |
| Q3 | Quiz: options | — | — | 8 questions |
| L5 | Lab: validate a second options type | — | new | `EmailOptions` with nested SMTP settings, a list of senders and a cross-field rule. About 40 min. |

### Section 9 — Production: Lifetimes and Reload

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 9.1 | ★ Production changes the stakes | 3 | — | Ownership, trust and uptime. "Nothing gets rebuilt to change a value." |
| 9.2 | ★ `IOptions`, `IOptionsSnapshot`, `IOptionsMonitor` | 7 | d13 | Lifetime, whether it re-reads, and when to use each. |
| 9.3 | ★ One file edit, three answers **[Predict]** | 9 | d13 | The centerpiece. Edit `appsettings.json` from 30 to 90, then compare same request vs next request: 30/30/30, then 30/30/90 → 30/90/90. "Scoped" means fresh per scope. **Lifetime** |
| 9.4 | ★ The two classic lifetime bugs | 6 | d13 | `IOptionsSnapshot` injected into a singleton, and `CurrentValue` cached in a constructor field. Anti-patterns #4 and #5. Scope validation catches the first one. **Lifetime** |
| 9.5 | React to change with `OnChange` | 6 | d13, d27 | Dispose the registration, debounce because one save fires twice, and log with templates. |
| 9.6 | ★ What actually reloads | 8 | d27 | The reload matrix. A file edit shows up in 0.3 s; an env var change never does. |
| 9.7 | "Config reloaded" ≠ "app reconfigured" | 5 | — | Kestrel endpoints, the DI graph and `HttpClient` handlers read once. K8s ConfigMaps. Rule #4: prefer a restart. |
| Q4 | Quiz: lifetimes and reload | — | — | 8 questions, mostly "which of these moves?" |

### Section 10 — Testing Configuration-Dependent Code

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 10.1 | ★ A test is just one more provider | 7 | d24 | `WebApplicationFactory` plus `AddInMemoryCollection`, added last. Why not `appsettings.Testing.json`, and why not mock `IConfiguration`. |
| 10.2 | Run xUnit v3 tests on .NET 10 | 4 | d24 | Microsoft.Testing.Platform, `dotnet run --project tests` vs the `global.json` opt-in, and `DefaultItemExcludes` for nested test folders. |
| 10.3 | Test that bad configuration stops startup **[New]** | 6 | new | Assert on the `OptionsValidationException`, so your fail-fast guard is itself tested. |
| 10.4 | Unit-test options and validators in isolation **[New]** | 5 | d16 | `Options.Create`, and calling an `IValidateOptions<T>` directly. |
| L6 | Lab: cover the Weather API's configuration | — | d24 | About 30 min. |

### Section 11 — Stage 3: Shared Configuration with Azure App Configuration

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 11.1 | ★ Several apps, several instances, shared settings | 4 | — | When you need it, and when you don't: one app that redeploys in five minutes doesn't need it (rule #3). |
| 11.2 | Create a store and grant access **[New]** | 7 | script | `az appconfig create`, the App Configuration Data Reader role, the Free tier, and teardown. |
| 11.3 | Connect and load a slice | 6 | d19 | `AddAzureAppConfiguration` and `Select("Weather:*")`. A failed connection at startup is a crash unless you handle it. |
| 11.4 | ★ Labels: one key, one store, a label per environment | 7 | d19 | `LabelFilter.Null` then the environment label, in that order. Labels aren't matched to your environment automatically. **Order** |
| 11.5 | ★ Refresh isn't automatic | 7 | d20 | `ConfigureRefresh`, the middleware and where it goes in the pipeline. Refresh is triggered by requests, so an idle instance never refreshes. |
| 11.6 | ★ The sentinel key pattern | 8 | d20 | Change the sentinel last. One instance gets all the values together, but the fleet doesn't flip at once. Not transactional; don't call it atomic. **Lifetime** |
| 11.7 | Refresh from background services and console apps | 5 | d20 | `IConfigurationRefresherProvider` and `TryRefreshAsync()`. |
| 11.8 | Key Vault references: the store holds the pointer | 8 | d22 | `ConfigureKeyVault`, the two roles it needs, and `SetSecretRefreshInterval`, because a rotated secret is otherwise cached forever. **Trust** |
| 11.9 | Failure, history and rollback | 5 | — | A failed refresh keeps the last known-good values. Revisions. `Map` for rewriting keys. |
| L7 | Lab: move the Weather API's settings into a store | — | d19, d20 | Labels, a sentinel, and one Key Vault reference. About 45 min (Azure). |

### Section 12 — Feature Flags: Configuration with an `if` Statement

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 12.1 | ★ A feature flag is just configuration | 5 | — | `Microsoft.FeatureManagement` is built on `IConfiguration`, so flags inherit the chain, precedence and reload behavior. Flags at every stage. |
| 12.2 | ★ Flags with no cloud at all | 8 | d28 | The `feature_management` schema and `AddFeatureManagement()`. An env var overrides a flag by last-wins. With conditions, `enabled: true` means "eligible", not "on". |
| 12.3 | Four ways to consume a flag | 8 | d28 | `IVariantFeatureManager`, `[FeatureGate]` (404 by default), the tag helper, and `UseForFeature`. Use `AddScopedFeatureManagement` when filters need scoped services. |
| 12.4 | Time windows: holiday pricing that turns itself on | 6 | d30 | `Microsoft.TimeWindow` and recurrence. A `FixedTimeProvider` makes the clock demoable. |
| 12.5 | ★ 50% of users or 50% of calls? **[Predict] [Preview]** | 4 | d29 | Same user, same session, two checks. Same answer? |
| 12.6 | ★ The percentage trap, explained **[Preview]** | 5 | d29 | `Microsoft.Percentage` is per call, not per user. The nav bar says new checkout and the checkout page says old. |
| 12.7 | Targeting: stable per-user rollouts | 8 | d29, d30 | `ITargetingContextAccessor`, users, groups, per-group percentages and exclusions. |
| 12.8 | Custom filters: a flag per tenant | 7 | d30 | `IFeatureFilter`, `AddFeatureFilter<TenantFilter>()`. |
| 12.9 | Variants: flags that return configuration | 8 | d31 | `GetVariantAsync` then `Bind`. Allocation order, `seed`, `status_override`. |
| 12.10 | Flip a flag in Azure without a redeploy | 7 | d21 | `UseFeatureFlags`. The flip takes effect per instance, on that instance's next refresh. |

### Section 13 — Flag Discipline: Debt, Testing and the Honest Trade-off

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 13.1 | ★ Using a flag is a branch in your code | 4 | d28 | Turning a flag off doesn't delete it. Deleting a flag is a pull request. |
| 13.2 | ★ Every flag needs an owner and an expiry | 6 | d32 | Release, experiment, ops and permission flags. Name flags for removal: `Checkout_V2_Rollout_2026Q1`. |
| 13.3 | Build a flag inventory endpoint | 8 | d32 | `GetFeatureNamesAsync`. Authorize it. A percentage or targeting flag's value is only *this* evaluation. |
| 13.4 | ★ Test both sides of every live flag | 7 | d33 | `FlagFactory` forces the flag on and off with one test per branch, so no branch ships untested behind a 2 a.m. switch. |
| 13.5 | ★ Same trade, one level up | 5 | — | Flags turn a deployment problem into a runtime problem. Not "instant rollback": the commit no longer tells you *which path* ran. Use revisions. |
| Q5 | Quiz: feature flags | — | — | 8 questions |

### Section 14 — Beyond the Web App: Workers, Console Tools and Desktop

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 14.1 | The same model in a worker service | 7 | d26 | `Host.CreateApplicationBuilder`, `DOTNET_ENVIRONMENT`, and the full provider list printed at startup. |
| 14.2 | Console and CLI tools | 5 | d08, d26 | The command line plus env vars. A short-lived process doesn't need refresh. |
| 14.3 | Desktop apps get installed, not deployed | 8 | d26 | `SetBasePath(AppContext.BaseDirectory)`: the same binary behaves differently from a different working directory. Desktop has no shared stage. |
| 14.4 | User preferences, `app.config`, and secrets in client apps | 6 | d26 | No write API, so use a per-user AppData file. `ConfigurationManager` is unrelated to `IConfiguration`. No cloud secrets in a client: call a backend instead. **Trust** |

### Section 15 — Extend the System: Build a Custom Provider

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 15.1 | A provider is `Load()` and a dictionary | 7 | d25 | `DotEnvConfigurationSource` and `DotEnvConfigurationProvider`. That's the entire contract. |
| 15.2 | Make it read like a built-in | 4 | d25 | Wrap it in an extension method: `AddDotEnvFile(...)`. |
| 15.3 | Reload with `OnReload` and change tokens | 7 | d25 | What makes `IOptionsMonitor` wake up. Callback to 9.3. |
| 15.4 | When `Load()` throws, and what's worth building | 5 | d25 | A throwing `Load()` is fail-fast by design. Good targets: a database or your own config service. Bad targets: anything the built-ins already cover. |
| L8 | Lab: a SQLite-backed provider with polling reload | — | new | About 45 min |

### Section 16 — Choose a Strategy

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 16.1 | ★ Four questions | 5 | — | Ownership, sensitivity, scope and cadence. You don't need stage three; most apps never do. |
| 16.2 | ★ Sort every setting into three buckets | 6 | — | Non-secret per-environment, secret, and per-instance. "If a value is in the wrong bucket, no amount of provider tuning fixes it." |
| 16.3 | Strategy by application shape | 8 | — | App Service, containers/AKS, microservices, workers, CLI tools and desktop. |
| 16.4 | ★ Six rules of thumb | 6 | — | One artifact; commit defaults, never secrets; cloud config only with a reason; prefer restarts; validate at startup; secrets need a rotation story. |
| 16.5 | ★ Eleven anti-patterns: name the failure mode | 8 | — | Walk each one and tag it Order / Shape / Lifetime / Trust. By now learners call it before you do. |
| Q6 | Quiz: spot the anti-pattern | — | — | 11 code snippets |

### Section 17 — Capstone: Modernize a Legacy App's Configuration

A Udemy **assignment** with a starter branch. Each milestone has a solution walkthrough video.

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 17.1 | Meet the legacy app **[New]** | 5 | new | `#if DEBUG` constants, injected `IConfiguration`, a secret in `appsettings.Production.json`, and a 50% `Percentage` flag. |
| 17.2 | Milestone 1: local dev | 6 | new | Secrets out of git, environment files, and a redacted debug view. |
| 17.3 | Milestone 2: typed, validated options | 6 | new | Replace every `config["..."]!` and add `ValidateOnStart`. |
| 17.4 | Milestone 3: deployment | 6 | new | Container, env vars, and key-per-file. |
| 17.5 | Milestone 4: flags with owners | 6 | new | Targeting instead of percentage, a named expiry, both-sides tests, and an inventory endpoint. |
| 17.6 | Milestone 5 (optional, Azure) | 6 | new | App Configuration labels, a sentinel, and Key Vault references. |

### Section 18 — Wrap-up

| # | Lecture | Min | Demo | What it covers |
| --- | --- | --: | --- | --- |
| 18.1 | ★ Three things to take home | 3 | — | `GetDebugView` tells you where a value came from. Validate at startup. Name every flag for the day you delete it. |
| 18.2 | What changed in .NET 10, in one place | 5 | d05, d06, d10, d17 | Null preservation, 11 connection-string prefixes, AOT-safe `ValidationContext`, and file-based user secrets. |
| 18.3 | Looking ahead: .NET 11 preview | 3 | — | Label it as preview and re-verify on the day you record. |
| 18.4 | FAQ: Aspire, secrets in App Configuration, `IOptionsMonitor` in a console app | 5 | — | Lifted from the talk's Q&A prep. |
| 18.5 | Bonus lecture | 2 | — | Where to go next. |

### Totals

| Section | Lectures | Min | ★ Core min |
| --- | --: | --: | --: |
| 1 Welcome | 4 | 17 | 17 |
| 2 Why configuration exists | 6 | 26 | 17 |
| 3 How configuration loads | 9 | 50 | 25 |
| 4 One flat dictionary | 4 | 24 | 13 |
| 5 Stage 1: your machine | 6 | 31 | 22 |
| 6 Diagnostics | 5 | 29 | 23 |
| 7 Stage 2: deployment | 10 | 63 | 23 |
| 8 Options | 8 | 56 | 27 |
| 9 Lifetimes and reload | 7 | 44 | 33 |
| 10 Testing | 4 | 22 | 7 |
| 11 Stage 3: App Configuration | 9 | 57 | 26 |
| 12 Feature flags | 10 | 66 | 22 |
| 13 Flag discipline | 5 | 30 | 22 |
| 14 Beyond the web | 4 | 26 | 0 |
| 15 Custom provider | 4 | 23 | 0 |
| 16 Choose a strategy | 5 | 33 | 25 |
| 17 Capstone | 6 | 35 | 0 |
| 18 Wrap-up | 5 | 18 | 3 |
| **Total** | **111** | **650 (≈ 10.8 h)** | **305 (≈ 5.1 h)** |

---

## Part 4 — Practice assets

| Asset | Count | Built from |
| --- | --- | --- |
| Quizzes | 6 (about 49 questions) | The [Predict] pairs, the reload matrix, the provider order table, and the 11 anti-patterns |
| Labs | 8 | Starter and solution branches per lab (`course/lab-NN-start`, `course/lab-NN-solution`) |
| Capstone | 1 assignment, 5 milestones | A new legacy starter app, with a solution walkthrough per milestone |
| Cheat sheets (PDF) | 7 | The deck's reference tables: provider order, one key/four spellings, the three interfaces, the reload matrix, three buckets, the app-shape matrix, and the anti-patterns |

## Part 5 — Build list before recording

1. Fix the six items under **Fix these before recording** (Part 1).
2. Add **d34**: your own file, `.local.json`, and `Sources.Clear` (Lectures 3.7–3.9, 5.6).
3. Write the **Azure provisioning and teardown** scripts (Lectures 7.7 and 11.2). Each needs the role
   assignments and a cost note.
4. Build the **lab starter and solution branches**, the **capstone legacy app**, and the Lab 3
   "three broken apps".
5. Add **tests for Lectures 10.3 and 10.4**. d24 only covers the override case today.
6. Pin the SDK in `global.json` for the course branch, and record the package versions
   (`dotnet list package`) for the resources sheet. The flag schema and Azure SDKs will drift after
   publishing.
7. Reuse the Slidev deck's layouts for the concept lectures. Record live-code lectures against the demos.

---

## Appendix A — Demo → lecture map

Every demo is used at least once. d34 is proposed.

| Demo | Lectures | | Demo | Lectures |
| --- | --- | --- | --- | --- |
| d01 provider-dump | 1.1, 3.1, 4.1, 6.1–6.5 | | d18 key-vault | 7.8–7.10 |
| d02 precedence | 3.2–3.6, L1 | | d19 appconfig-labels | 11.3, 11.4, L7 |
| d03 array-merge | 4.2 | | d20 appconfig-sentinel | 11.5–11.7, L7 |
| d04 env-var-keys | 7.2, L4 | | d21 appconfig-flag-flip | 12.10 |
| d05 connstr-prefixes | 7.5, 18.2 | | d22 appconfig-keyvault-refs | 11.8 |
| d06 null-preserved | 4.4, 18.2 | | d23 key-per-file | 7.6, L4 |
| d07 launchsettings-trap | 5.4, 5.5 | | d24 in-memory-tests | 10.1, 10.2, L6 |
| d08 command-line | 7.3, 14.2 | | d25 custom-provider | 15.1–15.4 |
| d09 user-secrets | 5.1, 5.2, L2 | | d26 non-web-hosts | 14.1–14.4 |
| d10 file-based-app | 5.3, 18.2 | | d27 reload-on-change | 9.5, 9.6 |
| d11 options-binding | 8.1–8.3 | | d28 flags-no-cloud | 12.2, 12.3, 13.1 |
| d12 validate-on-start | 8.4 | | d29 percentage-trap | 12.5–12.7 |
| d13 options-lifetimes | 9.2–9.5 | | d30 flag-filters | 12.4, 12.7, 12.8 |
| d14 named-options | 8.7 | | d31 flag-variants | 12.9 |
| d15 recursive-validation | 8.5 | | d32 flag-inventory | 13.2, 13.3 |
| d16 validate-options-class | 8.6, 10.4 | | d33 flag-testing | 13.4 |
| d17 source-generators | 8.8, 18.2 | | d34 own-file *(new)* | 3.7–3.9, 5.6 |
