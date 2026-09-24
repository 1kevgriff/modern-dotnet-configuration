# Proposed running order — the developer's journey

Working document. Maps all 59 existing slides onto the five acts. **Nothing has been moved yet.**

Ids are `OUTLINE.md` slide ids. `→` marks a slide that moves from where it is today.

---

## Opening (unchanged)

`1` `2` cold open · `3` title · `4` about · `5` roadmap **(rewrite to the five acts)** ·
`6` why externalize · `7` three eras · `8` build-time→runtime trade · `9` thesis

`10` — was the `THE SPINE` divider. **Repurpose** into the journey map:
*my laptop → the team's baseline → a shared dev server → production → coordinated runtime change.*

`11` four failure modes — keep here. It is the lens the whole journey uses.

---

## Act 1 — SHARED BASELINE · "We agree on defaults"

| | |
| --- | --- |
| new | **Divider:** *`appsettings.json` is a team decision, checked in.* |
| `12` | It's one flat dictionary |
| `13` | Every scalar is text — or null |
| `21` → | Arrays overlay. They don't replace. *(moves up: it is an overlay rule, not a local-dev topic)* |

## Act 2 — MY MACHINE · "Local differences without changing everyone else's"

| | |
| --- | --- |
| `19` | **Divider**, retitled: *Clone the repo and it runs — and no secret touches git.* |
| `20` | The environment file merges. Key by key. |
| `22` `23` | User secrets: not encrypted, not a vault · the CLI |
| `24` `24a` | The launchSettings trap — setup and reveal |
| `16` → | Default provider order *(moves here: "which source wins" is a my-machine question)* |
| `17` → | Host configuration, the two-pass read |
| `18` → | Same key. Four winners. |
| `14` `15` | GetDebugView, and the marked dump — **the cold-open payoff** |

## Act 3 — SHARED DEV · "The application has left my laptop"

| | |
| --- | --- |
| new | **Divider:** *launchSettings and user secrets are gone. The environment supplies the values now.* |
| `32` `33` | Environment variables · four became eleven |
| `34` `35` | Key-per-file · Key Vault |
| `26` `27` | Stop injecting IConfiguration · the options type |
| `29` | ValidateOnStart — *proves the environment is complete* |
| `36` `37` | In-memory for tests · a custom provider |

## Act 4 — PRODUCTION · "Same app, production ownership and trust"

| | |
| --- | --- |
| new | **Divider:** *Production supplies the values. Nothing is rebuilt to change one.* |
| `28` `28a` `28b` | The three interfaces, and what actually moves |
| `30` | Named options and source generators |
| `40` | What actually reloads |
| `38` | **Divider**, retitled: *Shared configuration adds coordination — and new failure modes.* |
| `39` | App Configuration and the sentinel |
| `41` | **Divider**, retitled: *Flags are configuration that changes behaviour, not just values.* |
| `42`–`47` | The flag block, unchanged in order |

## Act 5 — DECISION PAYOFF · "Where should this value live?"

| | |
| --- | --- |
| new | **Divider:** *Choose by ownership, sensitivity, scope, and change cadence.* |
| `48` `49` | Three buckets · by application shape |
| `50` | Don't do this |
| `51` | Desktop — install, not deploy |
| `53` `54` | Questions · thanks |

`52` (null is preserved now) → **moves to `30a`**, beside binding, per the earlier direction.

---

## The recurring example

`Weather:TimeoutSeconds` already crosses acts 1, 2 and 4. Pairing it with `Weather:ApiKey`
covers the secret story end to end: placeholder in the baseline → user secrets on my machine →
Key Vault in the shared environment → never in a desktop client.

Both already exist in `demos/d01`, so this needs no new content.

## Three judgement calls I made

1. **The cold-open payoff stays in Act 2, not Act 4.** Slides 1–2 pose a mystery; slide 15
   resolves it. Moving `GetDebugView` to production would leave it unresolved for ~40 minutes,
   and the audience review singled the cold open out as the deck's strongest opening. "Make
   diagnostics operational" is served by a callback in Act 4 instead of relocating the slide.

2. **Precedence (`16` `17` `18`) moves *up* into Act 2.** "Why does my value differ from my
   coworker's?" is the my-machine question, and it is what the launchSettings trap needs.

3. **Arrays (`21`) move up into Act 1**, next to the flat-dictionary model. It is an overlay
   rule, not something specific to local development.
