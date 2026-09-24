// One-shot: take the shout out of the speaker notes without losing what they say.
//
// The notes are the talk and they carry corrections verified against /demos, so this
// does NOT cut them. It changes register only: ALL-CAPS used for emphasis becomes
// lower case, shouty labels become sentence case, and a few stock phrases go.
//
// Domain terms are protected. ORDER / SHAPE / LIFETIME / TRUST are the four failure
// modes and a real motif; lower-casing them would break the deck.
//
//   node build/detic-notes.mjs          dry run - prints every change
//   node build/detic-notes.mjs --write  applies it

import { readFileSync, writeFileSync, copyFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import path from 'node:path'
import process from 'node:process'

const OUTLINE = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../../OUTLINE.md')
const NL = String.fromCharCode(10)

/** Never lower-case these: acronyms, APIs, and the failure-mode motif. */
const KEEP = new Set([
  'ORDER', 'SHAPE', 'LIFETIME', 'TRUST',
  'JSON', 'XML', 'INI', 'YAML', 'API', 'APIS', 'CLI', 'SDK', 'URL', 'URI', 'EXE', 'DLL',
  'HTTP', 'HTTPS', 'AOT', 'SRE', 'PR', 'QA', 'DI', 'IIS', 'AKS', 'ACA', 'VM', 'OS',
  'NET', 'ASP', 'ENV', 'ID', 'IDS', 'UI', 'IDE', 'TLS', 'DNS', 'KV', 'CI', 'CD',
  'F5', 'GUID', 'UTC', 'LTS', 'EOL', 'MVP', "DON'T", 'DONT', 'DO',
])

/** Stock phrases that read as generated. */
const PHRASES = [
  [/^THE LINE TO SAY:/gm, 'Say:'],
  [/^THE LINE:/gm, 'Say:'],
  [/\bTHE LINE TO SAY:/g, 'Say:'],
  [/^SAY IT PLAINLY:/gm, 'Say:'],
  [/^SAY IT LIKE THIS:/gm, 'Say:'],
  [/^SAY IT:/gm, 'Say:'],
  [/\bDO NOT\b/g, "Don't"],
  [/\bDo NOT\b/g, "Don't"],
  [/\bthat(?:'|&apos;)s the win condition for (?:this slide|the motif)\b/gi,
   'that is when this slide has done its job'],
  [/\bwin condition\b/gi, 'the point'],
  [/\bThe honest one-liner is\b/g, 'The one-liner is'],
  [/\bthe honest thing to cut\b/g, 'the first thing to cut'],
  [/\bSTRUCTURAL HONESTY\b/g, 'Worth naming'],
  [/\bNEVER CUT\b/g, 'never cut'],
]

let s = readFileSync(OUTLINE, 'utf8')
const before = s
const changes = []

for (const [re, to] of PHRASES) {
  const hits = s.match(re)
  if (hits) changes.push(`phrase ${re.source}  x${hits.length}`)
  s = s.replace(re, to)
}

// Shouty labels: a line that is ALL CAPS and ends in a colon.
s = s.replace(/^([A-Z][A-Z0-9 ,'"()/&;-]{6,}):$/gm, (m, label) => {
  if (/^(ON SCREEN|DO|SAY)$/.test(label)) { /* fall through to sentence case anyway */ }
  const words = label.split(' ')
  const cased = words.map((w, i) => {
    const bare = w.replace(/[^A-Z0-9]/g, '')
    if (KEEP.has(bare)) return w
    return i === 0
      ? w.charAt(0) + w.slice(1).toLowerCase()
      : w.toLowerCase()
  }).join(' ')
  if (cased !== label) changes.push(`label  ${label}  ->  ${cased}`)
  return `${cased}:`
})

// Inline emphasis caps: only function/filler words that can never be a domain term.
const EMPHASIS = [
  'THIS', 'THAT', 'THESE', 'THOSE', 'WHAT', 'WHEN', 'WHERE', 'WHICH', 'HERE', 'THERE',
  'FIRST', 'LAST', 'ALWAYS', 'NEVER', 'ONLY', 'BOTH', 'EVERY', 'EVERYTHING', 'NOTHING',
  'ANYTHING', 'SAME', 'REALLY', 'ACTUALLY', 'MUST', 'WILL', 'CANNOT', 'BEFORE', 'AFTER',
  'INSIDE', 'ACROSS', 'TWICE', 'BECAUSE', 'INSTEAD', 'ALREADY', 'STILL', 'ANY', 'NOT',
  'MORE', 'LESS', 'THAN', 'WITHOUT', 'THEIR', 'YOUR', 'SOMETHING', 'SOMEONE', 'ANYONE',
]
const emphasisRe = new RegExp(`\\b(${EMPHASIS.join('|')})\\b`, 'g')
let emph = 0
s = s.split(NL).map(line => {
  // leave headings, table rows and fenced markers alone
  if (/^\s*[#|`]/.test(line)) return line
  // leave a line that is ENTIRELY caps (a deliberate banner) alone
  const letters = line.replace(/[^A-Za-z]/g, '')
  if (letters && letters === letters.toUpperCase() && letters.length > 12) return line
  return line.replace(emphasisRe, w => { emph += 1; return w.toLowerCase() })
}).join(NL)
if (emph) changes.push(`emphasis caps lowered  x${emph}`)

if (!process.argv.includes('--write')) {
  console.log(`  dry run - ${changes.length} change groups${NL}`)
  for (const c of changes) console.log(`   ${c}`)
  const d = s.length - before.length
  console.log(`${NL}  notes length ${before.length} -> ${s.length} (${d >= 0 ? '+' : ''}${d})`)
  console.log(`${NL}  re-run with --write to apply`)
  process.exit(0)
}

// Content must not be lost, only re-cased: compare letter counts case-insensitively.
const norm = t => t.toLowerCase().replace(/[^a-z0-9]/g, '')
const lostChars = norm(before).length - norm(s).length
if (lostChars > 400) {
  console.error(`  REFUSING: ${lostChars} characters would disappear; this should re-case, not cut`)
  process.exit(1)
}

copyFileSync(OUTLINE, OUTLINE + '.bak')
writeFileSync(OUTLINE, s, 'utf8')
console.log(`  de-shouted the notes: ${changes.length} change groups, ${lostChars} chars removed`)
