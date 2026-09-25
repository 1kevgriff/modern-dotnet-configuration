// Parses OUTLINE.md into structured slide records.
//
// OUTLINE.md is UPSTREAM and READ-ONLY. Nothing in this directory ever writes to it.
// (make_outline.py destroyed it once by regenerating over hand-edited content.)

import { readFileSync } from 'node:fs'
import { pathToFileURL } from 'node:url'

/** Split a slide's raw markdown into fenced blocks and prose, preserving order. */
function splitBlocks(md) {
  const out = []
  const fence = /^```(\S*)[ \t]*$/
  const lines = md.split('\n')
  let prose = []
  let i = 0

  const flushProse = () => {
    const text = prose.join('\n').trim()
    if (text) out.push({ type: 'prose', text })
    prose = []
  }

  while (i < lines.length) {
    const m = lines[i].match(fence)
    if (!m) {
      prose.push(lines[i])
      i += 1
      continue
    }
    flushProse()
    const lang = m[1] || 'text'
    const body = []
    i += 1
    while (i < lines.length && !/^```\s*$/.test(lines[i])) {
      body.push(lines[i])
      i += 1
    }
    i += 1 // closing fence
    out.push({ type: 'code', lang, body: body.join('\n') })
  }
  flushProse()
  return out
}

/** Pull contiguous markdown tables out of a prose chunk, keeping order. */
function splitTables(text) {
  const out = []
  const lines = text.split('\n')
  let buf = []
  let table = []

  const isRow = l => /^\s*\|.*\|\s*$/.test(l)
  const flushBuf = () => {
    const t = buf.join('\n').trim()
    if (t) out.push({ type: 'prose', text: t })
    buf = []
  }
  const flushTable = () => {
    if (table.length) out.push({ type: 'table', body: table.join('\n') })
    table = []
  }

  for (const l of lines) {
    if (isRow(l)) {
      if (!table.length) flushBuf()
      table.push(l.trim())
    } else {
      flushTable()
      buf.push(l)
    }
  }
  flushTable()
  flushBuf()
  return out
}

const RE = {
  slide: /^# Slide (\S+)\s*$/,
  headline: /^\*\*Headline:\*\*\s*(.+?)\s*$/m,
  headlineOnly: /^\*\*Headline-only slide\.\*\*/m,
  divider: /^\*\*Section divider\.\*\*\s*Kicker\s*`([^`]*)`,\s*title\s*\*\*(.+?)\*\*/m,
  fullBleed: /^\*\*Full-bleed code slide\.\*\*/m,
  reveal: /^\*\*Reveal\.\*\*/m,
  marker: /Gold marker on line\(s\)\s*(\d+)(?:[–-](\d+))?/,
  badge: /Failure-mode badges?, top right:\s*(`[A-Z]+`(?:\s*and\s*`[A-Z]+`)?)/,
  goldCaption: /^Caption,\s*gold:\s*\*\*(.+?)\*\*\s*$/m,
  blockquote: /^>\s?(.*)$/,
}

/** Paragraph openers the generator already consumes, so they are not "leftover" prose. */
const DIRECTIVE = [
  /^\*\*Headline:\*\*/,
  /^\*\*Headline-only slide\.\*\*/,
  /^\*\*Section divider\.\*\*/,
  /^\*\*Full-bleed code slide\.\*\*/,
  /^\*\*Reveal\.\*\*/,
  /^\*\*Setup\.\*\*/,
  /^\*\*Setup \/ reveal/,
  /^\*\*Title slide\.\*\*/,
  /^\*\*About slide\.\*\*/,
  /^\*\*Thanks slide\.\*\*/,
  /^\*\*Roadmap\.\*\*/,
  /^Failure-mode badges?, top right:/,
  /^Gold marker on line/,
  /^Caption, gold:/,
  /^>/,
]

export function parseOutline(path) {
  const raw = readFileSync(path, 'utf8')
  const lines = raw.split('\n')

  // Index every "# Slide N" heading. Ids are strings: 24a and 28b are real slides.
  const heads = []
  lines.forEach((l, n) => {
    const m = l.match(RE.slide)
    if (m) heads.push({ id: m[1], line: n })
  })
  if (!heads.length) throw new Error(`no "# Slide N" headings found in ${path}`)

  return heads.map((h, idx) => {
    const end = idx + 1 < heads.length ? heads[idx + 1].line : lines.length
    const body = lines.slice(h.line + 1, end).join('\n')

    // Content runs to "## Notes"; notes are the first fenced block after it.
    const notesAt = body.indexOf('\n## Notes')
    const contentMd = (notesAt === -1 ? body : body.slice(0, notesAt))
      .replace(/^\s*## Slide Content\s*/m, '')
      .replace(/\n---\s*$/, '')
      .trim()
    const notesMd = notesAt === -1 ? '' : body.slice(notesAt)

    const notesBlocks = splitBlocks(notesMd).filter(b => b.type === 'code')
    const notes = notesBlocks.length ? notesBlocks[0].body.trimEnd() : ''

    // Flatten content into ordered blocks: code fences, tables, prose.
    const blocks = splitBlocks(contentMd).flatMap(b =>
      b.type === 'code' ? [b] : splitTables(b.text))

    const prose = blocks.filter(b => b.type === 'prose').map(b => b.text).join('\n\n')

    const marker = prose.match(RE.marker)
    const badgeRaw = prose.match(RE.badge)
    const divider = prose.match(RE.divider)
    const headline = prose.match(RE.headline)
    const goldCaption = prose.match(RE.goldCaption)

    // A blockquote is AUTHORED ON-SCREEN TEXT wherever it appears - not only on a
    // headline-only slide. Slide 10 carries the journey map this way and slide 14 its
    // security caveat; gating this on headlineOnly dropped both silently.
    //
    // Each ">" line is a DELIBERATE line (slide 9: "Two lines, stacked."). A line WITHOUT
    // ">" is markdown lazy continuation and folds into the one above - slide 8 relies on
    // that, and filtering on ">" alone ate the second half of its sentence:
    //
    //     > We traded a build-time decision
    //     for a runtime one.
    //
    // A blank line ends the quote, or the commentary after it gets swallowed onto the slide.
    const quote = /^>/m.test(prose)
      ? (() => {
          const lines = []
          let open = false
          for (const l of prose.split('\n')) {
            if (RE.blockquote.test(l)) {
              const text = l.match(RE.blockquote)[1].trim()
              if (text) { lines.push(text); open = true }
            } else if (!l.trim()) {
              open = false
            } else if (open) {
              lines[lines.length - 1] += ' ' + l.trim()   // lazy continuation
            }
          }
          return lines.join('\n\n')
        })()
      : ''

    // Prose left over once every line the generator already understands is removed.
    // Whatever survives here is either stage direction or real on-screen content, and
    // nothing can tell those apart automatically — the sidecar has to decide per slide.
    const residualProse = prose
      .split(/\n\s*\n/)
      .map(p => p.trim())
      .filter(p => p && !DIRECTIVE.some(re => re.test(p)))

    return {
      id: h.id,
      blocks,
      prose,
      residualProse,
      notes,
      headline: headline ? headline[1] : '',
      quote,
      kicker: divider ? divider[1] : null,
      dividerTitle: divider ? divider[2] : null,
      isDivider: Boolean(divider),
      isFullBleed: RE.fullBleed.test(prose),
      isReveal: RE.reveal.test(prose),
      isHeadlineOnly: RE.headlineOnly.test(prose),
      goldCaption: goldCaption ? goldCaption[1] : '',
      // "line(s) 6–7" -> "{6-7}"; a single line -> "{6}"
      lineMarker: marker ? (marker[2] ? `${marker[1]}-${marker[2]}` : marker[1]) : null,
      badges: badgeRaw ? [...badgeRaw[1].matchAll(/`([A-Z]+)`/g)].map(m => m[1]) : [],
      code: blocks.filter(b => b.type === 'code'),
      tables: blocks.filter(b => b.type === 'table'),
    }
  })
}

// `node build/parse-outline.mjs` prints what the parser sees, for eyeballing.
if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  const slides = parseOutline(new URL('../../../OUTLINE.md', import.meta.url))
  for (const s of slides) {
    const tags = [
      s.isDivider && `divider(${s.kicker}|${s.dividerTitle})`,
      s.isFullBleed && 'fullbleed',
      s.isReveal && 'reveal',
      s.isHeadlineOnly && 'headline-only',
      s.lineMarker && `mark{${s.lineMarker}}`,
      s.badges.length && `badge:${s.badges.join('+')}`,
      s.goldCaption && 'goldcaption',
    ].filter(Boolean).join(' ')
    console.log(
      `${s.id.padEnd(4)} code:${String(s.code.length).padEnd(2)} tbl:${String(s.tables.length).padEnd(2)} ` +
      `notes:${String(s.notes.length).padStart(5)}  ${(s.headline || s.quote.split('\n')[0] || '').slice(0, 44).padEnd(46)}${tags}`)
  }
  console.log(`\n${slides.length} slides`)
}
