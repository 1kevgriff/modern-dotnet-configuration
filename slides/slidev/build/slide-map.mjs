// Sidecar for the OUTLINE.md -> slides.md generator.
//
// OUTLINE.md supplies headlines, code blocks, tables and speaker notes. This file supplies
// the two things it cannot:
//
//   1. the LAYOUT each slide uses
//   2. content for the handful of slides where OUTLINE.md only *describes* a visual
//      ("Left, captioned WHAT YOU WROTE: nested JSON") without carrying the content
//
// Every authored block carries `from:` naming where it was lifted from. Nothing here is invented;
// it is all either in /demos, in the slide's own ## Notes, or in README.md.
//
// Plain ESM, not YAML, on purpose: indented code inside an indentation-sensitive format is the
// exact class of bug that mangled the PowerPoint build.

const SITE = 'consultwithgriff.com'

export const map = {
  // ---- cold open -------------------------------------------------------------------
  0: { layout: 'blank', prose: 'drop' },
  1: { layout: 'code' },
  2: { layout: 'code' },

  3: {
    layout: 'cover',
    variant: 'title',
    headline: 'Modern .NET Configuration',
    // No kicker. A three-noun strapline over the title is exactly the filler Kevin cut
    // from the PowerPoint deck; it does not come back.
    lines: ['Kevin Griffin', 'Microsoft MVP'],
    from: 'OUTLINE.md slide 3 byline',
    prose: 'drop'
  },

  4: {
    layout: 'cover',
    variant: 'bio',
    headline: 'Kevin Griffin',
    lines: [
      'Independent Consultant',
      'CTO, Shows On Sale',
      'Microsoft MVP',
      SITE + '  ·  @1kevgriff',
    ],
    from: 'OUTLINE.md slide 4 + README.md §Speaker',
  },

  '4a': { layout: 'default', prose: 'keep', qr: '/repo-qr.svg' },

  // no longer a roadmap - a definition, stated once
  5: { layout: 'statement', prose: 'keep' },

  6: {
    layout: 'cards',
    cols: 5,
    cards: [
      { n: '01', title: 'No hard-coding' },
      { n: '02', title: 'Varies by environment' },
      { n: '03', title: 'Change it on the fly' },
      { n: '04', title: 'Trust', accent: true },
      { n: '05', title: 'Ownership', accent: true },
    ],
    clicks: true,
    from: 'OUTLINE.md slide 6 — card titles inline, emphasis as described',
    prose: 'drop',
  },

  7: { layout: 'code' },
  '7a': { layout: 'code' },
  '7b': { layout: 'code' },
  8: { layout: 'statement' },
  9: {
    // NOT `statement`: that layout renders only a blockquote, and this slide's content is
    // a headline plus a table. As a statement it rendered completely blank.
    layout: 'default',
    prose: 'keep',
  },

  // ---- the journey -----------------------------------------------------------------
  10: {
    layout: 'section',
    footnote: 'Each boundary changes *who supplies the value* and *what it costs to get it wrong*. The application does not change — only the answer to “where did this come from?” does.',
  },

  // ---- ACT 1 - the shared baseline --------------------------------------------------
  '11a': { layout: 'code', prose: 'keep' },
  // the default load, demonstrated before it is theorised. Numbers verified in
  // scratchpad/loadproof/EVIDENCE.txt on SDK 10.0.303.
  '11b': { layout: 'code', prose: 'keep' },
  '13a': { layout: 'default', prose: 'keep' },
  '18a': { layout: 'code' },
  '18c': { layout: 'code', prose: 'keep' },
  '18b': { layout: 'code', prose: 'keep' },

  12: {
    layout: 'panels',
    arrow: true,
    prose: 'drop',
    panels: [
      {
        caption: 'THREE WAYS TO SPELL IT',
        lang: 'text',
        from: 'the three syntaxes used across demos/d01, d02 and d08',
        body: `appsettings.json   "Weather": { "TimeoutSeconds": 30 }

environment        Weather__TimeoutSeconds=10

command line       --Weather:TimeoutSeconds=5`,
      },
      {
        caption: 'ONE KEY, ONE DICTIONARY',
        lang: 'text',
        dark: true,
        from: 'the flat key all three produce; values are strings',
        body: `Weather:TimeoutSeconds   "30"
Weather:TimeoutSeconds   "10"
Weather:TimeoutSeconds   "5"

same key, three providers - last one wins`,
      },
    ],
    footnote: 'JSON nesting and an environment variable’s `__` both produce colon-separated keys. Every value arrives as text — or null.',
  },

  13: {
    layout: 'code',
    prose: 'keep',
  },
  14: { layout: 'code', prose: 'drop' },
  15: { layout: 'code' },
  16: {
    layout: 'default',
    prose: 'drop',   // the reading-direction label is the blockquote, rendered above the table
    // "row 1 beats row 8" is now the gold label above the table, so this keeps only the
    // fact that label does not carry.
    footnote: '.NET 10 also loads the application-named settings files. They apply to web apps too, and almost nobody knows they exist.',
  },
  17: { layout: 'code', prose: 'drop' },
  18: {
    // Observed outputs now, not a styled table - the values come from a real run.
    layout: 'code',
    prose: 'keep',
  },

  // ---- stage 1: local dev ----------------------------------------------------------
  19: { layout: 'section' },

  20: {
    layout: 'panels',
    panels: [
      {
        caption: 'appsettings.json',
        lang: 'json',
        from: 'demos/d01-provider-dump/appsettings.json',
        body: `"Weather": {
  "ApiBaseUrl": "https://api.example.com",
  "TimeoutSeconds": 30,
  "ApiKey": "placeholder-..."
}`,
      },
      {
        caption: 'appsettings.Development.json',
        lang: 'json',
        // Overrides TimeoutSeconds too, so this agrees with the 30 -> 120 the previous
        // slide just measured. ApiKey is deliberately absent, to show what survives.
        from: 'the running example, matching the measured 30 -> 120',
        body: `"Weather": {
  "ApiBaseUrl": "https://localhost:7104",
  "TimeoutSeconds": 120
}`,
      },
      {
        caption: 'WHAT THE APP SEES',
        lang: 'text',
        dark: true,
        // Kept to ~42 columns so the three panels stay SIDE BY SIDE. Stacking them would
        // lose the base | override | result comparison that is the whole slide.
        from: 'the merged result of the two files above',
        body: `ApiBaseUrl       https://localhost:7104
TimeoutSeconds   120
ApiKey           placeholder`,
      },
    ],
    footnote: '`ApiKey` is not in the Development file at all, so it survives from the base. The merge runs key by key; the file is not swapped out.',
  },

  21: {
    layout: 'panels',
    panels: [
      {
        caption: 'THE TWO FILES, AND WHAT YOU GET',
        lang: 'text',
        from: 'demos/d03-array-merge/README.md captured output, abbreviated for the screen',
        body: `appsettings.json               a, b, c
appsettings.Development.json   x

WHAT YOUR APP BINDS            x, b, c`,
      },
      {
        caption: 'BECAUSE ARRAY ELEMENTS ARE KEYS',
        lang: 'text',
        dark: true,
        from: 'demos/d03-array-merge/README.md — captured output, source column relabelled',
        body: `Weather:AllowedOrigins:0  = https://x.example.com   Development
Weather:AllowedOrigins:1  = https://b.example.com   base, survived
Weather:AllowedOrigins:2  = https://c.example.com   base, survived`,
      },
    ],
    footnote: 'Only index 0 was overlaid. Precedence then runs per key, exactly as it always does.',
  },

  22: {
    layout: 'panels',
    panels: [
      {
        caption: 'IN THE REPO',
        lang: 'xml',
        from: 'demos/d09-user-secrets/D09.UserSecrets.csproj',
        body: `<UserSecretsId>d09a1b2c-3d4e-5f60-7182-93a4b5c6d7e8</UserSecretsId>`,
        note: 'a pointer, and nothing else',
      },
      {
        caption: 'ON YOUR MACHINE',
        lang: 'text',
        dark: true,
        from: 'demos/d09-user-secrets/README.md — the directory the provider reports, plus the file name',
        body: `%APPDATA%\\Microsoft\\UserSecrets\\d09a1b2c-...\\secrets.json

{ "Weather:ApiKey": "dev-key-12345" }`,
        note: 'plaintext, on disk, unencrypted',
      },
    ],
    prose: 'drop'
  },

  23: { layout: 'code' },
  24: {
    layout: 'default',
    prose: 'keep',
    // the question is the blockquote in OUTLINE.md; a footnote copy rendered it twice
  },
  '24a': {
    layout: 'code',
    prose: 'keep',
  },

  // ---- options ---------------------------------------------------------------------
  25: { layout: 'section' },

  26: {
    layout: 'panels',
    panelCaptions: ["DON'T", 'DO'],
    prose: 'drop',
  },

  27: {
    layout: 'code',
    prose: 'drop',
  },
  28: {
    layout: 'default',
    prose: 'drop',
  },

  // 28a/28b are a before/after pair: same headline, same layout, so only the values move.
  '28a': {
    layout: 'reveal',
    footnote: '*now `appsettings.json` changes to 90 underneath the running app.*',
  },
  '28b': {
    layout: 'reveal', headline: 'Same three interfaces. One file edit.',
    footnote: 'The middle column matters: a snapshot already resolved in the current request stays at 30. It is scoped, so it only picks up the change in a **new scope** — normally the next request.',
  },

  29: {
    layout: 'default',
    footnote: 'The point is *where* this happened: at startup, before the process took traffic. Without `ValidateOnStart` it happens on first `.Value` access — in production, on the first request that reaches that code path.',
  },
  30: {
    layout: 'panels',
    panelCaptions: ['NAMED', 'GENERATED'],
    footnote: 'Enable generated binding in the project file: `<EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>`',
  },

  // ---- stage 2: deployment ---------------------------------------------------------
  31: { layout: 'section' },
  32: {
    layout: 'default',
    gold: true,
    footnote: '**`:` is not portable in an environment variable name. `__` is.**',
  },
  33: {
    layout: 'default',
    bigNum: { from: '4', to: '11' },
    prose: 'drop',
  },
  34: {
    layout: 'default',
    footnote: 'The path must be absolute. On Kubernetes the mounts are symlink swaps, so even the reloading overload may not fire — treat restart as the contract.',
  },
  // one key, four spellings - the SHAPE bug in one slide
  '34a': { layout: 'default', prose: 'keep' },

  35: {
    layout: 'default',
    footnote: '`--` becomes `:` because Key Vault forbids a colon in a secret name. No reload by default — `ReloadInterval` is null until you set it.',
  },
  37: { layout: 'code' },

  // ---- ACT 4 - production -----------------------------------------------------------
  '36a': { layout: 'section' },

  38: { layout: 'section' },
  39: {
    layout: 'default',
    prose: 'drop',
  },
  '39a': { layout: 'default', prose: 'keep' },
  '43a': { layout: 'code', prose: 'keep' },

  40: {
    layout: 'default',
    footnote: 'Changing an environment variable on a running container does nothing at all.',
  },

  // ---- feature flags ---------------------------------------------------------------
  41: { layout: 'section' },
  42: {
    // "Row one is highlighted." - OUTLINE.md slide 42
    markRow: 1,
    layout: 'default',
    prose: 'drop',
  },
  43: {
    layout: 'code',
    footnote: 'That is the entire setup. No cloud service, no cloud account, no network dependency — just a NuGet package and a JSON section.',
  },
  44: {
    layout: 'default',
    gold: true,
    footnote: '**“Same user, same session, checks this flag twice. Same answer both times?”**',
  },
  // same headline as its setup slide, so the reveal lands as a value change
  '44a': {
    layout: 'reveal', headline: '50% of users, or 50% of calls?',
    footnote: 'The nav bar says new checkout, the checkout page says old, and it is invisible in a single test run. The fix: targeting, or variant allocation with a `seed` — that is what gives a stable per-user assignment.',
  },
  45: {
    layout: 'code',
    footnote: 'A variant hands back an `IConfigurationSection`, so it binds like anything else in this talk.',
  },
  46: {
    layout: 'default',
    gold: true,
    footnote: '**`NewCheckout` never gets deleted. `Checkout_V2_Rollout_2026Q1` files its own expiry.**',
  },
  '46a': {
    // `code`, not `default`: on the default layout a blockquote renders ABOVE the content
    // (slide 16 needs its reading-direction label there), which put this slide's caveat
    // above the branch it is describing.
    layout: 'code',
  },
  47: {
    layout: 'statement',
    // "Beneath, smaller" - OUTLINE.md slide 47. One claim, then two supporting lines.
    lede: true,
    prose: 'drop',
  },

  // ---- ACT 5 - where should this value live? ---------------------------------------
  '47a': { layout: 'section' },

  48: {
    prose: 'drop',
    layout: 'default',
  },
  51: {
    layout: 'default',
    // The outline's "Three lines beneath, muted" — a markdown list, so it stays a list.
    footnote: [
      '- `SetBasePath(AppContext.BaseDirectory)` — the working directory of a double-clicked',
      '  EXE is not the install directory',
      '- `IConfiguration` has an indexer setter but **no persistence API** — nothing writes',
      '  back to the JSON. User preferences are a different problem',
      '- **No cloud secrets in a client binary.** Authenticate the user, call a backend',
    ].join('\n'),
  },
  52: {
    layout: 'default',
    footnote: 'All rows are **the JSON provider specifically** — providers that already carried real nulls, such as in-memory, did not all behave this way. d06’s own one-liner: *“Retries was 3 on .NET 9 and is null on .NET 10.”*',
  },

  // ---- close -----------------------------------------------------------------------
  53: { layout: 'section' },
  // ---- appendix - off the main path -------------------------------------------------
  '54a': { layout: 'section' },
  54: {
    layout: 'cover',
    variant: 'thanks',
    qr: '/repo-qr.svg',
    headline: "Let's keep talking.",
    lines: [
      SITE,
      'github.com/1kevgriff/modern-dotnet-configuration',
      'X · LinkedIn · GitHub — @1kevgriff',
      'Bluesky — @consultwithgriff.com',
    ],
    from: 'README.md §Speaker',
  },
}
