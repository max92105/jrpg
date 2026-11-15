# NU2. The digit strips generated from Cormorant Garamond by a script in `art-src/`

Art + script, about 150 lines (no game code, no test code). Decision 653 (Q4); tunable "Digit strips". NU3 draws them.

## Goal
Two PNG strips of the digits 0-9, made by Claude with a one-off PowerShell script kept in `art-src/`, from the OFL serif
the game already ships (`content/art/fonts/titles/CormorantGaramond-SemiBold.ttf`): slanted, heavy, white fading to a
pale lavender-grey on the lower part, a thin dark edge baked in. The clip's look, never its glyphs or assets (decision
264). A normal strip and a critical strip 1.4x bigger, so a critical never scales the art (Q3).

## The spec
- **Files:** `content/art/fonts/DamageDigits.png` (normal) and `content/art/fonts/CriticalDigits.png` (critical), beside
  the game's fonts (`FontLibrary` reads only `.ttf` files there).
- **Layout:** ten cells side by side, "0" to "9" in that order, transparent background. Normal: cells 40 x 52 px, the
  image 400 x 52. Critical: cells 56 x 73 px, the image 560 x 73.
- **Each digit (normal; the critical scaled by 1.4):** ink 39 px tall, its bottom on row 45 of its cell, no wider than
  38 px; it needn't be centered, NU3 measures each digit's own columns. GDI+ can't choose Cormorant's lining figures, so
  the script scales each digit's ink to the 39 px height and sets it on the baseline whatever figure style the font's
  default glyphs have.
- **Style:** slanted about 10 degrees right (a shear transform); made heavy by stroking the glyph outline with its own
  fill (about 2 px) before filling; filled with a vertical gradient, white over the upper half fading to about
  `#C9C3DD` at the bottom; a dark edge about 2 px wide in about `#1A1830`, drawn under the fill (a wider stroke first);
  antialiased. White, so the game tints criticals, heals and Poison.

## Changes
- `art-src/digits/New-DigitStrips.ps1` (new, about 110 lines): loads the font through
  `System.Drawing.Text.PrivateFontCollection`, builds each digit as a `GraphicsPath`, measures, scales, shears, strokes
  and fills it as above, and writes both PNGs. Parameters at the top (cell sizes, heights, colors, slant, edge) with the
  spec's values. Run: `powershell -NoProfile -ExecutionPolicy Bypass -File art-src/digits/New-DigitStrips.ps1`.
  System.Drawing is used only by this tool, never by the game or the editor (ADR 0009: the game reads PNGs).
- `art-src/README.md`: a line for `digits/` (the script that makes the digit strips, run by hand when they change).
- `content/art/art-sources.json`: a source for the generated digits (its name "Damage digits rendered from Cormorant
  Garamond", the Cormorant authors, `OFL-1.1`, `licenseFile` `fonts/titles/OFL.txt`, a note naming the script) and the
  two files under it; `content/art/CREDITS.md`: its section, written by hand (ADR 0038).

## Tests
- No new test: `ArtSourcesTests` fails until both files are listed and passes once they are; the game reads them only
  from NU3 (with `UiArtFilesTests`' row).

## Verify by hand
1. Run the script: both PNGs are written; open them: ten slanted white-to-lavender digits with a dark edge each, the
   critical one bigger, nothing touching a cell's edge, transparent around.
2. Put a capture of the clip's number beside `DamageDigits.png`: the weight, slant and gradient read alike; adjust the
   script's parameters and run again until they do.
3. `powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/verify.ps1`: green (the art sources listed).

