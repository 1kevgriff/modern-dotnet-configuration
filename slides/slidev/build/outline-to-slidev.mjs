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

/**
 * Pick a column count and code size so panel code FITS its column.
 *
 * Slide 26's two snippets are 71 characters wide; side by side that needs a ~10px font, which
 * is unreadable from the back of a room, and the text simply spilled outside the panel. So the
 * widest line decides: try the requested columns, and if the font that would need is below the
 * legibility floor, stack instead.
 */
function fitPanels(list, wanted) {
  const maxLen = Math.max(...list.map(p => Math.max(...String(p.body).split(NL).map(l => l.length))), 1)

  // Sizes are in Slidev's 980px logical canvas, which is scaled up to the projector — so the
  // floor is about size RELATIVE to the slide, not absolute pixels. 10.5/980 ≈ 1.07% of slide
  // width, which is ~21px on a 1080p projector.
  const SLIDE = 980, GUTTER = 64, GAP = 14
  const ADVANCE = 0.6        // JetBrains Mono advance width, in em
  const PADDING = 28         // panel's own left+right padding, px
  const MIN = 10.5, MAX = 15 // legibility floor and a sane ceiling, px

  const sizeFor = cols => {
    const col = (SLIDE - GUTTER - GAP * (cols - 1)) / cols - PADDING
    return col / maxLen / ADVANCE
  }

  let cols = wanted
  if (cols > 1 && sizeFor(cols) < MIN) cols = 1
  const size = Math.min(MAX, Math.max(MIN, sizeFor(cols)))
  return { cols, size: size.toFixed(1) }
}

/** Longest line across a slide's code blocks. */
const widestLine = s =>
  Math.max(1, ...s.code.map(b => Math.max(...b.body.split(NL).map(l => l.length))))

/**
 * Font size for a full-width code block, so a long line is never clipped.
 * Code panels set `overflow: hidden`, so "too wide" would mean silently losing characters.
 */
function fitCode(s, availablePx, max) {
  const size = availablePx / widestLine(s) / 0.6
  return Math.min(max, Math.max(10.5, size)).toFixed(1)
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
const heading = s => (s.headline ? `# ${s.headline}` : '')

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
      front: { layout: 'code', codeSize: fitCode(s, 820, 17) },
      body: chunks(outlineBlocks(s), goldCaption(s), footnote(s, cfg)),
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

  section(s) {
    return {
      front: { layout: 'section', kicker: s.kicker || '' },
      body: `# ${s.dividerTitle}`,
    }
  },

  statement(s, cfg) {
    return { front: { layout: 'statement' }, body: chunks(s.quote, footnote(s, cfg)) }
  },

  roadmap(s, cfg) {
    const items = cfg.items.map((t, i) => `${i + 1}. ${t}`).join(NL)
    return {
      front: { layout: 'roadmap', panelTitle: cfg.panelTitle },
      body: chunks(provenance(cfg.from), items),
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
    const { cols, size } = fitPanels(list, cfg.cols ?? list.length)
    return {
      front: { layout: 'panels' },
      body: chunks(
        heading(s),
        `<PanelRow :cols="${cols}" size="${size}">`,
        list.map(panelBlock).join(NL + NL),
        '</PanelRow>',
        cfg.arrow ? '<Arrow />' : '',
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
      front: { layout: 'reveal', codeSize: fitCode(s, 820, 17) },
      body: chunks(head ? `# ${head}` : '', outlineBlocks(s), goldCaption(s), footnote(s, cfg)),
    }
  },

  default(s, cfg) {
    return {
      front: { layout: 'default', codeSize: fitCode(s, 844, 15) },
      body: chunks(
        heading(s),
        cfg.bigNum ? `<BigNum from="${esc(cfg.bigNum.from)}" to="${esc(cfg.bigNum.to)}" />` : '',
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
  'fonts:',
  '  sans: Manrope',
  '  mono: JetBrains Mono',
  "  weights: '400,600,800'",
  '  provider: google',
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
