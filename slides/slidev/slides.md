---
theme: default
title: Modern .NET Configuration
info: |
  ## Modern .NET Configuration
  Kevin Griffin - Microsoft MVP - .NET 10 / C# 14

  Generated from OUTLINE.md. Regenerate with `npm run generate`.
author: Kevin Griffin
keywords: dotnet,configuration,options,feature-flags
class: text-left
highlighter: shiki
lineNumbers: false
drawings:
  persist: false
transition: none
mdc: true
fonts:
  sans: Manrope
  mono: JetBrains Mono
  provider: none
layout: "code"
codeSize: "17.0"
---

<!--
  GENERATED FILE - DO NOT HAND-EDIT.

  Source of truth is ../../OUTLINE.md plus build/slide-map.mjs.
  Regenerate with:  npm run generate

  Editing this file directly means your change is lost on the next generate.
-->

<!-- OUTLINE.md # Slide 1 -->

```text
appsettings.Development.json
────────────────────────────
  "Weather": {
    "TimeoutSeconds": 120
  }


$ dotnet run
────────────────────────────
  Weather timeout is 10 seconds.
```

<!--
[COLD OPEN 1 of 2] THE CONTRADICTION
60-min: keep, ~90 seconds for both cold open slides

FIRST THING THE ROOM SEES. No title slide yet. No introduction. Say nothing
for a beat, then read it out flatly:

"The file says one hundred and twenty. The app says ten."

Then STOP TALKING. Count to ten in your head. Let it be uncomfortable.

Do NOT explain it. Do NOT say "who can tell me why". Do not take a guess from
the floor. The only line you say is:

"We'll come back to this."

Then advance.

WHY OPEN HERE: "why externalize configuration" and "how do we load it" are
both expository. Two explanatory beats back to back is a flat first ten
minutes. A small mystery buys attention for the parts that are genuinely just
exposition - and it gets paid off properly at GetDebugView.

This slide and the next are lifted from d01 in the repo. Everything on screen
today runs.
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 2 -->

```text
Weather:
  ApiBaseUrl=https://localhost:7104
    (JsonConfigurationProvider for 'appsettings.Development.json')
  Retries=3
    (JsonConfigurationProvider for 'appsettings.json')
  TimeoutSeconds=10
    (EnvironmentVariablesConfigurationProvider Prefix: '')
  ApiKey=***
    (UserSecretsConfigurationProvider)
```

<!--
[COLD OPEN 2 of 2] THE DUMP, UNEXPLAINED
60-min: keep

The same dump that comes back later - but with NO gold marker and NO
explanation. They cannot read it yet and that is the point.

Give it five seconds. Say:

"This is the app telling us exactly what happened. By the time we come back to
this slide, you'll be able to read it."

Then advance to the title and introduce yourself.

NOTE THE DEVICE: this image appears twice in the deck. Here it is unmarked and
unexplained. At the payoff slide it is the same dump with a gold marker on the
line that resolves the mystery. Do not mark it here and do not walk it.

If someone shouts out the answer - and occasionally someone does - say "yes,
and in twenty minutes everyone will know why" and move on. Do not let it turn
into a conversation; the whole device depends on the room sitting with it.
-->

---
layout: "cover"
variant: "title"
image: "/kevin-griffin.png"
kicker: "CONFIGURATION · OPTIONS · FEATURE FLAGS"
---

<!-- OUTLINE.md # Slide 3 -->

<!-- from: OUTLINE.md slide 3 byline -->

# Modern .NET Configuration

- Kevin Griffin
- Microsoft MVP
- .NET 10 / C# 14

<!--
=== BEFORE THIS SLIDE IS ON SCREEN ===
[0-04 min] COLD OPEN - S1.0
Snippet: the d01 output, captured
60-min: keep, 3 min

Black screen. No title slide yet. No setup, no explanation.

DO:
- The file says 120. The app says 10. Both on screen, neither explained.
- The GetDebugView() dump on screen shows an unexpected provider won.
- Let the room sit with it. Fifteen full seconds. Count them.
- Say only: "We'll come back to this." Move on.

WHY THIS OPENING:
"Why externalize configuration" and "how do we load it" are both expository.
Two explanatory beats back to back is a flat first ten minutes. A small mystery
buys attention for the parts that are genuinely just exposition. S2 pays it off.

DO NOT take questions here. It only works while it stays unexplained.

=== THEN BRING UP THIS SLIDE ===

[title card]
60-min: keep

Now introduce yourself - briefly. The mystery from the cold open is already
running, so don't stall here.

- Kevin Griffin, software consultant, Microsoft MVP.
- Repo QR goes here AND on the thanks slide. People photograph the first one.

Frame the next 90 minutes in one line:
"Every app has configuration. Almost nobody revisits it. Today we find out what
it actually does, where it cuts you, and what holds up in production."3
-->

---
layout: "cover"
variant: "bio"
image: "/kevin-griffin.png"
---

<!-- OUTLINE.md # Slide 4 -->

<!-- from: OUTLINE.md slide 4 + README.md §Speaker -->

# Kevin Griffin

- Software consultant — .NET and Azure
- Microsoft MVP
- Builds and runs Shows On Sale
- consultwithgriff.com

<!--
[about me]
60-min: keep, but 30 seconds MAX

Keep this SHORT. Credibility, then move - the room came for configuration, not
for a resume, and the cold open is still unresolved.

The one line that actually matters:
"Everything in this talk is something I got wrong in production first."

That's the license to be opinionated for the next 85 minutes. It also sets up
S6.6 (flag debt) and S4.6 (the idle-app refresh surprise) as scars rather than
trivia.

Do NOT list every technology you've touched. If they want the resume it's on
the site, and the site is on the last slide.4
-->

---
layout: "roadmap"
panelTitle: "What we'll cover"
---

<!-- OUTLINE.md # Slide 5 -->

<!-- from: OUTLINE.md slide 5 -->

1. The provider chain and precedence
2. Local dev → deployment → shared
3. Binding, options, and validation
4. Feature flags and flag debt
5. Choosing a strategy

<!--
[roadmap]
60-min: keep - it's the map, and at 60 minutes the map does more work

Five beats. Read them fast; it's a map, not a stop. Twenty seconds.

  01  How the provider chain really resolves        (S2, S3)
  02  Local dev -> deployment -> shared             (S1.4 spine, S4)
  03  Binding, options, and failing fast            (S5)
  04  Feature flags and flag debt                   (S6)
  05  Choosing a strategy, and where to stop        (S7)

Point at 02 and say "that's the spine - everything hangs off it."
Point at 04 and say "this is about a quarter of the talk, and it's the half
people don't expect."

Then get off this slide. The cold open is STILL unexplained and that tension is
doing work for you until S2.1.5
-->

---
layout: "default"
---

<!-- OUTLINE.md # Slide 6 -->

# Why externalize configuration at all?

<!-- from: OUTLINE.md slide 6 — card titles inline, emphasis as described -->

<Cards :cols="5">

<Card n="01" title="No hard-coding"></Card>
<Card n="02" title="Varies by environment"></Card>
<Card n="03" title="Change it on the fly"></Card>
<Card n="04" title="Trust" accent></Card>
<Card n="05" title="Ownership" accent></Card>

</Cards>

<Caption>

04 and 05 are why this talk exists in 2026.

</Caption>

<!--
[04-10 min] WHY - S1.1
60-min: compress to 4 min - trust and ownership get one line each

Five answers. First three obvious, last two are why this talk exists in 2026.

1. NO HARD-CODING - a value baked into an assembly changes only by editing code,
   rebuilding, retesting, redeploying. Value and logic have completely different
   rates of change. They shouldn't share a lifecycle.

2. VALUES THAT VARY BY ENVIRONMENT - same binary, local DB / staging API / prod
   endpoint. If the CODE differs per environment, you aren't testing what you ship.

3. CHANGING A VALUE ON THE FLY - timeout, log level, toggle. No deploy window,
   no dropped traffic.

4. TRUST - a credential compiled into an assembly ships everywhere that assembly
   ships: every laptop, build artifact, container layer, decompiler. This is not
   about rate of change, it's about BLAST RADIUS. Arguably reason #1 today.

5. OWNERSHIP - the person who needs to change the value often can't compile the
   code. Configuration is the seam between dev and ops. An SRE raising a timeout
   at 3am shouldn't need your build pipeline, your PR review, or you.

THE PIVOT: reasons 1-2 were solved by web.config twenty years ago.
Reasons 3, 4, 5 are why we're here - and they're the three that COST something.
A value outside the binary can be missing, malformed, stale, or arriving from a
source you forgot about. The rest of the talk is about paying that cost on purpose.
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 7 -->

```csharp
// 2002
#if DEBUG
    const string Db = "localhost";
#else
    const string Db = "sql-prod-01";
#endif

// 2005
ConfigurationManager.AppSettings["Db"];

// 2018
config.GetConnectionString("Default");
```

<!--
[10-13 min] COLD OPEN PART TWO - S1.2
60-min: fold into the Why block - drop the storytelling, keep the through-line

Three eras. Score each against the five reasons.

1. CONSTANTS AND #if DEBUG
   Hardcoded values, commented-out blocks, a pre-commit ritual.
   Simple and dangerous - failure mode was shipping the wrong uncommented line.
   Fails #1 outright. Fails #4 catastrophically.

2. web.config / app.config + ConfigurationManager.AppSettings
   XML, static, one string-keyed bag. No reload, no layering, framework-only.
   Transforms (Web.Release.config) ran at BUILD time.
   Wins #1 and #2 - but you built one artifact per environment, and #3 was impossible.

3. THE .NET GENERIC HOST (2018, .NET Core 2.1) + IConfiguration
   Provider chain, layering, binding, DI, reload - and the SAME model for a web
   app, a worker, a console tool, and a WinForms app.
   One artifact, many environments. All five reasons, finally.

THE THROUGH-LINE - say this out loud, it comes back twice:
"We traded a build-time decision for a runtime one."
Everything hard about modern configuration comes from that trade.
S6.8 shows feature flags making the same trade one level up.

ON SCREEN: three snippets, one per era, dated. No prose. Walk them in order
and score each against the five reasons from the previous slide.
-->

---
layout: "statement"
---

<!-- OUTLINE.md # Slide 8 -->

We traded a build-time decision for a runtime one.

<!--
[10-13 min] THE TRADE - S1.2 through-line
60-min: keep - it costs 15 seconds and it earns the S6.8 callback

Headline only. Say it, let it sit, move on.

Everything hard about modern configuration comes from this trade. You are no
longer deciding at compile time what the app will do - you are deciding at
startup, or at runtime, or in a portal while the app is serving traffic.

That's the whole reason the rest of the talk has failure modes at all. A
constant can't be missing, malformed, stale, or arriving from a provider you
forgot about. A runtime value can be all four.

COMES BACK TWICE:
- S4.6, when App Configuration moves the decision later still.
- S6.8, when feature flags make the same trade one level up: deploys to flags.
  Point at this slide from there. "Same trade, one level up."
-->

---
layout: "statement"
---

<!-- OUTLINE.md # Slide 9 -->

**If an operational value can differ by environment, deployment, or time, configure it.**

**If a code path must be switchable without a deploy, flag it.**

<!--
[13-16 min] THESIS - S1.3
60-min: keep - this is the organizing claim

State it plainly:

"If a value could ever differ between two environments, two deployments, or two
moments in time, it belongs in configuration. If a behavior could ever need to
change without a deploy, it belongs behind a feature flag."

The bar is COULD EVER, not DOES TODAY. Moving a value into configuration later is
a code change under time pressure. Putting it there now costs one line. Same
argument for flags: adding one before you ship is cheap; adding one during an
incident is not.

HANDLE TWO OBJECTIONS OUT LOUD - both are fair:

- "Then everything is configurable and nothing is knowable."
  True if you stop at externalizing. That's why S5 binding + ValidateOnStart
  exist. A configurable value that's validated at startup is MORE knowable than
  a constant buried in code - it's declared, typed, and checked in one place.

- "That's a lot of flags."
  Yes, and every one is debt. S6.6 is the discipline that makes this position
  survivable rather than reckless.
-->

---
layout: "section"
kicker: "THE SPINE"
---

<!-- OUTLINE.md # Slide 10 -->

# local dev → deployment → shared

<!--
[13-16 min] THE SPINE - S1.4
60-min: keep, 2 min - at 60 this slide does the organizing work nothing else can

Three stages, increasing in both capability and operational cost.
"Shared" not "scale" on purpose - stage three is about configuration that
outlives or spans a single deployment unit. Scale is one reason you get there,
not the definition.

LOCAL DEV - a developer clones the repo and it runs; secrets never touch git.
  appsettings.json, appsettings.Development.json, user secrets, launchSettings.json

DEPLOYMENT - one artifact, many environments; the platform supplies the values.
  env vars, command line, key-per-file mounts, Key Vault

SHARED - many apps and instances; values that change without a deploy.
  Azure App Configuration - labels, refresh, sentinel keys, feature flags

EACH STAGE IS INTRODUCED BY THE FAILURE THAT FORCES YOU TO THE NEXT ONE.
This is what gives the spine motion instead of making it a list:

  Local dev   -> "works on my machine"       (launchSettings overriding your env
                                             var; secrets on one laptop)
  Deployment  -> "the value didn't make it"  (a provider you forgot was winning;
                                             wrong key shape; secret cached)
  Shared      -> "it changed, but not everywhere at once" (stale caches,
                                             half-applied edits, idle instance)

TWO THINGS TO SAY WITH THIS UP:
- The stages are CUMULATIVE, not alternatives. A stage-three app still has
  appsettings.json for defaults and still takes per-instance values from env vars.
  You add layers; you don't replace them.
- KNOW WHERE TO STOP. Stage three earns its complexity when more than one app
  shares values, or something genuinely must change without a deploy. S7 decides.
-->

---
layout: "default"
---

<!-- OUTLINE.md # Slide 11 -->

# Almost every config bug is one of four things

<!-- from: OUTLINE.md slide 11 ## Notes — the four numbered definitions, verbatim -->

<Cards :cols="2">

<Card n="01" title="ORDER">A provider you forgot about is winning.</Card>
<Card n="02" title="SHAPE">The key you set doesn't produce the key the binder looks for.</Card>
<Card n="03" title="LIFETIME">You cached a value that was supposed to change — or vice versa.</Card>
<Card n="04" title="TRUST">A secret is sitting in a file that ships with the app.</Card>

</Cards>

<!--
[motif slide] FOUR FAILURE MODES - S1.5
60-min: keep - it's 30 seconds and it pays off all talk

Not a section. A label to hang on things as they come up. Whatever stage you're
in, almost every configuration bug is one of four things:

1. ORDER    - a provider you forgot about is winning.
2. SHAPE    - the key you set doesn't produce the key the binder looks for.
3. LIFETIME - you cached a value that was supposed to change, or didn't cache
              one that wasn't.
4. TRUST    - a secret is sitting in a file that ships with the app.

NAME THEM OUT LOUD AS THEY COME UP:
  order on Same key. Four winners.    shape on Environment variables
  lifetime on the three interfaces    trust on User secrets

By the anti-patterns slide the audience should be calling them before you do.
That's the win condition for this slide.
-->

---
layout: "panels"
---

<!-- OUTLINE.md # Slide 12 -->

# It's one flat dictionary

<PanelRow :cols="2" size="11.2" arrow>

<!-- from: demos/d01-provider-dump/appsettings.json -->

<Panel caption="WHAT YOU WROTE">

```json
{
  "Weather": {
    "ApiBaseUrl": "https://api.example.com",
    "TimeoutSeconds": 30,
    "ApiKey": "placeholder-set-a-real-one-with-user-secrets"
  },
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Demo;..."
  }
}
```

</Panel>

<!-- from: the flat projection of the same file — ":" delimiter, values are strings -->

<Panel caption="WHAT ACTUALLY EXISTS" dark>

```text
Weather:ApiBaseUrl         "https://api.example.com"
Weather:TimeoutSeconds     "30"
Weather:ApiKey             "placeholder-set-a-real-..."
ConnectionStrings:Default  "Server=localhost;Database=..."
```

</Panel>

</PanelRow>

<!--
[16-23 min] THE MODEL - S2
60-min: keep, 5 min

Configuration is not "the appsettings.json file." It is an ordered chain of
key/value providers collapsed into ONE flat, case-insensitive dictionary of
strings, plus a binder that projects slices of it onto typed objects.

- Flat IDictionary<string, string?> with ':' as the hierarchy delimiter.
  {"Db": {"Timeout": 30}} is the single key Db:Timeout with the STRING "30".
- KEYS ARE CASE-INSENSITIVE. DB:TIMEOUT == db:timeout.
- VALUES ARE ALWAYS STRINGS. Every int, bool, TimeSpan, Uri is a binder conversion.
- LAST PROVIDER WINS. Add() appends; reads walk the list in reverse, first hit wins.
- ARRAY ELEMENTS ARE KEYS TOO: Servers:0:Host, Servers:1:Host.
- A duplicate key INSIDE one file provider throws FormatException.
  A duplicate key ACROSS providers is the whole point of the system.

Show that these are all the same read:
  config["Db:Timeout"]
  config.GetSection("Db")["Timeout"]
  config.GetSection("Db").GetValue<int>("Timeout")
  config.GetValue<int>("Db:Timeout", defaultValue: 30)
-->

---
layout: "statement"
---

<!-- OUTLINE.md # Slide 13 -->

Every scalar is text — or null.

<!--
[16-23 min] EVERYTHING IS A STRING - S2
60-min: keep - it's 20 seconds and it justifies the whole options block

Headline only.

Every int, every bool, every TimeSpan, every Uri you ever read out of
configuration is a BINDER CONVERSION that happened on your behalf. The
dictionary holds strings. Only strings. Always strings.

WHY THIS MATTERS ENOUGH FOR ITS OWN SLIDE:
This is the fact that makes S5 necessary rather than merely tidy. If every
value is a string, then every value can be the WRONG string - malformed,
empty, or a word where you wanted a number - and nothing in the configuration
system will tell you. Binding plus validation is where the type system
re-enters the story.

It is also why "30" and 30 are the same thing here, why a trailing space in an
environment variable ruins your day, and why an empty string is not null - at
least, it wasn't until .NET 10 (S9).
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 14 -->

```csharp
var dump = ((IConfigurationRoot)app.Configuration).GetDebugView(ctx =>
    ctx.Key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
    ctx.Key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
    ctx.Key.Contains("key", StringComparison.OrdinalIgnoreCase)
        ? "***"
        : ctx.Value);

app.Logger.LogInformation("Configuration:\n{Dump}", dump);
```

<!--
[16-23 min] THE DIAGNOSTIC - S2.1
The cold-open dump, now marked
60-min: NEVER CUT

This is the show-don't-tell moment of the talk, and it explains the cold open.
GetDebugView() prints every key, its effective value, AND the provider that
supplied it.

The cold-open dump returns, now with the gold marker. Walk it and point at the
line that explains the wrong value from minute zero.

Output shape:
  Db:
    Timeout=30 (JsonConfigurationProvider for 'appsettings.json' (Optional))
    ConnectionString=*** (EnvironmentVariablesConfigurationProvider Prefix: '')

SAY:
- The processValue overload takes Func<ConfigurationDebugViewContext, string>.
  Context carries Path, Key, Value, ConfigurationProvider.
- REDACT. Filter anything containing secret / password / key before printing.
  Logging GetDebugView() unredacted is anti-pattern #11.
- Gate it on IsDevelopment().
- To enumerate the chain directly: ((IConfigurationRoot)app.Configuration).Providers

This is the single most useful diagnostic in the entire system. If the audience
takes one thing home, make it this.
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 15 -->

```text {6-7}
Weather:
  ApiBaseUrl=https://localhost:7104
    (JsonConfigurationProvider for 'appsettings.Development.json')
  Retries=3
    (JsonConfigurationProvider for 'appsettings.json')
  TimeoutSeconds=10
    (EnvironmentVariablesConfigurationProvider Prefix: '')
  ApiKey=***
    (UserSecretsConfigurationProvider)
```

<!--
[16-23 min] THE DUMP - S2.1 - THE PAYOFF
The cold-open dump, marked this time
60-min: NEVER CUT

THIS IS THE SLIDE THE COLD OPEN WAS FOR.

The gold marker is on TimeoutSeconds=10, supplied by
EnvironmentVariablesConfigurationProvider - even though
appsettings.Development.json clearly says 120 and that is the file everyone
in the room was looking at for the last sixteen minutes.

SAY IT LIKE THIS:
"Sixteen minutes ago I showed you an app with the wrong timeout. Here's why.
The JSON says 120. An environment variable says 10. The environment variable
is later in the chain, so the environment variable wins. That's it. That's the
whole mystery."

Then name failure mode #1 - ORDER - and point at the badge when it turns up
two slides from now.

WHAT THE DUMP GIVES YOU THAT NOTHING ELSE DOES: the provider name in
parentheses. Not the value - anyone can log the value. The SOURCE.

Note ApiKey=*** is redacted by the lambda on the previous slide. Say that out
loud, because someone is about to go add GetDebugView to a production app.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 16 -->

# Default provider order

| | Source | |
| --- | --- | --- |
| 1 | Host / chained configuration | added LAST, so it wins |
| 2 | Command-line arguments | the provider runs twice |
| 3 | Environment variables | unprefixed |
| 4 | User secrets | Development only |
| 5 | `{ApplicationName}.settings.{Environment}.json` | |
| 6 | `{ApplicationName}.settings.json` | |
| 7 | `appsettings.{Environment}.json` | |
| 8 | `appsettings.json` | |

<Caption>

Rows 5 and 6 are the ones almost nobody knows exist — and they apply to web apps too.
Row 1 is the other surprise: read first, applied last.

</Caption>

<!--
[S3.1] DEFAULT PROVIDER ORDER
60-min: keep - compressed into the order block

HIGHEST PRIORITY FIRST. Reads top-down as "who wins".

The two rows people have never seen are 5 and 6 -
{ApplicationName}.settings.json and its environment variant. They are real,
and they apply to WEB APPS TOO, not just workers. WebApplication.CreateBuilder
constructs a HostApplicationBuilder internally, so it inherits them.
The known exception is CreateSlimBuilder, whose slim defaults omit them.

Verified on SDK 10.0.303: a file-based app web.cs really does probe
web.settings.json and web.settings.Development.json.

ROW 1 IS THE OTHER SURPRISE. The chained provider is added LAST, which makes
host configuration the highest-priority source, not the lowest. People assume
the opposite because it is read first. Read first, applied last.

ROW 2 - the command-line provider is registered TWICE: once during host
configuration so --environment can pick which files load, and once at the end
so it still wins the final read. That is deliberate, not a bug.

WHEN THE DUMP IS ON SCREEN the audience will count more providers than this
table has rows - two MemoryConfigurationProviders and the prefixed env var
providers. Say that those are host plumbing and move on; do not narrate all
thirteen.
-->

---
layout: "code"
codeSize: "16.5"
---

<!-- OUTLINE.md # Slide 17 -->

```text
$ dotnet run --environment Staging

MemoryConfigurationProvider
EnvironmentVariablesConfigurationProvider Prefix: 'ASPNETCORE_'
MemoryConfigurationProvider
EnvironmentVariablesConfigurationProvider Prefix: 'DOTNET_'
CommandLineConfigurationProvider                        <--  picks which file loads
JsonConfigurationProvider for 'appsettings.json'
JsonConfigurationProvider for 'appsettings.Staging.json'
JsonConfigurationProvider for 'web.settings.json'
JsonConfigurationProvider for 'web.settings.Staging.json'
EnvironmentVariablesConfigurationProvider
CommandLineConfigurationProvider                        <--  wins the final read
ChainedConfigurationProvider
```

<!--
[S3.2] HOST CONFIGURATION - THE TWO-PASS READ
60-min: keep - one slide, best "aha" in the order block

A real dump. One command, two effects.

Host configuration is read FIRST and it decides EnvironmentName. EnvironmentName
then decides WHICH appsettings.{Environment}.json even gets loaded. So the
command-line provider is registered TWICE on purpose:

  pass 1  so --environment Staging can influence which files load
  pass 2  so the same argument still wins the final read

Point at the two CommandLineConfigurationProvider lines. They are the same
provider, registered at both ends of the chain. That looks like a bug in the
framework until you see what it buys.

THE LINE TO SAY:
"ASPNETCORE_ENVIRONMENT is not just another setting. It is the input that picks
the rest of your inputs."

That reframing is what makes the two-phase design look deliberate instead of
accidental. It also explains why setting the environment variable AFTER the host
is built does nothing at all.

For a non-web host it is DOTNET_ENVIRONMENT, not ASPNETCORE_ENVIRONMENT. Say it
once; someone in the room is writing a worker this week.
-->

---
layout: "default"
class: "mark-row-4 mark-value"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 18 -->

<Badge>ORDER</Badge>

# Same key. Four winners.

| | |
| --- | --- |
| `appsettings.json` | 30 |
| `appsettings.Development.json` | 120 |
| `Weather__TimeoutSeconds` | 10 |
| **`--Weather:TimeoutSeconds`** | **5** |

<Caption>

*last one wins*

</Caption>

<!--
[layering] PRECEDENCE, SHOWN
Snippets: lifted from d02 and d03
60-min: keep this; the array slide is the FIRST thing to cut if you run long
Do not read the slide aloud. Walk down the four rows, name the source each
time, and let the numbers do the work.

THE LAYERS - JSON, then the environment file, then an env var, then a CLI arg.
Same key, four different winners, one at a time. Call out failure mode #1: ORDER.
This is what makes "last provider wins" concrete instead of abstract.

THE ARRAY SURPRISE is the next slide.
appsettings.json has ["a","b","c"]. The environment file has ["x"].
Result: ["x","b","c"] - NOT ["x"].

Why: the environment file MERGES OVER the base file key by key. It is not a
replacement. Arrays don't merge cleanly because index keys overlay individually:
  Servers:0 = "x"   (overwritten)
  Servers:1 = "b"   (survives)
  Servers:2 = "c"   (survives)

This gets an audible reaction every time. But it's a gasp, not a load-bearing
idea - it's the first cut at 60 minutes.

WHAT TO DO INSTEAD: prefer an object keyed by name, or replace the section
deliberately. Also: EnvironmentName is arbitrary - appsettings.QA-East.json works
fine if ASPNETCORE_ENVIRONMENT=QA-East.
-->

---
layout: "section"
kicker: "STAGE 1"
---

<!-- OUTLINE.md # Slide 19 -->

# Local dev

<!--
[31-39 min] STAGE 1 - LOCAL DEV
60-min: compress to 4 min

THE PROBLEM THIS STAGE SOLVES:
A developer clones the repo and it runs. Secrets never touch git.

Sources: appsettings.json, appsettings.Development.json, user secrets,
launchSettings.json

Close the block on its signature failure - "works on my machine" - which is what
pushes you to stage two.

ON SCREEN: the four sources and what each one yields, so the room can follow
the terminal without squinting. The last row is gold because it is the winner.
-->

---
layout: "panels"
---

<!-- OUTLINE.md # Slide 20 -->

# The environment file merges. Key by key.

<PanelRow :cols="3" size="10.6">

<!-- from: demos/d01-provider-dump/appsettings.json -->

<Panel caption="appsettings.json">

```json
"Weather": {
  "ApiBaseUrl": "https://api.example.com",
  "TimeoutSeconds": 30,
  "ApiKey": "placeholder-..."
}
```

</Panel>

<!-- from: demos/d01-provider-dump/appsettings.Development.json -->

<Panel caption="appsettings.Development.json">

```json
"Weather": {
  "ApiBaseUrl": "https://localhost:7104"
}
```

</Panel>

<!-- from: the merged result of the two files above -->

<Panel caption="WHAT THE APP SEES" dark>

```text
ApiBaseUrl       https://localhost:7104
TimeoutSeconds   30           <- survived
ApiKey           placeholder  <- survived
```

</Panel>

</PanelRow>

<Caption>

The surviving rows are the whole point. It merges key by key — it does not replace.

</Caption>

<!--
[31-39 min] JSON FILES - S4.1
60-min: keep, brief

appsettings.json - safe defaults, COMMITTED.
appsettings.Development.json - overrides only, COMMITTED.
ApiKey in the base file is a PLACEHOLDER (empty string), never a real value.

POINTS:
- The environment file merges over the base file KEY BY KEY. Not a replacement.
  (The array consequence is the next slide.)
- EnvironmentName is arbitrary. Development / Staging / Production are just the
  framework's well-known values.
- Build Action = Content, Copy = PreserveNewest - or the file isn't next to the
  DLL at runtime. This one bites people in published output, not in F5.

IF SOMEONE ASKS about taking full control:
  builder.Configuration.Sources.Clear();
  then AddJsonFile / AddEnvironmentVariables / AddCommandLine explicitly.
Mention it exists; don't dwell. Most apps shouldn't.

RULE OF THUMB TO STATE HERE (S7.3 #2):
appsettings.json should be safe to publish. If leaking it would matter, something
is in the wrong bucket.

ON SCREEN: three panels. Base, the environment overlay, and what the app
actually sees. Gold marks the values the environment file supplied.

THE ROW THAT TEACHES IT is TimeoutSeconds. The Development file says nothing
about it, so 30 SURVIVES from the base file. That is what "merges key by key"
means, and it is why this is not a file swap.

HOUSEKEEPING - say these, do not slide them:
- EnvironmentName is ARBITRARY. Development / Staging / Production are just the
  framework's well-known values. appsettings.QA-East.json works fine if
  ASPNETCORE_ENVIRONMENT=QA-East. People think the three names are magic.
- Build Action = Content, Copy = PreserveNewest, or the file is not next to the
  DLL at runtime. This bites in published output, never during F5, which is why
  it is always found in production.
-->

---
layout: "panels"
---

<!-- OUTLINE.md # Slide 21 -->

# Arrays overlay. They don't replace.

<PanelRow :cols="1" size="15.0">

<!-- from: demos/d03-array-merge/README.md captured output, abbreviated for the screen -->

<Panel caption="THE TWO FILES, AND WHAT YOU GET">

```text
appsettings.json               a, b, c
appsettings.Development.json   x

WHAT YOUR APP BINDS            x, b, c
```

</Panel>

<!-- from: demos/d03-array-merge/README.md — captured output, source column relabelled -->

<Panel caption="BECAUSE ARRAY ELEMENTS ARE KEYS" dark>

```text
Weather:AllowedOrigins:0  = https://x.example.com   Development
Weather:AllowedOrigins:1  = https://b.example.com   base, survived
Weather:AllowedOrigins:2  = https://c.example.com   base, survived
```

</Panel>

</PanelRow>

<Caption>

Only index 0 was overlaid. Precedence then runs per key, exactly as it always does.

</Caption>

<!--
[S4.1] THE ARRAY SURPRISE
Snippet: lifted from d03
60-min: FIRST THING TO CUT if you are running long - it is a gasp, not a
load-bearing idea

The reliable audible reaction of the talk. Set it up as a question:
"The base file has three hosts. The Development file has one. How many hosts
does the app see?"

Let them answer ONE. Then show three.

WHY: the environment file merges key by key, and array elements ARE keys.
There is no array in the dictionary - there is Hosts:0, Hosts:1, Hosts:2. The
override only writes Hosts:0. Nothing deletes Hosts:1 and Hosts:2, so they
survive. Point at the right-hand panel and say exactly that.

Call back to the flat dictionary slide. This is that slide's consequence, and
it is the moment people realise the mental model actually predicts behaviour
rather than just describing it.

WHAT TO DO INSTEAD - say it, because someone is about to go fix this today:
- Prefer an object keyed by name over a positional array.
- Or replace the whole section deliberately rather than relying on the overlay.
- Anti-pattern #10 is exactly this assumption in appsettings.Production.json.

This is the slide people quote back to you afterwards.
-->

---
layout: "panels"
---

<!-- OUTLINE.md # Slide 22 -->

# Not encrypted. Not a vault.

<PanelRow :cols="1" size="15.0">

<!-- from: demos/d09-user-secrets/D09.UserSecrets.csproj -->

<Panel caption="IN THE REPO">

```xml
<UserSecretsId>d09a1b2c-3d4e-5f60-7182-93a4b5c6d7e8</UserSecretsId>
```

<template #note>a pointer, and nothing else</template>

</Panel>

<!-- from: demos/d09-user-secrets/README.md — the directory the provider reports, plus the file name -->

<Panel caption="ON YOUR MACHINE" dark>

```text
%APPDATA%\Microsoft\UserSecrets\d09a1b2c-...\secrets.json

{ "Weather:ApiKey": "dev-key-12345" }
```

<template #note>plaintext, on disk, unencrypted</template>

</Panel>

</PanelRow>

<!--
[31-39 min] USER SECRETS - S4.4
Snippet: lifted from d09
60-min: keep - mention .NET 10 file-based apps in one line

WHAT IT IS NOT: not encrypted, not a vault.
THE SINGLE PURPOSE: keeping secrets out of the repo on a dev box.

  dotnet user-secrets init          (adds <UserSecretsId> to the .csproj)
  dotnet user-secrets set "Weather:ApiKey" "dev-key-12345"
  dotnet user-secrets list / remove / clear

Bulk load by piping JSON into 'dotnet user-secrets set'.

Storage - mention, but tell them NOT to write code against it:
  Windows: %APPDATA%\Microsoft\UserSecrets\<id>\secrets.json
  Linux/macOS: ~/.microsoft/usersecrets/<id>/secrets.json

Registered automatically by the default builders ONLY in Development.
It sits ABOVE the JSON files and BELOW environment variables and command line.

.NET 10 BONUS - file-based apps:
'dotnet run app.cs' programs get a stable UserSecretsId derived from a hash of
the file path.
  dotnet user-secrets set "ApiKey" "value" --file app.cs
A five-line single-file app reading configuration with no .csproj anywhere.
Worth its own slide at 90. At 60, one sentence.

Failure mode #4: TRUST. Name it.

ON SCREEN: the repo holds a GUID. Your laptop holds the actual secret, in
plaintext, in a file anyone with your login can read.

SAY IT PLAINLY: "This is not encryption. This is not a vault. The whole
feature is a convention for putting the file somewhere git will never see."

That is not a criticism - it is exactly the right tool for the job it has. But
half the room believes it is encrypted, and the ones who believe that are the
ones who will reach for it in production.

WHERE IT SITS IN THE CHAIN - say it, do not slide it:
Development environment ONLY. Above the JSON files, below environment
variables and the command line. The precedence table already showed the rank;
point back at it rather than repeating it.

Storage paths, for the record - and tell them NOT to write code against these:
  Windows       %APPDATA%MicrosoftUserSecrets<id>secrets.json
  Linux/macOS   ~/.microsoft/usersecrets/<id>/secrets.json
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 23 -->

```bash
dotnet user-secrets init
dotnet user-secrets set "Weather:ApiKey" "dev-key-12345"
dotnet user-secrets list
dotnet user-secrets remove "Weather:ApiKey"
dotnet user-secrets clear

# .NET 10 - file-based apps, no .csproj anywhere
dotnet user-secrets set "ApiKey" "value" --file app.cs
```

<!--
[S4.4] USER SECRETS - THE COMMANDS
Snippet: lifted from d09
60-min: keep, fast

Five commands and one .NET 10 addition. Do not read them out. Let people
photograph it and say what matters instead.

WHAT MATTERS:
- 'init' writes the UserSecretsId into the .csproj. That id is the only thing
  the repo ever learns about your secrets.
- Values are set by KEY PATH using the colon form - "Weather:ApiKey", not
  nested JSON. The provider flattens it exactly like everything else.
- Bulk load exists: pipe a JSON file into 'dotnet user-secrets set'. Useful
  when onboarding someone. Do not then commit that JSON file.

THE .NET 10 LINE IS THE ONE TO CALL OUT:
'dotnet run app.cs' programs get a stable UserSecretsId derived from a hash of
the FILE PATH. A single-file app with no .csproj anywhere still gets user
secrets, addressed with --file.

That is genuinely new and it lands well - a five-line program reading
configuration properly with no project file. Worth mentioning that moving the
file changes the hash, and therefore changes which secrets it sees.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 24 -->

<Badge>ORDER</Badge>

# The launchSettings trap

| you set this, in this shell | and this file exists |
| --- | --- |
| `$env:Weather__TimeoutSeconds = "10"` | `Properties/launchSettings.json` |
| `dotnet run` | `"environmentVariables": { "Weather__TimeoutSeconds": "45" }` |

<Caption gold>

**“Which one wins?”**

</Caption>

<!--
[31-39 min] "WORKS ON MY MACHINE" - S4.2 gotcha
Snippet: lifted from d07
60-min: keep if at all possible - this is the block's whole payoff

THE #1 "works on my machine" configuration story:

launchSettings.json environment variables OVERRIDE machine and user environment
variables during local F5 / dotnet run - and launchSettings.json is a
DEVELOPMENT-ONLY file that never deploys.

So: you set an env var, you run locally, nothing changes, you conclude env vars
don't work. Or worse - it works locally because of launchSettings, and the value
simply isn't there in production.

Show the two side by side: the env var you set, and the launchSettings entry
that quietly beats it. Then the same app with that entry deleted.

CLOSE THE STAGE HERE:
"That's stage one's signature failure. The fix isn't a better laptop - it's
letting the platform supply the values. That's stage two."

Failure mode #1 (ORDER) and #2 (SHAPE) both live here.
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 24a -->

```text
$ dotnet run
  Weather timeout is 45 seconds.
```

<Caption gold>

`launchSettings.json` is a development-only file that never deploys.

</Caption>

<Caption>

So the value you carefully set is beaten locally by a file that will not exist in production — where your environment variable is the only thing left.

</Caption>

<!--
[STAGE 1 REVEAL] THE launchSettings TRAP
Snippet: lifted from d07
60-min: keep - this is the whole payoff of the stage

Let them sit on 45 for a second before you explain it.

THE TWO-SIDED FAILURE, and both sides bite:
- Locally it works when it should not. launchSettings quietly wins, so you never
  find out your environment variable was not being read.
- In production it breaks when it should not. launchSettings is not deployed, so
  the value that was silently winning is simply gone.

That is why this is the number one "works on my machine" configuration story. It
is not that the machine is different. It is that a development-only file is in
the chain locally and absent everywhere else.

CLOSE THE STAGE HERE:
"That is stage one's signature failure. The fix is not a better laptop - it is
letting the platform supply the values. That is stage two."
-->

---
layout: "section"
kicker: "OPTIONS"
---

<!-- OUTLINE.md # Slide 25 -->

# Binding, and the three interfaces

<!--
[39-51 min] OPTIONS - S5
60-min: 8 min - this block survives the cut nearly intact

The longest non-flag block in the talk, and correctly so. This is where
"configuration" stops being strings and starts being types you can validate.

Snippets here: d11 binding, d12 ValidateOnStart, d13 the three interfaces.
The three-interfaces before/after pair is the longest beat in the talk.
DO NOT RUSH IT.

This is also the answer to the thesis objection from S1.3 - "then nothing is
knowable." Binding plus validation is what makes a configurable value MORE
knowable than a constant.
-->

---
layout: "panels"
---

<!-- OUTLINE.md # Slide 26 -->

# Stop injecting IConfiguration

<PanelRow :cols="1" size="13.3">

<Panel caption="DON'T">

```csharp
// DON'T - over-broad dependency, stringly typed, unvalidated, re-parsed
public sealed class WeatherClient(IConfiguration config)
{
    public Task<Forecast> GetAsync() =>
        CallAsync(config["Weather:ApiBaseUrl"]!,
                  int.Parse(config["Weather:TimeoutSeconds"]!));
}
```

</Panel>

<Panel caption="DO">

```csharp
// DO - the class declares exactly what it needs
public sealed class WeatherClient(IOptions<WeatherOptions> options)
{
    private readonly WeatherOptions _options = options.Value;
}
```

</Panel>

</PanelRow>

<Caption gold>

That `!` is anti-pattern #3 on its own.

</Caption>

<!--
[39-51 min] STOP INJECTING ICONFIGURATION - S5.1
60-min: keep

THE DON'T:
  public sealed class WeatherClient(IConfiguration config)
      => CallAsync(config["Weather:ApiBaseUrl"]!,
                   int.Parse(config["Weather:TimeoutSeconds"]!));

Four things wrong: stringly typed, unvalidated, untestable, re-parsed on every call.
Plus that null-forgiving '!' is anti-pattern #3 all by itself.

THE DO:
  sealed class WeatherOptions with const SectionName, [Required][Url] ApiBaseUrl,
  [Range(1,300)] TimeoutSeconds, [Range(0,10)] Retries, [Required][MinLength(8)] ApiKey
  - required init properties, defaults where sensible.

  public sealed class WeatherClient(IOptions<WeatherOptions> options)

THE ARGUMENT: the class now DECLARES EXACTLY WHAT IT NEEDS. You can read the
options type and know the whole configuration surface of that component. You
cannot do that with IConfiguration - it's a bag of everything.

Say the word "seam" again. This is the same ownership argument from S1.1 #5,
just inside the codebase instead of between teams.
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 27 -->

```csharp
public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    [Required, Url]            public required string ApiBaseUrl { get; init; }
    [Range(1, 300)]            public int TimeoutSeconds { get; init; } = 30;
    [Required, MinLength(8)]   public required string ApiKey { get; init; }
}

builder.Services
    .AddOptionsWithValidateOnStart<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .ValidateDataAnnotations();
```

<!--
[39-51 min] REGISTRATION - S5.2
Snippet: lifted from d11
60-min: merge into the previous slide if tight

  builder.Services
      .AddOptions<WeatherOptions>()
      .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
      .ValidateDataAnnotations()
      .Validate(o => o.Retries == 0 || o.TimeoutSeconds >= 5,
                "TimeoutSeconds must be >= 5 when retries are enabled.")
      .ValidateOnStart();

Or the shorthand that makes fail-fast the DEFAULT POSTURE:

  builder.Services
      .AddOptionsWithValidateOnStart<WeatherOptions>()
      .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
      .ValidateDataAnnotations();

Prefer the second form in every sample you show from here on. Modeling the good
default matters more than showing the long form.

ValidateDataAnnotations lives in Microsoft.Extensions.Options.DataAnnotations,
referenced implicitly by the web SDK - so it just works and people wonder why.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 28 -->

<Badge>LIFETIME</Badge>

# IOptions, IOptionsSnapshot, IOptionsMonitor

| | Lifetime | Re-reads config? | Use when |
| --- | --- | --- | --- |
| `IOptions<T>` | Singleton | No — bound once, forever | The value can't change at runtime |
| `IOptionsSnapshot<T>` | **Scoped** | Once per request | Per-request consistency in a web app |
| `IOptionsMonitor<T>` | Singleton | Yes, with `OnChange` | Singletons and background services |

<!--
[39-51 min] THE THREE INTERFACES - S5.3
Snippet: lifted from d13. THE LONGEST BEAT IN THE TALK. DO NOT RUSH IT.
60-min: NEVER CUT

  IOptions<T>          Singleton  no re-read     value can't change at runtime
  IOptionsSnapshot<T>  SCOPED     once per scope per-request consistency (web)
  IOptionsMonitor<T>   Singleton  yes + OnChange singletons & background services

BEFORE / AFTER PAIR: all three interfaces on one slide, then the same three
after appsettings.json changed underneath the running app.
Three different answers to the same question. That image is the slide.

THE CLASSIC BUG: injecting IOptionsSnapshot<T> into a singleton. It's scoped -
the container either throws at validation, or you capture the first scope's value
forever. Anti-pattern #4.

THE OTHER CLASSIC BUG: caching _monitor.CurrentValue in a field at construction.
That quietly turns a monitor back into an IOptions<T>. Anti-pattern #5.
Read CurrentValue at the point of use, every time.

TWO CONVENTIONS IN THE BACKGROUND SERVICE SAMPLE - say them, people copy this code:
- OnChange returns an IDisposable registration. HOLD IT AND DISPOSE IT or you leak
  the subscription.
- The log message is a TEMPLATE with named placeholders, never an interpolated
  string. Structured logging is the house rule.

Failure mode #3: LIFETIME. Name it here.
-->

---
layout: "reveal"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 28a -->

# Same three interfaces. One file edit.

```text
IOptions<T>          30
IOptionsSnapshot<T>  30
IOptionsMonitor<T>   30
```

<Caption>

*now `appsettings.json` changes to 90 underneath the running app.*

</Caption>

<!--
[OPTIONS] THE SETUP - BEFORE THE EDIT
Snippet: lifted from d13
60-min: NEVER CUT

All three interfaces, same key, same value. Nothing interesting yet - and that
is the point. Establish the baseline so the reveal has something to move
against.

ASK THE ROOM, and wait for an answer:
"I am about to change appsettings.json to 90 while this is running.
 Which of these three change?"

Most rooms say all three. Some say none. Both are wrong, and being wrong out
loud is what makes the next slide land.

Do not explain lifetimes yet. The table two slides back already told them; this
is the check on whether they believed it.
-->

---
layout: "reveal"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 28b -->

# Same three interfaces. One file edit.

```text
                          same request      next request
IOptions<T>          30        30                30      <- never moves
IOptionsSnapshot<T>  30        30                90      <- new scope only
IOptionsMonitor<T>   30        90                90      <- immediately
```

<Caption>

The middle column matters: a snapshot already resolved in the current request stays at 30. It is scoped, so it only picks up the change in a **new scope** — normally the next request.

</Caption>

<!--
[OPTIONS] THE REVEAL - AFTER THE EDIT
Snippet: lifted from d13
60-min: NEVER CUT. This is the longest beat in the talk.

Three columns, because the honest answer has a middle state:

  IOptions          never moves. Bound once at startup, forever.
  IOptionsSnapshot  moves, but only in a NEW SCOPE - normally the next request.
                    A snapshot already resolved in the current request stays at 30.
  IOptionsMonitor   moves immediately. It is a singleton holding a change token.

THE MIDDLE COLUMN IS THE ONE PEOPLE GET WRONG. "Scoped" does not mean "fresh
whenever you ask" - it means fresh per scope. Within one request it is stable,
which is exactly the property you want for per-request consistency.

THEN NAME THE TWO CLASSIC BUGS while this is up:
- IOptionsSnapshot<T> injected into a singleton. It is scoped: the container
  either throws, or you capture the first scope's value forever.
- _monitor.CurrentValue cached in a constructor field, which quietly turns a
  monitor back into an IOptions<T>. Read CurrentValue at the point of use.

Failure mode #3: LIFETIME. Badge is on the table slide; name it here.
-->

---
layout: "default"
codeSize: "15.0"
---

<!-- OUTLINE.md # Slide 29 -->

```csharp
builder.Services
    .AddOptionsWithValidateOnStart<WeatherOptions>()
    .Bind(builder.Configuration.GetSection("Weather"))
    .ValidateDataAnnotations();
```

```text
Unhandled exception. Microsoft.Extensions.Options.OptionsValidationException:
  DataAnnotation validation failed for 'WeatherOptions' members:
  'TimeoutSeconds' with the error: 'The field TimeoutSeconds must be between 1 and 300.'
```

<Caption>

The point is *where* this happened: at startup, before the process took traffic. Without `ValidateOnStart` it happens on first `.Value` access — in production, on the first request that reaches that code path.

</Caption>

<!--
[39-51 min] VALIDATION - S5.5
Snippet: lifted from d12
60-min: NEVER CUT

FAIL FAST IS THE WHOLE POINT.
Without ValidateOnStart(), validation runs LAZILY on first .Value access - a bad
deploy looks healthy until the first request reaches the affected code path. With
it, the process refuses to start. That's exactly the behavior you want during a
rolling deployment: the bad instance never takes traffic.

Bad config = no start. Show the startup exception naming the offending property.

NESTED OBJECTS AND COLLECTIONS ARE NOT VALIDATED BY DEFAULT. Opt in:
  [Required, ValidateObjectMembers]  on the nested object
  [ValidateEnumeratedItems]          on the collection
This surprises people who think [Required] recurses. It doesn't.

COMPLEX / CROSS-FIELD / SERVICE-DEPENDENT rules belong in a class:
  IValidateOptions<T>, registered with
  services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<T>, V>())
  (TryAddEnumerable, not Add - otherwise duplicate registrations pile up.)

Tie back to S1.3: this is the answer to "everything is configurable and nothing is
knowable." A validated option is more knowable than a buried constant.
-->

---
layout: "panels"
---

<!-- OUTLINE.md # Slide 30 -->

# Named options and source generators

<PanelRow :cols="1" size="14.9">

<Panel caption="NAMED">

```csharp
// Named - same shape, several times
builder.Services.Configure<EndpointOptions>("primary",   config.GetSection("Endpoints:Primary"));
builder.Services.Configure<EndpointOptions>("secondary", config.GetSection("Endpoints:Secondary"));

EndpointOptions Primary => monitor.Get("primary");
```

</Panel>

<Panel caption="GENERATED">

```csharp
// Generated - no reflection at runtime
[OptionsValidator]
public sealed partial class ValidateWeatherOptions : IValidateOptions<WeatherOptions>;
```

</Panel>

</PanelRow>

<Caption>

Plus one line, not a panel: `<EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>`

</Caption>

<!--
[39-51 min] NAMED OPTIONS + GENERATORS - S5.4, S5.6
60-min: CUT the generator half to a single sentence

NAMED OPTIONS - for "same shape, several times": multiple API clients, queues.
  services.Configure<EndpointOptions>("primary", config.GetSection("Endpoints:Primary"));
  services.Configure<EndpointOptions>("secondary", ...);
  then monitor.Get("primary")
Configure<T>(section) with no name is shorthand for Options.DefaultName ("").

SOURCE GENERATORS - binding and validation both default to REFLECTION. Two opt-ins:

  <EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>

  [OptionsValidator]
  public sealed partial class ValidateWeatherOptions : IValidateOptions<WeatherOptions>;
  (empty partial - the generator writes the implementation)

The options validation generator is ON BY DEFAULT when the project references
Microsoft.Extensions.Options 8+ or builds an ASP.NET Core app. It rewrites
[Range], [MinLength], [MaxLength], [Length] into generated equivalents.
With [OptionsValidator] you do NOT also call ValidateDataAnnotations().

OPTIONAL (d17, 90 min only): PublishAot=true surfaces IL2026 / IL3050 warnings
from reflection binding; turn both generators on and they disappear.
Niche. At 60 minutes this is one slide.
-->

---
layout: "section"
kicker: "STAGE 2"
---

<!-- OUTLINE.md # Slide 31 -->

# Deployment

<!--
[51-59 min] STAGE 2 - DEPLOYMENT
60-min: compress to 4 min, one slide

THE PROBLEM THIS STAGE SOLVES:
One artifact, many environments. The platform supplies the values.

Sources: environment variables, command line, key-per-file mounts, Key Vault.

Close the block on its signature failure - "the value didn't make it" - which is
what pushes you to stage three.

RULE OF THUMB TO LAND HERE (S7.3 #1):
One artifact, many environments. If you build a different binary per environment,
you have reintroduced Web.Release.config. Configuration is a deployment-time
input, not a build input.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 32 -->

<Badge>SHAPE</Badge>

# Environment variables

| what you export | the key it becomes |
| --- | --- |
| `Weather__ApiBaseUrl` | `Weather:ApiBaseUrl` |
| `Weather__AllowedOrigins__0` | `Weather:AllowedOrigins:0` |
| `ConnectionStrings__Default` | `ConnectionStrings:Default` |

<Caption gold>

**`:` is not portable in an environment variable name. `__` is.**

</Caption>

<!--
[51-59 min] ENVIRONMENT VARIABLES - S4.2
Snippet: lifted from d04
60-min: keep - this is the core of the compressed stage-2 slide

The workhorse in containers and every PaaS.

':' IS NOT PORTABLE in env var names. '__' is, and maps to ':'.
  Weather__ApiBaseUrl        -> Weather:ApiBaseUrl
  ConnectionStrings__Default -> ConnectionStrings:Default

Arrays use the index as a segment:
  Weather__AllowedOrigins__0, Weather__AllowedOrigins__1

PREFIXES:
  AddEnvironmentVariables(prefix: "MYAPP_") loads only matching variables AND
  STRIPS the prefix from the key.
  MYAPP_Weather__TimeoutSeconds -> Weather:TimeoutSeconds
  Useful for isolating one app on a shared host.

DOTNET_ and ASPNETCORE_ are RESERVED for host settings. Don't reuse them for app
config - you'll get behavior you didn't ask for.

Failure mode #2: SHAPE. This is where it lives - the key you set doesn't produce
the key the binder is looking for. Single underscore instead of double is the
most common version.

ALSO SAY (it comes back in S8): changing an environment variable on a running
container does NOTHING. Env vars are read once at startup. Restart the container.
-->

---
layout: "default"
codeSize: "15.0"
---

<!-- OUTLINE.md # Slide 33 -->

# Four became eleven

<BigNum from="4" to="11" />

```text
.NET 9      CUSTOMCONNSTR_   MYSQLCONNSTR_*   SQLCONNSTR_*   SQLAZURECONNSTR_*

.NET 10     + POSTGRESQLCONNSTR_*   DOCDBCONNSTR_        REDISCACHECONNSTR_
            + SERVICEBUSCONNSTR_    EVENTHUBCONNSTR_     NOTIFICATIONHUBCONNSTR_
            + APIHUBCONNSTR_

            * also sets ConnectionStrings:{KEY}_ProviderName
```

<!--
[51-59 min] CONNSTR PREFIXES - S4.2 - .NET 10 DELTA
Snippet: lifted from d05
60-min: keep as one line on the deployment slide - it's a good "new in 10" beat

Certain prefixed environment variables are rewritten into the ConnectionStrings:
section. This is an App Service compatibility behavior that most people have
never heard of.

.NET 9 recognized FOUR. .NET 10 recognizes ELEVEN.

  CUSTOMCONNSTR_{KEY}       -> ConnectionStrings:{KEY}
  MYSQLCONNSTR_{KEY}        -> + _ProviderName MySql.Data.MySqlClient
  SQLCONNSTR_{KEY}          -> + _ProviderName System.Data.SqlClient
  SQLAZURECONNSTR_{KEY}     -> + _ProviderName System.Data.SqlClient

  NEW IN 10:
  POSTGRESQLCONNSTR_{KEY}   -> + _ProviderName Npgsql
  DOCDBCONNSTR_{KEY}        (Cosmos DB)
  REDISCACHECONNSTR_{KEY}
  SERVICEBUSCONNSTR_{KEY}
  EVENTHUBCONNSTR_{KEY}
  NOTIFICATIONHUBCONNSTR_{KEY}
  APIHUBCONNSTR_{KEY}

d05 SHOWS: set POSTGRESQLCONNSTR_Default, then
Configuration.GetConnectionString("Default") resolving on .NET 10 where it
wouldn't have on .NET 9.

Short, concrete, and it's a genuinely new thing - good energy beat here.
-->

---
layout: "default"
codeSize: "15.0"
---

<!-- OUTLINE.md # Slide 34 -->

# Key-per-file

```text
/run/secrets/
  Weather__ApiKey            ->   Weather:ApiKey
  ConnectionStrings__Default ->   ConnectionStrings:Default
```

```csharp
AddKeyPerFile(dir, optional: true)                      // does NOT reload
AddKeyPerFile(dir, optional: true, reloadOnChange: true) // does
```

<Caption>

The path must be absolute. On Kubernetes the mounts are symlink swaps, so even the reloading overload may not fire — treat restart as the contract.

</Caption>

<!--
[51-59 min] KEY-PER-FILE - S4.7
Snippet: lifted from d23, or name it and move on
60-min: CUT to a name on the deployment slide

Docker secrets and Kubernetes mounted secrets are ONE FILE PER VALUE. This is how
you consume them without an Azure round trip.

  builder.Configuration.AddKeyPerFile(directoryPath: "/run/secrets", optional: true);

File NAME is the key, file CONTENTS are the value, '__' is the delimiter.
A file named Weather__ApiKey containing abc123 becomes Weather:ApiKey = abc123.

THE PATH MUST BE ABSOLUTE. Relative paths fail in a way that looks like "the
provider isn't working."

This is the right answer for AKS and Docker Swarm and it costs nothing. Worth
naming even when you skip the snippet - half the room is on Kubernetes and has been
shelling secrets in through env vars.

RELOAD - GET THIS RIGHT: the 3-argument overload above does NOT reload.
AddKeyPerFile(dir, optional) resolves to reloadOnChange: false. You only get
a watcher from the 4-argument overload. And even then, on Kubernetes those
mounts are symlink swaps rather than in-place writes, so the watcher may or
may not fire. Treat restart as the contract.
-->

---
layout: "default"
codeSize: "15.0"
---

<!-- OUTLINE.md # Slide 35 -->

# Azure Key Vault

| secret in the vault | key in configuration |
| --- | --- |
| `Weather--ApiKey` | `Weather:ApiKey` |

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{vaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

<Caption>

`--` becomes `:` because Key Vault forbids a colon in a secret name. No reload by default — `ReloadInterval` is null until you set it.

</Caption>

<!--
[51-59 min] KEY VAULT - S4.5
Snippet: lifted from d18. No Azure dependency now - it is code on a slide.
60-min: one slide covers it; the mechanics were never visual anyway.

  builder.Configuration.AddAzureKeyVault(
      new Uri($"https://{name}.vault.azure.net/"),
      new DefaultAzureCredential());
  - guarded by if (!builder.Environment.IsDevelopment())

FOUR THINGS THAT ACTUALLY MATTER:

1. SECRET NAMING. Key Vault forbids ':' in secret names. The default
   KeyVaultSecretManager maps '--' to the delimiter:
   secret Weather--ApiKey  ->  key Weather:ApiKey
   This is failure mode #2 (SHAPE) wearing a cloud hat.

2. REGISTER IT LAST so it overrides the file providers it's meant to replace.

3. NO RELOAD BY DEFAULT. Secrets are cached for the life of the process.
   ReloadInterval defaults to NULL - meaning never. Opt into polling:
     new AzureKeyVaultConfigurationOptions { ReloadInterval = TimeSpan.FromMinutes(30) }
   Tie to S7.3 #6: "It's in Key Vault" plus a process that caches it forever is a
   secret you cannot rotate.

4. EXPIRED SECRETS ARE STILL LOADED by default. Disabled secrets never are.
   Filter with a custom KeyVaultSecretManager overriding Load(SecretProperties).

CREDENTIALS: DefaultAzureCredential is the DEV CONVENIENCE chain (CLI, VS, env
vars, managed identity). In production prefer ManagedIdentityCredential
explicitly - narrower, and it FAILS FAST instead of silently walking the chain.
User-assigned identity: set AZURE_CLIENT_ID.

ISOLATION: one vault per app AND per environment. Don't multiplex with name
prefixes - that's anti-pattern #9.
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 36 -->

```csharp
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Weather:ApiBaseUrl"]    = "http://localhost/stub",
                ["Weather:TimeoutSeconds"] = "1",
            }));
}
```

<!--
[S4.8] IN-MEMORY - THE TESTING PROVIDER
Snippet: lifted from d24
60-min: cut the slide, keep the sentence

Added LAST, so it beats everything. That is the entire trick.

This is the right answer for WebApplicationFactory integration tests: you get
the real application, the real provider chain, the real binding and validation
- and then you overwrite exactly the handful of keys the test cares about.

WHY IT BEATS THE ALTERNATIVES:
- Mocking IConfiguration tests your mock, not your configuration.
- A test appsettings.json is a second file to keep in sync with the first.
- Environment variables in a test runner leak between tests.

Say the ordering point out loud, because it is the whole reason this works:
AddInMemoryCollection is appended, and reads walk the chain in reverse. It
does not matter what appsettings.json says. It does not matter what the
developer has in user secrets. The dictionary wins.

SAME TRICK FOR FEATURE FLAGS - forward reference to S6.7. A flag is just
configuration, so you set feature_management:feature_flags:0:enabled here and
test both sides of the branch. That is the slide people photograph in the
flags block.

The other use, if anyone asks: in-memory as code DEFAULTS, added FIRST so
everything overrides it. Rarer, but it is how you ship a sane fallback
without a file.
-->

---
layout: "code"
codeSize: "16.3"
---

<!-- OUTLINE.md # Slide 37 -->

```csharp
public sealed class DotEnvConfigurationSource(string path) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new DotEnvConfigurationProvider(path);
}

public sealed class DotEnvConfigurationProvider(string path) : ConfigurationProvider
{
    public override void Load() => Data = ParseDotEnv(path);
}
```

<!--
[S4.10] CUSTOM PROVIDER - THE EXTENSIBILITY POINT
Snippet: lifted from d25
60-min: cut - point at the repo

Two types. That is the whole contract.

  IConfigurationSource   - a factory. Build() hands back a provider.
  ConfigurationProvider  - has a protected IDictionary<string,string?> Data
                           and a virtual Load(). Fill the dictionary.

Then an extension method so it reads like every other provider:
  builder.Configuration.AddSqlConfiguration(connectionString);

THREE THINGS TO LAND, THEN MOVE ON:

1. Load() RUNS ONCE, at build time. It is not lazy. If you want refresh you
   schedule it yourself and call OnReload() - that is what fires the change
   token that makes IOptionsMonitor wake up. Point back at the three
   interfaces slide when you say it.

2. A provider that THROWS in Load() takes the app down at startup. That
   sounds like a bug and is frequently the feature - it is the same
   fail-fast posture as ValidateOnStart.

3. Good real targets: a database-backed provider, a .env reader, a provider
   that pulls from your own internal config service. Bad target: anything
   you could have done with the eleven providers that already exist.

Do not live-code this. Show the shape, say the three things, point at d25 in
the repo. It is interesting to about six people in the room and they will
read the code later.
-->

---
layout: "section"
kicker: "STAGE 3"
---

<!-- OUTLINE.md # Slide 38 -->

# Shared

<!--
[59-64 min] STAGE 3 - SHARED
60-min: compress to 3 min - labels + sentinel key only

THE PROBLEM THIS STAGE SOLVES:
Many apps and instances; values that change without a deploy.

Note this block is only 5 minutes even at 90, because S6 now carries App
Configuration's most interesting behavior. Show the sentinel key here and let
feature flags do the rest.

Close on the signature failure - "it changed, but not everywhere at once" -
which is what forces the discipline: sentinel keys, validation, flag hygiene.
-->

---
layout: "default"
codeSize: "15.0"
---

<!-- OUTLINE.md # Slide 39 -->

# Azure App Configuration

```text
1.  edit  Weather:Timeout      ->  60
2.  edit  Weather:Retries      ->  5
3.  edit  Weather:Sentinel     ->  v4      <-- last, always
                                             |
   next request TRIGGERS a refresh ---------+  it may still serve the OLD
                                               values; later requests see
                                               all three change together
```

```csharp
.ConfigureRefresh(refresh =>
{
    refresh.Register("Weather:Sentinel", refreshAll: true)
           .SetRefreshInterval(TimeSpan.FromSeconds(30));
})
```

<!--
[59-64 min] APP CONFIGURATION - S4.6
Snippet: lifted from d20. No Azure dependency now.
60-min: keep, compressed - labels and the sentinel key are the two ideas

Packages: Microsoft.Extensions.Configuration.AzureAppConfiguration
        + Microsoft.Azure.AppConfiguration.AspNetCore (refresh middleware)

THE SIX THINGS WORTH SAYING:

1. LABELS ARE THE ENVIRONMENT AXIS. One store, Development/Staging/Production
   labels, Selected in order so labeled values overlay unlabeled defaults.
   Select("Weather:*", LabelFilter.Null) then Select("Weather:*", envName).
   Load a SLICE, not the whole store.

2. REFRESH IS NOT AUTOMATIC. You must call ConfigureRefresh and then RegisterAll()
   or Register(key). Feature flags are the exception - UseFeatureFlags self-registers.
   This trips up everyone exactly once.

3. REFRESH IS ACTIVITY-DRIVEN in ASP.NET Core. The middleware checks on an
   incoming request once the interval has elapsed. AN IDLE APP NEVER REFRESHES.
   It is also ASYNCHRONOUS: the request that TRIGGERS the refresh does not
   block on it and may still serve the OLD values. Later requests see the new
   ones. Say this - people assume the triggering request gets the update.
   Background services must inject IConfigurationRefresherProvider and call
   TryRefreshAsync() themselves. Say this slowly - it's the #1 surprise.
   Also: app.UseAzureAppConfiguration() must go EARLY in the pipeline, or another
   middleware short-circuits before refresh ever runs.

4. THE SENTINEL KEY PATTERN - the thing to actually take home.
   Instead of RegisterAll(), watch ONE key you bump AFTER every other edit lands:
     refresh.Register("Weather:Sentinel", refreshAll: true)
   Update the sentinel LAST. Each instance then reloads the selected values
   together in one refresh, so no single process sees a half-applied edit.
   Do NOT say "atomic" - the store is not transactional across keys, and
   instances refresh independently, so they do not all switch at the same
   moment. It answers "half-applied", not "everywhere at once".

5. FAILURE MODE IS GRACEFUL - a failed refresh keeps last known-good and retries.
   STARTUP now retries with backoff, bounded by ConfigureStartupOptions =>
   options.Timeout. If it still cannot load when that budget runs out the
   failure surfaces during startup and normally aborts the process. So
   "a failed connection at startup crashes you" is true but incomplete -
   it retries first.

6. KEY VAULT REFERENCES live in the store as pointers with a distinct content
   type; ConfigureKeyVault dereferences them. Give them their OWN cadence with
   SetSecretRefreshInterval(key, TimeSpan) - a rotated secret is otherwise cached
   forever. MINIMUM is one minute - a shorter interval throws ArgumentOutOfRangeException
   rather than being silently floored. This is the
   App Configuration provider resolving Key Vault REFERENCES, not the
   standalone Key Vault provider, which has its own ReloadInterval. (Same rotation argument as the Key Vault slide.)

Also exists: Map() rewrites keys on the way in, e.g. App__Settings__X -> App:Settings:X.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 40 -->

# What actually reloads

| Source | Reloads? | How |
| --- | --- | --- |
| `appsettings*.json` | Yes (host default) | `FileSystemWatcher` |
| User secrets | Yes | file watcher |
| Environment variables | **No** | read once at startup |
| Command line | **No** | read once at startup |
| Azure Key Vault | Only if `ReloadInterval` set | polling |
| Azure App Configuration | Yes, with `ConfigureRefresh` | polling on activity |
| Key-per-file | **Only when `reloadOnChange` is enabled** | file watcher |
| In-memory | No | — |
| Custom | Your call | `OnReload()` |

<Caption>

The **No** rows are the ones that matter: changing an environment variable on a running container does nothing at all.

</Caption>

<!--
[S8] RELOAD SEMANTICS
60-min: show the table, do NOT walk it

  appsettings*.json   YES, when reloadOnChange: true (host default)  FileSystemWatcher
  User secrets        YES                                            file watcher
  Environment vars    NO - read once at startup
  Command line        NO - read once at startup
  Azure Key Vault     only if ReloadInterval is set (default: NEVER) polling
  App Configuration   YES, with ConfigureRefresh + a trigger         polling on activity
  Key-per-file        ONLY with the 4-arg overload                   file watcher
  In-memory           NO
  Custom              your call - OnReload()

FOUR CONSEQUENCES - say these out loud, the table alone doesn't land them:

- Changing an environment variable on a running container does NOTHING.
  Restart the container.
- reloadOnChange on a Kubernetes ConfigMap mount is UNRELIABLE - symlink swaps,
  not in-place writes. The watcher may or may not see them. Treat restart as the
  contract.
- A single file save often fires the watcher TWICE (write + metadata). Debounce
  anything expensive hanging off OnChange.
- "CONFIG RELOADED" DOES NOT MEAN "APP RECONFIGURED." Reloading configuration
  does not reconfigure things that read it once at startup: Kestrel endpoints,
  the DI graph, HttpClient handler pipelines.

RULE OF THUMB (S7.3 #4): prefer restart to hot reload unless hot reload is a
requirement someone actually asked for. Hot reload is a distributed-systems
problem wearing a config hat.
-->

---
layout: "section"
kicker: "FEATURE FLAGS"
---

<!-- OUTLINE.md # Slide 41 -->

# The second half of the thesis

<!--
[64-84 min] FEATURE FLAGS - S6
60-min: 10 min - S6.1-6.4 plus S6.6. Cut variants entirely.

ROUGHLY A QUARTER OF THE TALK. Not an appendix to Azure App Configuration - this
is the second half of the thesis from S1.3, and the only part of the
configuration story where a value changes while someone is watching.

NOTE FOR THE ABSTRACT: the submitted abstract does not mention feature flags. It
only implies them via "Azure App Configuration." If this abstract ever gets
resubmitted, add a clause. At 60 minutes especially, 20% of the room's time is
going somewhere the abstract didn't promise.

Block breakdown at 90:
  64-67  S6.1  a flag is just configuration
  67-72  S6.2-6.3  flags with zero cloud            d28
  72-77  S6.4  filters + the per-call gotcha        d29, d30
  77-80  S6.5  variants                             d31 (optional)
  80-84  S6.6 flag debt + S6.8 counterargument      d32
-->

---
layout: "default"
class: "mark-row-1"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 42 -->

# A feature flag is just configuration

| Stage | Where the flag lives | What you get |
| --- | --- | --- |
| **Local dev** | `appsettings.json` | Branch in code without branching in git. Zero cloud. |
| **Deployment** | environment variable | Ship dark, enable per environment |
| **Shared** | App Configuration | Flip at runtime, target a cohort, roll back in seconds |

<!--
[64-67 min] FLAGS ARE CONFIGURATION - S6.1
60-min: keep - this is the load-bearing idea of the whole flags block

Microsoft.FeatureManagement IS BUILT ON IConfiguration.
ANY configuration provider can back a feature flag. No new infrastructure, no new
file format, no service dependency.

That means flags inherit EVERYTHING from S2-S4: the provider chain, precedence,
reload semantics, and the debug view. A flag in appsettings.Development.json is
overridden by the same flag in an environment variable, by the same last-wins
rule as any other key.

FLAGS LIVE AT ALL THREE STAGES OF THE SPINE - this is why the slide matters:

  LOCAL DEV    appsettings.json / user secrets
               Branch in code without branching in git. ZERO CLOUD.
  DEPLOYMENT   environment variable per environment
               Ship dark, enable per environment at deploy time.
  SHARED       Azure App Configuration
               Flip at runtime, target a cohort, roll back in seconds.

SHOW THE FIRST ROW BEFORE ANYONE SEES AZURE.
It's the row that makes flags feel FREE. Most of the room believes feature flags
require LaunchDarkly or a platform team. They require a JSON file.
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 43 -->

```json
{
  "feature_management": {
    "feature_flags": [
      { "id": "NewCheckout", "enabled": false },
      { "id": "BetaBanner",  "enabled": true }
    ]
  }
}
```

```csharp
builder.Services.AddFeatureManagement();

if (await features.IsEnabledAsync("NewCheckout", ct)) { ... }
```

<Caption>

That is the entire setup. No cloud service, no cloud account, no network dependency — just a NuGet package and a JSON section.

</Caption>

<!--
[67-72 min] LOCAL FLAGS - S6.2, S6.3
Snippet: lifted from d28
60-min: keep

THE SCHEMA:
  "feature_management": { "feature_flags": [
      { "id": "NewCheckout", "enabled": false },
      { "id": "BetaBanner",  "enabled": true }
  ]}
  services.AddFeatureManagement();

SCHEMA NOTES WORTH SAYING OUT LOUD:
- feature_management / feature_flags is the MICROSOFT SCHEMA, shared across the
  .NET, Go, Python and JavaScript libraries. The older .NET-only FeatureManagement
  section still works; when both are present, feature_management wins.
- No "conditions" means the flag is simply enabled. WITH conditions, enabled:true
  means "eligible to be evaluated," NOT "on." That distinction catches people.
- requirement_type defaults to Any; set All when every filter must pass.
- A COLON IS FORBIDDEN in a flag name - flag names are configuration keys.
- Point the library at a custom section with AddFeatureManagement(config.GetSection("MyFlags")).

CONSUMING IT - four shapes, show at least two:
  IVariantFeatureManager.IsEnabledAsync("NewCheckout", ct)   <- the modern interface
  [FeatureGate("NewCheckout")] on a controller/action        <- 404 by default;
                                    override with IDisabledFeaturesHandler
  <feature name="BetaBanner">  and  <feature negate="true" name="BetaBanner">
  app.UseForFeature("NewCheckout", branch => branch.UseMiddleware<...>())

REGISTRATION DETAIL THAT BITES PEOPLE:
AddFeatureManagement() registers feature management as a SINGLETON. If a filter
needs scoped services (current user, a DbContext) you must use
AddScopedFeatureManagement() instead.

PACKAGES: FeatureGate, the tag helper, and UseForFeature need
Microsoft.FeatureManagement.AspNetCore. The core package covers everything else.
-->

---
layout: "default"
codeSize: "15.0"
---

<!-- OUTLINE.md # Slide 44 -->

# 50% of users, or 50% of calls?

```json
{
  "id": "NewCheckout",
  "enabled": true,
  "conditions": {
    "client_filters": [
      { "name": "Microsoft.Percentage", "parameters": { "Value": 50 } }
    ]
  }
}
```

<Caption gold>

**“Same user, same session, checks this flag twice. Same answer both times?”**

</Caption>

<!--
[72-77 min] FILTERS - S6.4
Snippets: lifted from d29 and d30
60-min: KEEP THE GOTCHA even if you cut the filter catalogue. Never-cut list.

BUILT IN, registered automatically by AddFeatureManagement() - except targeting,
which needs .WithTargeting<T>():

  Microsoft.Percentage   progressive rollout: 5% -> 25% -> 100%
  Microsoft.TimeWindow   scheduled enable/disable, Recurrence for daily/weekly
  Microsoft.Targeting    named users, groups, per-group percentages, exclusions

Targeting needs an ITargetingContextAccessor so the library knows who the current
user is:
  services.AddFeatureManagement().WithTargeting<HttpContextTargetingContextAccessor>();

*** THE SLIDE THAT EARNS ITS OWN MINUTE ***

PLAIN Microsoft.Percentage IS EVALUATED PER CALL, NOT PER USER.

A 50% flag checked twice in one request can answer DIFFERENTLY EACH TIME. A user
can watch the feature flicker between page loads. Nav bar says new checkout,
checkout page says old.

THIS IS THE MOST COMMON FEATURE-FLAG BUG IN THE WILD.

THE FIX: targeting, or variant allocation with a seed. That's what gives a STABLE
per-user assignment.

d29 is the flicker, d30 the stable version. If you only have room for one,
show the flicker and describe the fix - the bug is the memorable half.

CUSTOM FILTERS: implement IFeatureFilter (one method, EvaluateAsync), register
with .AddFeatureFilter<TenantFilter>(). Good example: a flag enabled per tenant.
-->

---
layout: "reveal"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 44a -->

# 50% of users, or 50% of calls?

```text
  D29 - Microsoft.Percentage is per-call, not per-user

  percentage   true  false true  true  false false true  false true  true
  targeting    true  true  true  true  true  true  true  true  true  true
```

<Caption gold>

`Microsoft.Percentage` is evaluated per call, not per user.

</Caption>

<Caption>

The nav bar says new checkout, the checkout page says old, and it is invisible in a single test run. The fix: targeting, or variant allocation with a `seed` — that is what gives a stable per-user assignment.

</Caption>

<!--
[FLAGS] THE REVEAL - PER CALL, NOT PER USER
Snippet: lifted from d29
60-min: KEEP THE GOTCHA even if you cut the filter catalogue. Never-cut list.

Ten checks. One user. One process. The percentage row flickers; the targeting
row does not. That contrast is the slide.

SAY IT PLAINLY:
"Microsoft.Percentage does not know who you are. It rolls the dice on every
 call. Fifty percent means half the CALLS, not half the USERS."

WHY IT IS NASTY: it is invisible in a single test run and invisible in code
review. It shows up as a user watching a feature flicker between page loads -
nav bar says new checkout, checkout page says old.

THE FIX, and say it immediately so nobody leaves with only the problem:
targeting with an ITargetingContextAccessor, or variant allocation with a seed.
Either gives a stable per-user assignment. A shared seed keeps that assignment
consistent across flags, which is what you want for a coherent experiment.
-->

---
layout: "code"
codeSize: "17.0"
---

<!-- OUTLINE.md # Slide 45 -->

```csharp
Variant variant = await features.GetVariantAsync("CheckoutLayout", ct);

var settings = new CheckoutLayoutSettings();
variant.Configuration.Bind(settings);   // it's an IConfigurationSection
```

<Caption>

The last line is the whole point: a variant hands you a configuration section, so you bind it like anything else in this talk.

</Caption>

<!--
[77-80 min] VARIANTS - S6.5
Snippet: lifted from d31 - optional even at 90
60-min: CUT. Mention in one sentence that flags can return values, move on.

A variant flag returns a VALUE - string, number, bool, or a whole configuration
object - instead of a boolean. This is where feature flags and the options
pattern meet, which is why it belongs in THIS talk and not a generic flags talk.

  Variant variant = await features.GetVariantAsync("CheckoutLayout", ct);
  variant.Configuration.Bind(settings);   // it's an IConfigurationSection -
                                          // bind it like anything else

That last line is the whole point. Land it and you can move on.

ALLOCATION is evaluated in order: user -> group -> percentile, falling back to
default_when_enabled, or default_when_disabled when the flag is off.
A SEED makes percentile assignment stable, and consistent ACROSS flags that share
the same seed. (This is the fix for the S6.4 gotcha.)

status_override (None / Enabled / Disabled) lets a variant flag also answer
IsEnabledAsync, so you can adopt variants without rewriting existing call sites.
You cannot override a flag whose enabled is false.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 46 -->

# Every flag is a permanent `if` with an owner and an expiry

| Kind | Lives for | Then |
| --- | --- | --- |
| **release** | days to weeks | delete after rollout |
| **experiment** | weeks | delete after the decision |
| **ops / kill switch** | permanent | and that's fine |
| **permission** | permanent | arguably not a flag — that's authorization |

<Caption gold>

**`NewCheckout` never gets deleted. `Checkout_V2_Rollout_2026Q1` files its own expiry.**

</Caption>

<!--
[80-84 min] FLAG DEBT - S6.6
Snippet: lifted from d32, the flag inventory endpoint
60-min: NEVER CUT. This is the closing argument.

THIS SECTION IS WHAT SEPARATES THE TALK FROM THE DOCUMENTATION.
It's the thing nobody else in the room will say.

Every flag is a permanent if-statement with an owner and an expiry date.

- A FLAG DOUBLES YOUR CODE PATHS. Ten live flags is up to 1,024 nominal
  combinations. You test maybe three of them.

- NAME FLAGS FOR THEIR REMOVAL.
  "NewCheckout" never gets deleted. It'll be there in 2031 and it will still be
  called New.
  "Checkout_V2_Rollout_2026Q1" files its own expiry.

- CATEGORIZE ON CREATION - lifetime differs by an ORDER OF MAGNITUDE:
    release     days to weeks    delete after rollout
    experiment  weeks            delete after the decision
    ops / kill  permanent        and that's fine
    permission  permanent        and arguably not a flag at all - that's authorization

- DELETING A FLAG IS A CODE CHANGE, NOT A CONFIG CHANGE. Turning it off in the
  portal and walking away leaves the dead branch compiling forever. This is the
  sentence people need to hear.

- INVENTORY THEM. GetFeatureNamesAsync() enumerates every flag the app knows
  about. A health endpoint listing flags and their current state is a five-minute
  build and THE ONE PEOPLE WILL ACTUALLY STEAL. That's d32.

TESTING (S6.7): flags are configuration, so the in-memory provider is the whole
story - or substitute IVariantFeatureManager outright.
THE RULE: test BOTH SIDES of every live flag, or you're shipping an untested
branch behind a switch someone can flip at 2am.
-->

---
layout: "default"
codeSize: "15.0"
---

<!-- OUTLINE.md # Slide 46a -->

# Deleting a flag is a code change, not a config change

```csharp
// internal diagnostics - authorize it; this leaks operational state
app.MapGet("/flags", async (IVariantFeatureManager features, CancellationToken ct) =>
{
    List<FlagRecord> inventory = [];
    await foreach (string name in features.GetFeatureNamesAsync(ct))
        inventory.Add(new FlagRecord(name, Category(name), Owner(name), Expiry(name),
                                     await features.IsEnabledAsync(name, ct)));
    return inventory;
}).RequireAuthorization();
```

<Caption>

Turning it off in the portal leaves the dead branch compiling forever. *Ten live flags is up to 1,024 nominal combinations. You test three.*

</Caption>

<!--
[FLAGS] DELETION AND INVENTORY
Snippet: lifted from d32
60-min: keep the first line; the endpoint is optional

THE SENTENCE PEOPLE NEED TO HEAR:
"Turning a flag off in the portal is not deleting it. The dead branch is still
 there, still compiling, still something the next person has to reason about."

Deleting a flag is a pull request, not a portal click. If that is not on
someone's board, it does not happen.

THE ENDPOINT IS THE THING PEOPLE STEAL. Two caveats, say both:
- Name and a boolean is not enough. The previous slide argued owner and expiry
  are what matter, so the record carries them.
- For a percentage or targeting flag that boolean is THIS evaluation, for THIS
  context. Slide 44 just proved it can differ on the very next call. It is an
  inventory of what exists, not a global on/off state.
- Authorize it. An open /flags endpoint hands an attacker your roadmap and your
  operational posture.
-->

---
layout: "statement"
---

<!-- OUTLINE.md # Slide 47 -->

Flags convert a deployment problem into a runtime problem.

You gain instant rollback. You lose *"the code that ran is the code in the commit."*

<!--
[80-84 min] THE COUNTERARGUMENT - S6.8
60-min: keep - it's 45 seconds and it closes the loop

Someone in the room is going to think it, so say it first:

FLAGS CONVERT A DEPLOYMENT PROBLEM INTO A RUNTIME PROBLEM.

You gain instant rollback. You lose "the code that ran is the code in the commit."

A flag flipped in a portal is a PRODUCTION CHANGE WITH NO PR, NO REVIEW, AND NO
DIFF - unless the store keeps history and you treat flag changes as deploys.
Azure App Configuration keeps revisions. Use them.

THE THROUGH-LINE CLOSES HERE:
That is the SAME TRADE as S1.2's build-time-to-runtime move, one level up.
Constants -> web.config -> Generic Host was trading build-time for runtime.
Deploys -> feature flags is the same trade again, one level up.

Call back to the history slide explicitly. This is the moment the talk becomes
one argument instead of a tour of providers.

End the flags block on JUDGMENT, not tooling.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 48 -->

# Sort every setting into three buckets

| | Examples | Where it belongs |
| --- | --- | --- |
| **Non-secret, per-environment** | base URLs, timeouts, retries, log levels, toggles | `appsettings.{Env}.json`, or App Configuration |
| **Secret** | API keys, connection strings with passwords | user secrets (dev) -> Key Vault (prod) |
| **Per-instance** | environment name, instance id, port, region | environment variables, set by the platform |

<Caption gold>

**If a value is in the wrong bucket, no amount of provider tuning fixes it.**

</Caption>

<!--
[84-87 min] THREE BUCKETS - S7.1
60-min: keep

  NON-SECRET, PER-ENVIRONMENT
    base URLs, timeouts, retry counts, log levels, feature toggles
    -> appsettings.{Env}.json in the repo, or App Configuration

  SECRET
    API keys, connection strings with passwords, client secrets
    -> user secrets (dev) -> Key Vault or the platform's secret store (prod)

  PER-INSTANCE / PER-DEPLOYMENT
    environment name, instance id, port, region
    -> environment variables, set by the platform

THE LINE TO SAY:
"If a value is in the wrong bucket, no amount of provider tuning fixes it."

That's the diagnostic. Most configuration pain people bring to you is a bucketing
error, not a provider error - and they've been trying to solve it with provider
order.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 49 -->

# By application shape

| App shape | Baseline | Secrets | Change without redeploy? |
| --- | --- | --- | --- |
| ASP.NET Core on App Service | JSON + App Service settings | Key Vault refs | App Service settings restart; App Config doesn't |
| Container / AKS | JSON + orchestrator env vars | key-per-file, or workload identity | restart the pod, or App Config |
| Many microservices | App Config + labels | Key Vault references | yes — sentinel + refresh |
| Worker / background | JSON + env vars | Key Vault via managed identity | `IOptionsMonitor` + explicit refresh |
| Console / CLI | JSON + command line | user secrets in dev | no — short-lived process |
| Desktop | JSON next to the EXE + AppData | **none in the client** | restart, or call your own API |

<!--
[84-87 min] CHOOSING A STRATEGY - S7.2, S7.3
60-min: keep - this is the one people photograph. Pause on it.

LEAVE THIS UP LONGER THAN FEELS COMFORTABLE. Count to five. Let them get the shot.

  ASP.NET Core on App Service   JSON + App Service settings; KV refs
                                App Service settings RESTART the app;
                                App Configuration doesn't
  Container / AKS               JSON baked in + orchestrator env vars;
                                key-per-file mount or KV via workload identity
  Many microservices            App Configuration, labels + per-app key prefix;
                                KV references; sentinel key + refresh
  Worker / background service   JSON + env vars; KV via managed identity;
                                IOptionsMonitor + explicit TryRefreshAsync
  Console / CLI                 JSON + command line + env vars; user secrets in
                                dev; NO hot reload - short-lived process
  Desktop (WinForms/WPF/MAUI)   JSON next to the EXE + per-user AppData;
                                NO SECRETS IN THE CLIENT

SIX RULES OF THUMB (S7.3) - read 1, 2, and 5 aloud at minimum:
  1. One artifact, many environments. A different binary per environment is
     Web.Release.config again.
  2. Commit defaults, never secrets. appsettings.json should be safe to publish.
  3. Reach for cloud configuration when you have a REAL reason: more than one app
     sharing values, or a need to change without a deploy. One app that redeploys
     in five minutes does not need App Configuration.
  4. Prefer restart to hot reload unless someone asked for hot reload.
  5. Validate at startup, always. Cheapest guardrail on every strategy above.
  6. Secrets get a rotation story or they aren't secure.

Rule 2 is on the never-cut list at any talk length.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 50 -->

# Don't do this

| | The one that costs you most |
| --- | --- |
| **ORDER** | assuming an array in `appsettings.Production.json` replaces the base array |
| **SHAPE** | `config["Some:Key"]!` with a null-forgiving operator and no validation |
| **LIFETIME** | `IOptionsSnapshot<T>` injected into a singleton |
| **TRUST** | secrets committed in `appsettings.json` — or worse, `appsettings.Production.json` |

<!--
[87-90 min] ANTI-PATTERNS - S10
60-min: keep, rapid-fire

Read these FAST. Don't explain them - by now the audience has seen every one.
The point is recognition, not instruction. If they're calling out the four
failure modes before you do, this slide is working.

 1. Secrets committed in appsettings.json - or worse, appsettings.Production.json.
 2. IConfiguration injected into business classes instead of bound options.
 3. config["Some:Key"]! with a null-forgiving operator and no validation.
 4. IOptionsSnapshot<T> in a singleton.
 5. IOptionsMonitor<T>.CurrentValue captured in a constructor field.
 6. Building a throwaway ConfigurationBuilder mid-request to "get a fresh value."
 7. Calling WebApplication.CreateBuilder() a second time just to read configuration.
 8. if (env.IsProduction()) branches in code instead of environment-specific
    configuration VALUES.
 9. One Key Vault for every app and environment, separated by secret-name prefixes.
10. Assuming an array in appsettings.Production.json replaces the base array.
    It doesn't. (Callback to the array slide - they'll remember.)
11. Logging GetDebugView() without redaction.
-->

---
layout: "default"
codeSize: "15"
---

<!-- OUTLINE.md # Slide 51 -->

# The spine assumes something that gets deployed

| | Server | Desktop |
| --- | --- | --- |
| Local dev | yes | yes |
| Deployment | a deploy | an **install** |
| Shared | App Configuration | **no directly trusted shared provider in the client** |

<Caption>

- `SetBasePath(AppContext.BaseDirectory)` — the working directory of a double-clicked
  EXE is not the install directory
- `IConfiguration` has an indexer setter but **no persistence API** — nothing writes
  back to the JSON. User preferences are a different problem
- **No cloud secrets in a client binary.** Authenticate the user, call a backend

</Caption>

<!--
[87-90 min] DESKTOP - S4.11
60-min: compress to one line inside the choosing slide

THE ABSTRACT PROMISES "cloud-native microservices OR DESKTOP APPS." This is the
section that pays it off. If you're short on time, this is the honest thing to cut
- but say the one-liner.

STRUCTURAL HONESTY - name this rather than letting the structure contradict you:
The S1.4 spine assumes a SERVER. A desktop app has local dev, has "deployment" as
an INSTALL, and has NO STAGE THREE at all - no orchestrator setting env vars, no
central store it should authenticate to, no instance fleet.

THE LINE: "The spine assumes something that gets deployed. Here's what changes
when it gets installed on someone's laptop instead."

The configuration MODEL is identical. What differs is what's wired up for you.
- Console apps and workers get the full default chain from the generic host.
  Same JSON + user secrets + env vars + command line, same precedence - but
  DOTNET_ENVIRONMENT, not ASPNETCORE_ENVIRONMENT, outside ASP.NET Core.
- WinForms/WPF have no host by default. Either build IConfiguration by hand, or
  (preferred) host the desktop app and get DI, logging, and options for free.

FOUR DESKTOP REALITIES:
- SetBasePath(AppContext.BaseDirectory). The working directory of a double-clicked
  EXE is NOT the install directory. Directory.GetCurrentDirectory() will bite you.
- Per-user WRITABLE settings are a different problem. IConfiguration is
  read-optimized and has NO WRITE API. User preferences belong in a JSON file
  under SpecialFolder.ApplicationData that you load as an extra provider and save
  yourself.
- app.config isn't gone, but ConfigurationManager.AppSettings and IConfiguration
  are two UNRELATED systems. Migrating means moving keys, not bridging them.
- CLIENT APPS MUST NOT HOLD CLOUD SECRETS. No Key Vault credential ships in a
  desktop binary. Authenticate the USER, call a backend that holds the secret.
  SAY THIS OUT LOUD - someone always asks.

Flags are the interesting exception: a desktop app absolutely can consume flags
from a file or your own backend. It just can't hold the credential to a flag store.
-->

---
layout: "default"
codeSize: "15.0"
---

<!-- OUTLINE.md # Slide 52 -->

# Null is preserved now

```json
{ "StringProperty": null, "IntProperty": null, "Array1": [null, null], "Array2": [] }
```

| Property | .NET 9 | .NET 10 |
| --- | --- | --- |
| `string?` (initialized) | `""` — already overwritten | `null` |
| `int?` (initialized) | **kept its initializer** | `null` |
| non-nullable value type | **threw** | `default(T)` |
| `[null, null]` | two empty strings | two nulls |
| `[]` | ignored | binds as empty array |

<Caption>

All rows are **the JSON provider specifically** — providers that already carried real nulls, such as in-memory, did not all behave this way. d06’s own one-liner: *“Retries was 3 on .NET 9 and is null on .NET 10.”*

</Caption>

<!--
[S9] .NET 10 DELTAS
60-min: fold into the slides where each one lives; skip as a standalone

Four talk-relevant deltas from .NET 9:

1. NULL VALUES ARE PRESERVED - *** BREAKING CHANGE ***
   Previously a null in configuration was treated as MISSING and skipped by the
   binder, and the JSON provider converted null to "".
   In .NET 10 the JSON provider reports null unchanged and the binder binds it
   like any other value. Binding of null array elements and empty arrays now works.
   -> A property that used to KEEP ITS DEFAULT when the JSON said null now gets
      OVERWRITTEN WITH NULL.
   This is the one that will actually break someone's upgrade. d06 has the
   before/after. Worth 60 seconds even at 60 minutes.

2. SEVEN NEW CONNECTION-STRING PREFIXES (11 total) - Postgres, Cosmos, Redis,
   Service Bus, Event Hubs, Notification Hubs, API Hubs. Covered on the connstr
   slide; don't repeat it here.

3. AOT-SAFE ValidationContext CONSTRUCTOR taking an explicit displayName,
   removing AOT warnings from options validation.

4. FILE-BASED APPS (dotnet run app.cs) participate in user secrets via a
   path-hash-derived UserSecretsId and 'dotnet user-secrets --file'.

OPTIONAL FORWARD-LOOKING CLOSER - label it clearly as .NET 11 PREVIEW:
generic OptionsBuilder<T>.Validate<TValidator>(), Func<Task> overloads on
ChangeToken.OnChange, and async DataAnnotations validation.
-->

---
layout: "section"
kicker: ""
---

<!-- OUTLINE.md # Slide 53 -->

# Questions

<!--
[Q&A]
60-min: keep - but see the note below about where questions actually go

THE ONE-LINE TAKEAWAY, say it BEFORE you open the floor - otherwise the last
thing the room hears is whatever the final question happened to be:

"GetDebugView() tells you where the value came from. Validate at startup so the
wrong value never takes traffic. And name every flag for the day you delete it."

AT 90 MINUTES: questions have been riding along, so this is a genuine open
floor with a few minutes of slack.

AT 60 MINUTES: the running order takes questions at three breath points -
after the model (14), after Options (31), after flags (48) - so this slide is
a backstop, not the main event.

QUESTIONS YOU WILL GET, AND THE SHORT ANSWERS:

- "When should I NOT use Azure App Configuration?"
  S7.3 rule 3. One app that redeploys in five minutes doesn't need it.
  This is the best question, so if nobody asks it, ask it yourself.

- "What about Aspire?"
  It shapes how configuration is WIRED between services; the provider chain
  underneath is the same model in this talk.

- "Should secrets go in App Configuration?"
  No - Key Vault references. The store holds the pointer, not the secret.

- "How do I test any of this?"
  S4.8 in-memory provider, added last so it beats everything. Same answer for
  feature flags (S6.7).

- "Does IOptionsMonitor work in a console app?"
  Yes, but nothing triggers a refresh for you. That's the S4.6 activity-driven
  point - call TryRefreshAsync yourself.

If the room goes quiet, go to the flag-inventory endpoint (d32). It's the
thing people ask about afterward anyway.
-->

---
layout: "cover"
variant: "thanks"
image: "/kevin-griffin.png"
---

<!-- OUTLINE.md # Slide 54 -->

<!-- from: README.md §Speaker -->

# Let's keep talking.

- consultwithgriff.com
- github.com/1kevgriff/modern-dotnet-configuration
- X · LinkedIn · GitHub — @1kevgriff
- Bluesky — @consultwithgriff.com

<!--
[thanks]
60-min: keep, 1 min

LEAVE THIS UP for the rest of the room's time, and through the hallway track.
People photograph the last slide. Put the repo QR here.

  Repo:      github.com/1kevgriff - modern-dotnet-configuration
             (slides, all demos numbered in presentation order, each with its
             own README and the point it's making)
  Web:       consultwithgriff.com
  X:         @1kevgriff
  Bluesky:   @consultwithgriff.com
  LinkedIn:  in/1kevgriff
  YouTube:   youtube.com/@consultwithgriff
  Speaking:  sessionize.com/kevingriffin

Thank the room and thank the organizers by name.

If you have slack left, the honest closer is the callback to S1.2 and S6.8:
"Twenty years ago we traded a build-time decision for a runtime one. Feature
flags are us making that same trade again. It's a good trade - as long as you
remember you made it."54
-->
