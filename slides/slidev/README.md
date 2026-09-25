# Slidev deck — Modern .NET Configuration

The presentable deck. 59 slides, built from `OUTLINE.md`.

```bash
npm install
npm run dev        # present at http://localhost:3030 (press `p` for presenter mode)
npm run verify     # generate + build + prove no slide clips its content
```

`dev`, `build` and every `export` run `generate` first, so `slides.md` can never be stale.

## OUTLINE.md is the source of truth

`slides.md` is **generated**. Editing it directly means losing your change on the next build.

```text
../../OUTLINE.md ──┐
                   ├──► build/outline-to-slidev.mjs ──► slides.md
build/slide-map.mjs┘
```

| File | What it owns |
| --- | --- |
| `../../OUTLINE.md` | headlines, code blocks, tables, speaker notes — **never written to by anything here** |
| `build/parse-outline.mjs` | turns the outline into structured slide records |
| `build/slide-map.mjs` | per-slide layout, and content the outline only *describes* |
| `build/outline-to-slidev.mjs` | merges the two and writes `slides.md` |

```bash
npm run generate   # OUTLINE.md + slide-map.mjs -> slides.md
```

### Why there is a sidecar at all

For most slides the outline carries literal content the generator lifts verbatim. For a handful
it carries only a *description* of a visual — slide 12 says "Left, captioned WHAT YOU WROTE:
nested JSON" and the JSON exists nowhere in the file. That content lives in `slide-map.mjs`,
and every authored block names its source in a `from:` field which is emitted into `slides.md`
as a comment. The values were lifted from `/demos` (d01, d03, d09), not invented.

### The generator refuses to build rather than quietly lose content

- a slide in `OUTLINE.md` with no `slide-map.mjs` entry → error, nothing written
- a `slide-map.mjs` entry with no matching slide → error, nothing written
- **a slide with leftover prose and no decision about it** → error, nothing written

That last one is the important one. The outline mixes stage direction ("Two panels, side by
side:") with real on-screen text ("Rows 5 and 6 are the ones almost nobody knows exist") in the
same paragraph, and nothing can separate them automatically. So each slide declares one of:

```js
footnote: '...'    // put this specific text on the slide
prose: 'keep'      // put the leftover prose on the slide verbatim
prose: 'drop'      // it was stage direction; the layout already expresses it
```

25 slides carry on-screen text; 15 are direction only.

## Editing

| You want to change | Edit |
| --- | --- |
| what a slide says, or the speaker notes | `../../OUTLINE.md`, then `npm run generate` |
| which layout a slide uses | `build/slide-map.mjs` |
| how a layout looks | `layouts/*.vue`, `components/*.vue`, `styles/index.css` |

Editing `OUTLINE.md` and re-running `generate` is the normal loop. `npm run dev` hot-reloads
`slides.md`, so keep `generate` running alongside it if you are iterating on content.

## Layouts

`code` (full-bleed) · `cover` (title/bio/thanks) · `section` (divider) · `statement`
(headline-only) · `panels` (captioned comparison) · `roadmap` · `reveal` (setup/reveal pairs)
· Slidev's `default` for headline + table.

Three setup/reveal pairs — 24→24a, 28a→28b, 44→44a — deliberately repeat their headline and
pin content to a fixed top, so advancing moves only the values.

Panel code is auto-sized: `fitPanels()` computes a font size from the widest line and falls
back from N columns to a single stacked column when the required size would be illegible.

## The overflow gate

`slidev build` succeeding proves very little — it will happily compile a slide whose code block
runs off the bottom of the screen, because panels set `overflow: hidden` and clip in silence.

```bash
npm run check      # renders all 59 slides and fails if anything overflows or clips
npm run verify     # build + check, the one command to run before you present
```

An earlier version of the fitting sized code on width alone and ten slides quietly lost their
bottom half, including the audience question that makes slide 44's reveal work. `fitPanels()`
and `fitCode()` now budget both dimensions, and `build/check-overflow.mjs` is what proves it.

## Fonts are self-hosted

`public/fonts/` holds Manrope and JetBrains Mono, declared in `styles/fonts.css`, with the
Slidev font provider set to `none`. A missing webfont changes every text metric and would
invalidate the fitting above, so the deck must not depend on conference wifi.

## Export

```bash
npm run export          # PDF
npm run export:png      # one PNG per slide
npm run export:pptx     # PowerPoint, native shapes and selectable text
```

Needs `playwright-chromium` (already a dev dependency).

All three pass `--wait 6000 --wait-until networkidle`, and that is not cosmetic: at the
old `--wait 1500` the PPTX export captured the "Loading slide…" placeholder for **all 64
slides** and still produced a valid-looking file. Always check the output, not the exit
code. Speaker notes do survive the PPTX export — all 64 of them.

## Known gaps

- **Repo QR code.** The outline asks for one on slides 3 and 54. Not added — generate it, drop
  it in `public/`, then reference it from `slide-map.mjs`.
- **Emphasis that is described but not rendered.** The outline asks for gold on the values that
  changed in slide 28b and on the environment-supplied values in slide 20. Both are mid-line,
  which Shiki cannot mark without a custom transformer; the surrounding text carries the point
  instead. Whole-row and whole-line emphasis *is* implemented (`markRow`, and Shiki line
  markers from "Gold marker on line(s) N").
- **Stray slide numbers in `OUTLINE.md`.** A few speaker notes end with a PowerPoint slide
  number glued to the sentence (`production."3`, `last slide.4`, `it."54`, and `until S2.1.5`).
  They came from the original `.pptx` extraction. `npm run generate` reports them; they need
  fixing in `OUTLINE.md`, which this tooling never writes to.
