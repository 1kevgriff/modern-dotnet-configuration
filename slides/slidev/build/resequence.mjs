// One-shot: re-sequence OUTLINE.md into the five-act developer journey.
//
// This is the ONE tool here that writes OUTLINE.md, and it is deliberately mechanical:
// it parses the file into slide blocks, reorders the list, and reassembles. It never
// rewrites a block's text. Invariants are asserted before anything is written, because
// make_outline.py destroyed this file once by regenerating over hand-edited content.
//
//   node build/resequence.mjs           dry run - prints the plan
//   node build/resequence.mjs --write   applies it

import { readFileSync, writeFileSync, copyFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import path from 'node:path'
import process from 'node:process'

const HERE = path.dirname(fileURLToPath(import.meta.url))
const OUTLINE = path.resolve(HERE, '../../../OUTLINE.md')
const NL = String.fromCharCode(10)

// ---------------------------------------------------------------------------------------
// the journey
// ---------------------------------------------------------------------------------------

const ORDER = [
  // opening — unchanged
  '1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11',

  // ACT 1 — shared baseline
  '11a', '12', '13', '21',

  // ACT 2 — my machine
  '19', '20', '22', '23', '24', '24a', '16', '17', '18', '14', '15',

  // ACT 3 — shared dev environment. 25 keeps its binding claim and opens the
  // binding/validation half of the act.
  '31', '32', '34', '35', '25', '26', '27', '29', '36',

  // ACT 4 — production
  '36a', '36b', '28', '28a', '28b', '40',
  '38', '39',
  '41', '42', '43', '44', '44a', '45', '46', '46a', '47',

  // ACT 5 — decision payoff
  '47a', '48', '49', '50', '51',

  // close, then appendix
  '53', '54',
  '54a', '33', '30', '37', '52',
]

/** Slides that did not exist before this pass. */
const NEW = {
  // Act 1 opener. The baseline is a team artefact, which is why it is checked in.
  '11a': {
    content: [
      '**Section divider.** Kicker `ACT 1 · THE SHARED BASELINE`, ' +
      'title **`appsettings.json` is a team decision, and it ships with the app.**. Navy ground.',
    ].join(NL),
    notes: [
      '[ACT 1] THE SHARED BASELINE',
      '60-min: keep, 30 seconds',
      '',
      'FIRST BOUNDARY. Everything from here to the end is one application moving:',
      'the baseline everyone shares -> my machine -> a shared dev server -> production.',
      '',
      'The baseline is the part everyone agrees on. It is checked in, it is reviewed,',
      'and it travels with the artifact. Nothing here is secret and nothing here is',
      'machine-specific - those come next, and they OVERRIDE this rather than replace it.',
      '',
      'THE QUESTION THIS ACT ANSWERS:',
      '  "Who owns this value, and what does everyone get by default?"',
      '',
      'Weather:TimeoutSeconds is the value to follow. It starts at 30 here and it will',
      'cross every boundary in the talk.',
    ].join(NL),
  },

  // Act 4 opener.
  '36a': {
    content: [
      '**Section divider.** Kicker `ACT 4 · PRODUCTION`, ' +
      'title **Production supplies the values. Nothing gets rebuilt to change one.**. Navy ground.',
    ].join(NL),
    notes: [
      '[ACT 4] PRODUCTION',
      '60-min: keep, 45 seconds',
      '',
      'LAST BOUNDARY, and the one with consequences. Same artifact as the shared dev',
      'server - genuinely the same bytes. What changed is who supplies the values and',
      'what it costs to get one wrong.',
      '',
      'THREE THINGS ARE TRUE HERE THAT WERE NOT TRUE ON YOUR LAPTOP:',
      '  OWNERSHIP  the person who needs to change a value may not be a developer',
      '  TRUST      a leaked value here is an incident, not an inconvenience',
      '  UPTIME     a restart is a change window, not a keystroke',
      '',
      'That third one is what the rest of this act is about. Say the line:',
      '"Nothing gets rebuilt to change a value. That is the whole point of the last',
      ' forty minutes."',
    ].join(NL),
  },

  // Act 4 bridge. Kevin: "Production adds a new question: can this value change
  // while the process is running?"
  '36b': {
    content: [
      '**Headline-only slide.**',
      '',
      '> Production adds a question the other environments never asked:',
      '> **can this value change while the process is running?**',
      '',
      'Binding and validation answered *is it there, and is it valid*. Lifetime answers',
      '*and does it still hold five minutes from now*.',
    ].join(NL),
    notes: [
      '[ACT 4] THE LIFETIME QUESTION',
      '60-min: keep - it is 20 seconds and it sets up the best beat in the talk',
      '',
      'On a laptop you restart the app without thinking. In production a restart is an',
      'event: dropped connections, a cold cache, a change window, maybe an approval.',
      '',
      'So production is the first place the question is worth asking at all:',
      '  "Can this value change WITHOUT stopping the process?"',
      '',
      'That question has exactly three answers in .NET, and they are the three interfaces',
      'on the next slide. Do not list them here - just land the question and advance.',
      '',
      'This is also the slide to point back to from What Actually Reloads. The interface',
      'decides whether your code SEES a change; the provider decides whether a change',
      'ever ARRIVES. Both have to line up.',
    ].join(NL),
  },

  // Act 5 opener.
  '47a': {
    content: [
      '**Section divider.** Kicker `ACT 5 · WHERE SHOULD THIS VALUE LIVE?`, ' +
      'title **Who owns it? Is it secret? Who needs it? How fast must it change?**. Navy ground.',
    ].join(NL),
    notes: [
      '[ACT 5] THE DECISION',
      '60-min: keep - this is what the room came for',
      '',
      'The journey is over. Now it pays out as a decision you can make on Monday.',
      '',
      'FOUR QUESTIONS, and they are the whole framework:',
      '  OWNERSHIP    who changes this - a developer, an SRE, a product manager?',
      '  SENSITIVITY  does it hurt if it leaks?',
      '  SCOPE        one app, or many?',
      '  CADENCE      per release, per environment, or while the app is serving traffic?',
      '',
      'Say plainly: you do not need stage three. Most apps never do. The next two slides',
      'are a sorting exercise, not a maturity model - nobody is behind for using JSON',
      'files and environment variables.',
    ].join(NL),
  },

  // Appendix opener.
  '54a': {
    content: [
      '**Section divider.** Kicker `APPENDIX`, ' +
      'title **Useful details we skipped.**. Navy ground.',
    ].join(NL),
    notes: [
      '[APPENDIX] OFF THE MAIN PATH',
      '60-min: never shown; these exist for Q&A and for the repo',
      '',
      'Do not walk these. They are here so the answer exists when someone asks, and so',
      'the PDF is complete for whoever reads it later.',
      '',
      '  connection-string prefixes          "we moved to Postgres and the name changed"',
      '  named options + source generators   "how do I bind the same shape twice?"',
      '  a custom provider                   "how would I read config from X?"',
      '  .NET 10 null preservation           "we upgraded and a default came back null"',
      '',
      'Each one is technically sound and each one interrupts the laptop -> shared dev ->',
      'production story, which is why they are back here.',
    ].join(NL),
  },
}

// ---------------------------------------------------------------------------------------

// The five NEW slides have been hand-edited since this migration ran, so a re-run would
// silently revert them to their templates. This tool has done its job; it needs --force.
if (!process.argv.includes('--force')) {
  console.error([
    '',
    '  REFUSING: this was a one-shot migration and OUTLINE.md has been hand-edited since.',
    '  Re-running would revert the act dividers and the lifetime bridge to their templates.',
    '  Edit OUTLINE.md directly. Pass --force only if you genuinely mean to re-sequence.',
    '',
  ].join(NL))
  process.exit(1)
}

const raw = readFileSync(OUTLINE, 'utf8')
const lines = raw.split(NL)

const heads = []
lines.forEach((l, n) => {
  const m = l.match(/^# Slide (\S+)\s*$/)
  if (m) heads.push({ id: m[1], line: n })
})

const preamble = lines.slice(0, heads[0].line).join(NL)
const blocks = new Map()
heads.forEach((h, i) => {
  const end = i + 1 < heads.length ? heads[i + 1].line : lines.length
  blocks.set(h.id, lines.slice(h.line, end).join(NL).replace(/\s+$/, ''))
})

// ---- invariants ------------------------------------------------------------------------
const problems = []
const existing = [...blocks.keys()]
const wanted = ORDER.filter(id => !NEW[id])

for (const id of wanted) if (!blocks.has(id)) problems.push(`ORDER wants slide ${id}, which OUTLINE.md does not have`)
for (const id of existing) if (!ORDER.includes(id)) problems.push(`slide ${id} exists but ORDER drops it - that would DELETE content`)
const dupes = ORDER.filter((id, i) => ORDER.indexOf(id) !== i)
if (dupes.length) problems.push(`ORDER repeats: ${dupes.join(', ')}`)

if (problems.length) {
  console.error(NL + '  REFUSING to re-sequence:' + NL)
  for (const p of problems) console.error(`   - ${p}`)
  process.exit(1)
}

// ---- assemble --------------------------------------------------------------------------
const out = [preamble.replace(/\s+$/, ''), '']
for (const id of ORDER) {
  if (NEW[id]) {
    out.push([
      `# Slide ${id}`, '',
      '## Slide Content', '',
      NEW[id].content, '',
      '## Notes', '',
      '```text', NEW[id].notes, '```', '',
      '---', '',
    ].join(NL))
  } else {
    out.push(blocks.get(id) + NL)
  }
}
const doc = out.join(NL).replace(/\n{4,}/g, NL + NL + NL) + NL

// ---- prove nothing was lost --------------------------------------------------------------
for (const [id, body] of blocks) {
  // Slides in NEW are regenerated from their template on every run, so their old text is
  // expected to change. Every OTHER slide must survive byte-identical.
  if (NEW[id]) continue
  if (!doc.includes(body)) {
    console.error(`  REFUSING: slide ${id}'s text did not survive reassembly`)
    process.exit(1)
  }
}
const after = (doc.match(/^# Slide /gm) || []).length
if (after !== ORDER.length) {
  console.error(`  REFUSING: expected ${ORDER.length} slides, assembled ${after}`)
  process.exit(1)
}

if (!process.argv.includes('--write')) {
  console.log(`  dry run - ${existing.length} existing + ${Object.keys(NEW).length} new = ${after} slides`)
  console.log(`  new order:${NL}    ${ORDER.join(' ')}`)
  console.log(`${NL}  re-run with --write to apply`)
  process.exit(0)
}

copyFileSync(OUTLINE, OUTLINE + '.bak')
writeFileSync(OUTLINE, doc, 'utf8')
console.log(`  re-sequenced OUTLINE.md into ${after} slides (backup at OUTLINE.md.bak)`)
