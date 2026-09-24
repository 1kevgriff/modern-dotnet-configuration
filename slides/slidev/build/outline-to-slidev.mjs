// OUTLINE.md  ->  slides.md
//
// ONE DIRECTION ONLY. OUTLINE.md is upstream and is never written to by anything in this
// directory; make_outline.py destroyed it once by regenerating over hand-edited content, and
// this generator is built so that cannot happen again.
//
//   node build/outline-to-slidev.mjs      (or: npm run generate)
//
// Fails loudly rather than writing a short or wrong deck:
//   - a slide in OUTLINE.md with no slide-map entry     -> error, nothing written
//   - a slide-map entry with no matching outline slide  -> error, nothing written
//   - any write target outside slides/slidev/           -> error, nothing written

import { writeFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import path from 'node:path'
import process from 'node:process'
import { parseOutline } from './parse-outline.mjs'
import { map } from './slide-map.mjs'

const HERE = path.dirname(fileURLToPath(import.meta.url))
const PROJECT = path.resolve(HERE, '..')              // slides/slidev
const OUTLINE = path.resolve(PROJECT, '../../OUTLINE.md')
const OUT = path.join(PROJECT, 'slides.md')

const NL = String.fromCharCode(10)

/** Refuse to write anywhere but inside slides/slidev/. */
function guardedWrite(target, body) {
  const rel = path.relative(PROJECT, target)
  if (rel.startsWith('..') || path.isAbsolute(rel))
    throw new Error(`REFUSING to write outside the deck directory: ${target}`)
  writeFileSync(target, body, 'utf8')
}

// ---------------------------------------------------------------------------------------
// emitters
// ---------------------------------------------------------------------------------------

const esc = s => String(s).replace(/"/g, '&quot;')

/**
 * Join chunks with a BLANK LINE between them, dropping empties.
 *
 * The blank line is load-bearing: markdown nested inside a Vue component tag is only parsed
 * as markdown when it is blank-line separated. Without it Vue's SFC parser reads the fence
 * body as raw HTML and dies on the first generic - `Configure<EndpointOptions>` reads as an
 * unclosed element.
 */
const chunks = (...xs) => xs.flat().filter(x => x && String(x).trim()).join(NL + NL)

/**
 * Collapse runs of blank lines, but never inside a fenced code block - slide 1's cold open
 * depends on a blank line between the JSON and the `$ dotnet run` output.
 */
function tidy(md) {
  const out = []
  let inFence = false
  for (const l of md.split(NL)) {
    if (/^```/.test(l)) inFence = !inFence
    if (!inFence && l.trim() === '' && out.length && out[out.length - 1].trim() === '') continue
    out.push(l)
  }
  return out.join(NL).trim()
}

/** A fenced code block, with Shiki line markers when the outline asked for one. */
function fence(block, marker) {
  const meta = marker ? ` {${marker}}` : ''
  return '```' + block.lang + meta + NL + block.body + NL + '```'
}

const provenance = from => (from ? `<!-- from: ${from} -->` : '')

/**
 * Speaker notes in OUTLINE.md still carry XML entities from the original PowerPoint
 * extraction (&quot;, &apos;, &gt;). Decode them so presenter mode shows real punctuation.
 * "--" is left alone: it is a closing comment delimiter inside an HTML comment.
 */
const decodeNotes = t => t
  .replace(/&quot;/g, '"')
  .replace(/&apos;/g, "'")
  .replace(/&gt;/g, '>')
  .replace(/&lt;/g, '<')
  .replace(/&amp;/g, '&')
  .replace(/--(?=>)/g, '- ')

function panelBlock(p) {
  const cls = p.dark ? ' dark' : ''
  return chunks(
    provenance(p.from),
    `<Panel caption="${esc(p.caption)}"${cls}>`,
    fence({ lang: p.lang, body: p.body }),
    p.note ? `<template #note>${p.note}</template>` : '',
    '</Panel>',
  )
}

/** Content blocks straight out of the outline, in their original order. */
function outlineBlocks(s) {
  return s.blocks
    .filter(b => b.type !== 'prose')
    .map((b, i) => (b.type === 'code' ? fence(b, i === 0 ? s.lineMarker : null) : b.body))
    .join(NL + NL)
}

// ---------------------------------------------------------------------------------------
// fitting
//
// Slidev's canvas is 980x552 logical px, scaled up to whatever it is projected onto. Code
// that does not fit is not wrapped - it is CLIPPED, silently, because panels set
// overflow:hidden. So both dimensions have to be computed, not hoped for.
//
// Width alone is not enough: an earlier version sized only on the widest line and ten slides
// lost their bottom half, including the audience question that makes slide 44's reveal work.
// build/check-overflow.mjs is the gate that proves these numbers are right.
// ---------------------------------------------------------------------------------------

const FRAME_H = 552
const ADVANCE = 0.6      // JetBrains Mono advance width, in em
const LEADING = 1.5      // .line line-height in styles/index.css
const MIN = 10.5         // legibility floor: ~1.1% of slide width, ~21px on a 1080p projector
const PRE_PAD_X = 28     // pre's left+right padding
const PRE_PAD_Y = 16     // pre's top+bottom padding
const PANEL_LABEL = 29   // the small uppercase caption above a panel, incl. margin
const GAP = 14           // gap between panels / stacked code blocks
const H1 = 52            // headline height incl. margin-bottom
const PAD_Y = 102        // layout padding: 3.2rem top + 3.2rem bottom
const CAPTION_LINE = 24   // 1rem at 1.45 line-height
const CAPTION_MARGIN = 12

const lineCount = body => String(body).split(NL).length
const longestLine = body => Math.max(1, ...String(body).split(NL).map(l => l.length))

/** Rough height of a caption, which wraps at roughly 95 characters on a full-width slide. */
const captionHeight = text =>
  text ? Math.ceil(String(text).length / 88) * CAPTION_LINE + CAPTION_MARGIN : 0

/** Largest font size at which `lines` of `maxLen` characters fit in the given box. */
const fitBox = (maxLen, lines, w, h, max) =>
  Math.max(MIN, Math.min(max, w / maxLen / ADVANCE, h / lines / LEADING))

/**
 * Pick a column count and code size so panel code fits its column BOTH ways.
 *
 * Stacking trades width for height, so the two options are scored against each other rather
 * than falling back blindly: whichever yields the larger legible size wins, preferring the
 * requested column count on a tie.
 */
function fitPanels(list, wanted, { heading, caption, arrow } = {}) {
  const maxLen = Math.max(...list.map(p => longestLine(p.body)))
  const availW = 980 - 64
  const availH = FRAME_H - PAD_Y - (heading ? H1 : 0) - captionHeight(caption)
  // the arrow sits in a widened gutter, which is width the panels no longer have
  const colGap = arrow ? 54 : GAP

  const score = cols => {
    const rows = Math.ceil(list.length / cols)
    const colW = (availW - colGap * (cols - 1)) / cols - PRE_PAD_X
    const rowH = (availH - GAP * (rows - 1)) / rows - PANEL_LABEL - PRE_PAD_Y
    // the tallest panel in any row has to fit that row
    const tallest = Math.max(...list.map(p => lineCount(p.body)))
    return fitBox(maxLen, tallest, colW, rowH, 15)
  }

  const a = score(wanted)
  const b = list.length > 1 ? score(1) : -1
  const cols = b > a + 0.25 ? 1 : wanted
  return { cols, size: score(cols).toFixed(1) }
}

/**
 * Font size for full-width code blocks on the code / reveal / default layouts.
 * `pad` is the layout's own vertical padding plus whatever sits above the code.
 */
function fitCode(s, { availW, pad, heading, caption, max }) {
  if (!s.code.length) return String(max)
  const maxLen = Math.max(...s.code.map(b => longestLine(b.body)))
  const lines = s.code.reduce((n, b) => n + lineCount(b.body), 0)
  const availH = FRAME_H - pad - (heading ? H1 : 0) - captionHeight(caption)
    - GAP * (s.code.length - 1) - PRE_PAD_Y * s.code.length
  return fitBox(maxLen, lines, availW - PRE_PAD_X, availH, max).toFixed(1)
}

/**
 * A caption block.
 *
 * The blank lines matter: inline `<Caption>text</Caption>` is raw HTML to markdown-it, so
 * backticks and ** inside it render LITERALLY. Slides 24a and 44a showed a bare ` character
 * on screen because of exactly that.
 *
 * Angle brackets inside a code span are escaped by markdown-it; outside one they would reach
 * Vue's parser as an element, so escape only those.
 */
function caption(text, gold) {
  if (!text) return ''
  const safe = String(text)
    .split(/(`[^`]*`)/)
    .map(part => (part.startsWith('`') ? part : part.replace(/</g, '&lt;')))
    .join('')
  return chunks(`<Caption${gold ? ' gold' : ''}>`, safe, '</Caption>')
}

const goldCaption = s => caption(s.goldCaption, true)

/** All caption text a slide will render, for the height budget. */
const captionText = (s, cfg) =>
  [s.quote, s.goldCaption, cfg.footnote ?? (cfg.prose === 'keep' ? s.residualProse.join(' ') : '')]
    .filter(Boolean).join(' ')
const heading = s => (s.headline ? `# ${s.headline}` : '')

/** A divider's sub-line: the journey map under slide 10's title. */
const quoteLine = s => (s.quote ? `<Caption>

${s.quote}

</Caption>` : '')

/**
 * On-screen text beneath the main visual.
 *
 * OUTLINE.md mixes stage direction ("Two panels, side by side:") with real slide content
 * ("Rows 5 and 6 are the ones almost nobody knows exist") in the same prose, and nothing can
 * separate them automatically. So the sidecar decides per slide, and main() REFUSES TO BUILD
 * if a slide has leftover prose and no decision — see checkProse(). Content cannot go missing
 * quietly; the build stops instead.
 */
function footnote(s, cfg) {
  return caption(
    cfg.footnote ?? (cfg.prose === 'keep' ? s.residualProse.join(NL + NL) : null),
    cfg.gold)
}

const LAYOUTS = {
  code(s, cfg) {
    return {
      front: { layout: 'code',
               codeSize: fitCode(s, { availW: 848, pad: 70, max: 17,
                                      heading: Boolean(s.headline),
                                      caption: captionText(s, cfg) }) },
      body: chunks(heading(s), outlineBlocks(s), caption(s.quote, true), goldCaption(s), footnote(s, cfg)),
    }
  },

  cover(s, cfg) {
    const front = { layout: 'cover', variant: cfg.variant, image: '/kevin-griffin.png' }
    if (cfg.kicker) front.kicker = cfg.kicker
    const lines = (cfg.lines || []).map(l => `- ${l}`).join(NL)
    return {
      front,
      body: chunks(provenance(cfg.from), `# ${cfg.headline}`, lines),
    }
  },

  section(s, cfg) {
    return {
      front: { layout: 'section', kicker: s.kicker || '' },
      body: chunks(`# ${s.dividerTitle}`, quoteLine(s), footnote(s, cfg)),
    }
  },

  statement(s, cfg) {
    return { front: { layout: 'statement' }, body: chunks(s.quote, footnote(s, cfg)) }
  },

  /**
   * The numbered list comes from OUTLINE.md, NOT the sidecar.
   *
   * It used to live in `cfg.items`, which meant the outline and the sidecar both held a
   * copy - so editing the outline changed nothing and the deck kept promising the old
   * agenda. The sidecar must never duplicate content the outline already owns.
   */
  roadmap(s, cfg) {
    const items = s.residualProse
      .flatMap(p => p.split(NL))
      .filter(l => /^\d+\.\s/.test(l.trim()))
      .map(l => l.trim())
    if (!items.length) throw new Error(`slide ${s.id}: roadmap has no numbered list in OUTLINE.md`)
    return {
      front: { layout: 'roadmap', panelTitle: cfg.panelTitle },
      body: chunks(items.join(NL), footnote(s, cfg)),
    }
  },

  cards(s, cfg) {
    const cards = cfg.cards
      .map(c =>
        `<Card n="${esc(c.n)}" title="${esc(c.title)}"${c.accent ? ' accent' : ''}>` +
        `${c.body ? esc(c.body) : ''}</Card>`)
      .join(NL)
    return {
      front: { layout: 'default' },
      body: chunks(
        heading(s),
        provenance(cfg.from),
        `<Cards :cols="${cfg.cols}">`,
        cards,
        '</Cards>',
        footnote(s, cfg),
      ),
    }
  },

  panels(s, cfg) {
    // Either authored panels from the sidecar, or the outline's own code blocks
    // paired with captions the sidecar supplies.
    const caps = cfg.panelCaptions || []
    const list = cfg.panels
      ?? s.code.map((b, i) => ({ caption: caps[i] ?? '', lang: b.lang, body: b.body }))
    const { cols, size } = fitPanels(list, cfg.cols ?? list.length, {
      heading: Boolean(s.headline),
      caption: captionText(s, cfg),
      arrow: Boolean(cfg.arrow),
    })
    return {
      front: { layout: 'panels' },
      body: chunks(
        heading(s),
        `<PanelRow :cols="${cols}" size="${size}"${cfg.arrow && cols > 1 ? ' arrow' : ''}>`,
        list.map(panelBlock).join(NL + NL),
        '</PanelRow>',
        footnote(s, cfg),
        goldCaption(s),
      ),
    }
  },

  // Setup/reveal pairs share this layout AND their headline, so that advancing the slide
  // moves only the values.
  reveal(s, cfg) {
    const head = cfg.headline || s.headline
    return {
      front: { layout: 'reveal',
               codeSize: fitCode(s, { availW: 916, pad: PAD_Y, max: 17,
                                      heading: Boolean(head),
                                      caption: captionText(s, cfg) }) },
      body: chunks(head ? `# ${head}` : '', outlineBlocks(s), goldCaption(s), footnote(s, cfg)),
    }
  },

  default(s, cfg) {
    // A long table plus a label plus a footnote does not fit at the default row height.
    // Counting the rows here beats shaving content off a reference slide people photograph.
    const rows = s.tables.length ? s.tables[0].body.split(NL).length - 2 : 0
    return {
      front: { layout: 'default',
               ...(rows >= 8 ? { class: 'table-dense' } : {}),
               // the outline calls out one row on some tables; the class drives the styling
               ...(cfg.markRow
                 ? { class: `${rows >= 8 ? 'table-dense ' : ''}mark-row-${cfg.markRow}` +
                            `${cfg.markValue ? ' mark-value' : ''}` }
                 : {}),
               codeSize: fitCode(s, { availW: 884, pad: PAD_Y, max: 15,
                                      heading: Boolean(s.headline),
                                      caption: captionText(s, cfg) }) },
      body: chunks(
        heading(s),
        cfg.bigNum ? `<BigNum from="${esc(cfg.bigNum.from)}" to="${esc(cfg.bigNum.to)}" />` : '',
        caption(s.quote, true),
        outlineBlocks(s),
        footnote(s, cfg),
        goldCaption(s),
      ),
    }
  },
}

// ---------------------------------------------------------------------------------------
// headmatter
// ---------------------------------------------------------------------------------------

const HEADMATTER = [
  'theme: default',
  'title: Modern .NET Configuration',
  'info: |',
  '  ## Modern .NET Configuration',
  '  Kevin Griffin - Microsoft MVP - .NET 10 / C# 14',
  '',
  '  Generated from OUTLINE.md. Regenerate with `npm run generate`.',
  'author: Kevin Griffin',
  'keywords: dotnet,configuration,options,feature-flags',
  'class: text-left',
  'highlighter: shiki',
  'lineNumbers: false',
  'drawings:',
  '  persist: false',
  'transition: none',
  'mdc: true',
  // provider:none - the fonts are self-hosted in public/fonts and declared in styles/fonts.css.
  // A conference machine with no network would otherwise fall back and change every metric.
  'fonts:',
  '  sans: Manrope',
  '  mono: JetBrains Mono',
  '  provider: none',
].join(NL)

const BANNER = [
  '<!--',
  '  GENERATED FILE - DO NOT HAND-EDIT.',
  '',
  '  Source of truth is ../../OUTLINE.md plus build/slide-map.mjs.',
  '  Regenerate with:  npm run generate',
  '',
  '  Editing this file directly means your change is lost on the next generate.',
  '-->',
].join(NL)

// ---------------------------------------------------------------------------------------
// main
// ---------------------------------------------------------------------------------------

function main() {
  const slides = parseOutline(OUTLINE)
  const seen = new Set()
  const problems = []
  const out = []

  for (const s of slides) {
    const cfg = map[s.id]
    if (!cfg) {
      problems.push(
        `slide ${s.id} ("${(s.headline || s.dividerTitle || '').slice(0, 40)}") has no slide-map entry`)
      continue
    }
    seen.add(String(s.id))

    // Leftover prose must be classified explicitly: footnote (put it on screen),
    // prose:'keep' (put the residue on screen verbatim), or prose:'drop' (it was
    // stage direction). No default — a silent drop is how content goes missing.
    if (s.residualProse.length && !cfg.footnote && !cfg.prose) {
      problems.push(
        `slide ${s.id} has unclassified prose - set footnote / prose:'keep' / prose:'drop':` +
        NL + s.residualProse
          .map(x => x.split(NL).map(l => '        | ' + l).join(NL))
          .join(NL))
      continue
    }

    const emit = LAYOUTS[cfg.layout]
    if (!emit) {
      problems.push(`slide ${s.id} asks for unknown layout "${cfg.layout}"`)
      continue
    }

    const { front, body } = emit(s, cfg)

    // Slide 1's frontmatter IS the deck headmatter - a separate leading block would render
    // as an extra, empty first slide.
    const first = out.length === 0
    const fm = Object.entries(front)
      .map(([k, v]) => (typeof v === 'number' ? `${k}: ${v}` : `${k}: ${JSON.stringify(String(v))}`))
      .join(NL)

    out.push(
      chunks(
        '---' + NL + (first ? HEADMATTER + NL + fm : fm) + NL + '---',
        first ? BANNER : '',
        `<!-- OUTLINE.md # Slide ${s.id} -->`,
        s.badges.map(b => `<Badge>${b}</Badge>`).join(NL),
        tidy(body),
        s.notes ? '<!--' + NL + decodeNotes(s.notes) + NL + '-->' : '',
      ),
    )
  }

  for (const id of Object.keys(map)) {
    if (!seen.has(String(id)))
      problems.push(`slide-map has an entry for "${id}" but OUTLINE.md has no such slide`)
  }

  if (problems.length) {
    console.error(NL + '  REFUSING to write slides.md:' + NL)
    for (const p of problems) console.error(`   - ${p}`)
    console.error('')
    process.exit(1)
  }

  guardedWrite(OUT, out.join(NL + NL) + NL)

  const counts = slides.reduce(
    (a, s) => ({
      code: a.code + s.code.length,
      tables: a.tables + s.tables.length,
      notes: a.notes + (s.notes ? 1 : 0),
    }),
    { code: 0, tables: 0, notes: 0 })
  const vals = Object.values(map)
  const authored = vals.filter(c => c.panels || c.cards || c.items || c.lines).length
  const kept = vals.filter(c => c.footnote || c.prose === 'keep').length
  const dropped = vals.filter(c => c.prose === 'drop').length

  console.log(`  wrote ${path.relative(process.cwd(), OUT)}`)
  console.log(
    `  ${slides.length} slides · ${counts.code} code blocks · ${counts.tables} tables · ` +
    `${counts.notes} notes · ${authored} slides with sidecar-authored content`)
  console.log(
    `  prose: ${kept} slides keep on-screen text, ${dropped} were stage direction only`)

  const missing = slides.filter(s => !s.notes).map(s => s.id)
  if (missing.length) console.log(`  slides without speaker notes: ${missing.join(', ')}`)

  // A sentence ending in punctuation immediately followed by 1-2 digits is a PowerPoint slide
  // number that got concatenated during the original extraction. It lives in OUTLINE.md, which
  // this tool never edits - so report it rather than guessing at a fix.
  const strays = slides.flatMap(s =>
    s.notes.split(NL)
      .filter(l => /(?:[.!?]|&quot;)[0-9]{1,2}$/.test(l) && !/\bS\d/.test(l.replace(/[0-9]{1,2}$/, '')))
      .map(l => `slide ${s.id}: ...${l.slice(-48)}`))
  if (strays.length) {
    console.log(`
  NOTE - ${strays.length} line(s) in OUTLINE.md look like a stray PowerPoint`)
    console.log('  slide number glued to the end of a sentence. Fix them in OUTLINE.md:')
    for (const x of strays) console.log(`    ${x}`)
  }
}

main()
