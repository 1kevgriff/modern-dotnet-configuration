// Renders the built deck and fails if any slide's content overflows its 980x552 frame.
//
// `slidev build` succeeding proves almost nothing: it happily compiles a slide whose code
// block runs off the bottom of the screen. This is the gate that actually catches that.
//
//   node build/check-overflow.mjs        (run against ./dist, so build first)

import http from 'node:http'
import fs from 'node:fs'
import { readFileSync } from 'node:fs'
import path from 'node:path'
import process from 'node:process'
import { fileURLToPath } from 'node:url'
import { chromium } from 'playwright-chromium'

const HERE = path.dirname(fileURLToPath(import.meta.url))
const ROOT = path.resolve(HERE, '../dist')

const TYPES = {
  '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css',
  '.png': 'image/png', '.svg': 'image/svg+xml', '.woff2': 'font/woff2',
  '.woff': 'font/woff', '.json': 'application/json', '.ttf': 'font/ttf',
}

if (!fs.existsSync(ROOT)) {
  console.error(`  no dist/ - run "npm run build" first`)
  process.exit(1)
}

const server = http.createServer((req, res) => {
  let p = new URL(req.url, 'http://x').pathname
  if (p === '/') p = '/index.html'
  let file = path.join(ROOT, decodeURIComponent(p))
  if (!fs.existsSync(file) || fs.statSync(file).isDirectory()) file = path.join(ROOT, 'index.html')
  res.writeHead(200, { 'content-type': TYPES[path.extname(file)] ?? 'application/octet-stream' })
  fs.createReadStream(file).pipe(res)
})

await new Promise(r => server.listen(0, r))
const base = `http://127.0.0.1:${server.address().port}`

const browser = await chromium.launch()
const page = await browser.newPage({ viewport: { width: 1280, height: 720 } })

// Derive the count from slides.md. A hardcoded default silently stopped checking once
// the deck grew past it, which is exactly when new slides are most likely to overflow.
const SLIDES = path.resolve(HERE, '../slides.md')
const total = Number(process.argv[2])
  || (readFileSync(SLIDES, 'utf8').match(/^<!-- OUTLINE\.md # Slide /gm) || []).length
const problems = []

for (let n = 1; n <= total; n++) {
  await page.goto(`${base}/${n}`, { waitUntil: 'networkidle' })
  await page.waitForTimeout(400)

  const bad = await page.evaluate(() => {
    // The built deck keeps every slide in the DOM; only one is on screen. Pick the one
    // whose box actually covers the middle of the viewport.
    const cx = window.innerWidth / 2
    const cy = window.innerHeight / 2
    const slide = [...document.querySelectorAll('.slidev-layout')].find(el => {
      const r = el.getBoundingClientRect()
      return r.width > 100 && r.height > 100 &&
             r.left <= cx && r.right >= cx && r.top <= cy && r.bottom >= cy
    })
    if (!slide) return null
    const frame = slide.getBoundingClientRect()
    const out = []
    // 2px of tolerance for sub-pixel rounding on borders and shadows.
    const TOL = 2
    for (const el of slide.querySelectorAll('pre, table, .panel, .cards, .caption, h1, li')) {
      const r = el.getBoundingClientRect()
      if (r.height === 0 || r.width === 0) continue
      const over = []
      if (r.bottom > frame.bottom + TOL) over.push(`bottom by ${Math.round(r.bottom - frame.bottom)}px`)
      if (r.right > frame.right + TOL) over.push(`right by ${Math.round(r.right - frame.right)}px`)
      if (r.top < frame.top - TOL) over.push(`top by ${Math.round(frame.top - r.top)}px`)
      if (over.length) {
        out.push(`${el.tagName.toLowerCase()}${el.className ? '.' + String(el.className).split(' ')[0] : ''}` +
                 ` overflows ${over.join(' and ')}`)
      }
      // a scrollable pre is content that is present but unreadable
      if (el.tagName === 'PRE' && el.scrollHeight > el.clientHeight + TOL)
        out.push(`pre clips ${el.scrollHeight - el.clientHeight}px of code vertically`)
      if (el.tagName === 'PRE' && el.scrollWidth > el.clientWidth + TOL)
        out.push(`pre clips ${el.scrollWidth - el.clientWidth}px of code horizontally`)
    }
    const id = document.querySelector('.slidev-page')?.dataset?.slideNo
    return { id, out: [...new Set(out)] }
  })

  if (bad?.out?.length) problems.push({ n, out: bad.out })
}

await browser.close()
server.close()

if (problems.length) {
  console.error(`\n  ${problems.length} slide(s) overflow or clip content:\n`)
  for (const p of problems) {
    console.error(`   slide ${p.n}`)
    for (const o of p.out) console.error(`      ${o}`)
  }
  console.error('')
  process.exit(1)
}
console.log(`  no overflow: ${total} slides fit their frame`)
