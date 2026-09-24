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
  1: { layout: 'code' },
  2: { layout: 'code' },

  3: {
    layout: 'cover',
    variant: 'title',
    headline: 'Modern .NET Configuration',
    kicker: 'CONFIGURATION · OPTIONS · FEATURE FLAGS',
    lines: ['Kevin Griffin', 'Microsoft MVP', '.NET 10 / C# 14'],
    from: 'OUTLINE.md slide 3 byline',
    prose: 'drop'
  },

  4: {
    layout: 'cover',
    variant: 'bio',
    headline: 'Kevin Griffin',
    lines: [
      'Software consultant — .NET and Azure',
      'Microsoft MVP',
      'Builds and runs Shows On Sale',
      SITE,
    ],
    from: 'OUTLINE.md slide 4 + README.md §Speaker',
  },

  5: {
    layout: 'roadmap',
    panelTitle: "What we'll cover",
    items: [
      'The provider chain and precedence',
      'Local dev → deployment → shared',
      'Binding, options, and validation',
      'Feature flags and flag debt',
      'Choosing a strategy',
    ],
    from: 'OUTLINE.md slide 5',
    prose: 'drop'
  },

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
    footnote: '04 and 05 are why this talk exists in 2026.',
    from: 'OUTLINE.md slide 6 — card titles inline, emphasis as described',
  },

  7: { layout: 'code' },
  8: { layout: 'statement' },
  9: {
    layout: 'statement',
    prose: 'drop',
  },

  // ---- the journey -----------------------------------------------------------------
  10: {
    layout: 'section',
    footnote: 'Each boundary changes *who supplies the value* and *what it costs to get it wrong*. The application does not change — only the answer to “where did this come from?” does.',
  },

  11: {
    layout: 'cards',
    cols: 2,
    cards: [
      { n: '01', title: 'ORDER', body: 'A provider you forgot about is winning.' },
      { n: '02', title: 'SHAPE', body: "The key you set doesn't produce the key the binder looks for." },
      { n: '03', title: 'LIFETIME', body: 'You cached a value that was supposed to change — or vice versa.' },
      { n: '04', title: 'TRUST', body: 'A secret is sitting in a file that ships with the app.' },
    ],
    from: 'OUTLINE.md slide 11 ## Notes — the four numbered definitions, verbatim',
    prose: 'drop'
  },

  // ---- ACT 1 - the shared baseline --------------------------------------------------
  '11a': { layout: 'section' },

  12: {
    layout: 'panels',
    arrow: true,
    prose: 'drop',   // the prose only describes the two panels, which the panels now are
    panels: [
      {
        caption: 'WHAT YOU WROTE',
        lang: 'json',
        from: 'demos/d01-provider-dump/appsettings.json',
        body: `{
  "Weather": {
    "ApiBaseUrl": "https://api.example.com",
    "TimeoutSeconds": 30,
    "ApiKey": "placeholder-set-a-real-one-with-user-secrets"
  },
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Demo;..."
  }
}`,
      },
      {
        caption: 'WHAT ACTUALLY EXISTS',
        lang: 'text',
        dark: true,
        from: 'the flat projection of the same file — ":" delimiter, values are strings',
        body: `Weather:ApiBaseUrl         "https://api.example.com"
Weather:TimeoutSeconds     "30"
Weather:ApiKey             "placeholder-set-a-real-..."
ConnectionStrings:Default  "Server=localhost;Database=..."`,
      },
    ],
  },

  13: {
    layout: 'statement',
    prose: 'drop',
  },
  14: { layout: 'code', prose: 'drop' },
  15: { layout: 'code' },
  16: {
    layout: 'default',
    prose: 'drop',   // the reading-direction label is the blockquote, rendered above the table
    // "row 1 beats row 8" is now the gold label above the table, so this keeps only the
    // fact that label does not carry.
    footnote: 'Rows 5 and 6 are the ones almost nobody knows exist — and they apply to web apps too.',
  },
  17: { layout: 'code' },
  18: {
    // "The last row is bold navy with a large gold value." - OUTLINE.md slide 18
    markRow: 4,
    markValue: true,
    layout: 'default',
    footnote: '*last one wins*',
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
        from: 'demos/d01-provider-dump/appsettings.Development.json',
        body: `"Weather": {
  "ApiBaseUrl": "https://localhost:7104"
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
TimeoutSeconds   30           <- survived
ApiKey           placeholder  <- survived`,
      },
    ],
    footnote: 'The surviving rows are the whole point. It merges key by key — it does not replace.',
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
    gold: true,
    footnote: '**“Which one wins?”**',
  },
  '24a': {
    layout: 'code',
    footnote: 'So the value you carefully set is beaten locally by a file that will not exist in production — where your environment variable is the only thing left.',
  },

  // ---- options ---------------------------------------------------------------------
  25: { layout: 'section' },

  26: {
    layout: 'panels',
    panelCaptions: ["DON'T", 'DO'],
    gold: true,
    footnote: 'That `!` is anti-pattern #3 on its own.',
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
    footnote: 'Plus one line, not a panel: `<EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>`',
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
  35: {
    layout: 'default',
    footnote: '`--` becomes `:` because Key Vault forbids a colon in a secret name. No reload by default — `ReloadInterval` is null until you set it.',
  },
  36: { layout: 'code' },
  37: { layout: 'code' },

  // ---- ACT 4 - production -----------------------------------------------------------
  '36a': { layout: 'section' },
  '36b': { layout: 'statement', prose: 'keep' },

  38: { layout: 'section' },
  39: {
    layout: 'default',
    prose: 'drop',
  },
  40: {
    layout: 'default',
    footnote: 'The **No** rows are the ones that matter: changing an environment variable on a running container does nothing at all.',
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
    footnote: 'The last line is the whole point: a variant hands you a configuration section, so you bind it like anything else in this talk.',
  },
  46: {
    layout: 'default',
    gold: true,
    footnote: '**`NewCheckout` never gets deleted. `Checkout_V2_Rollout_2026Q1` files its own expiry.**',
  },
  '46a': {
    layout: 'default',
    footnote: 'Turning it off in the portal leaves the dead branch compiling forever. *Ten live flags is up to 1,024 nominal combinations. You test three.*',
  },
  47: {
    layout: 'statement',
    prose: 'drop',
  },

  // ---- ACT 5 - where should this value live? ---------------------------------------
  '47a': { layout: 'section' },

  48: {
    layout: 'default',
    gold: true,
    footnote: '**If a value is in the wrong bucket, no amount of provider tuning fixes it.**',
  },
  49: {
    layout: 'default',
    prose: 'drop',
  },
  50: {
    layout: 'default',
    prose: 'drop',
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
