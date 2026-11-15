---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-14-battle-turn-presentation, slice NU3 (DigitStrip, DigitStripTests, FloatingNumbersView strips,
ArtFolders.Fonts, UiArtFiles.Fonts, SizeOf removed). 2026-11-14. Round 1. Verdict: ready to commit. Deviations accepted
by the Done note: strips measured on the first draw, private DigitStripImage, ArtFolders.Fonts constant only, SizeOf
tests removed.

- NU2 Criticals (Min(1.0), gradient spanning the heavy stroke) and Suggestions (line length, $glyphScale): fixed
- Warning: DigitRow.cs:4 and :20 still say "at the font's own size" / "width in the font"; advances now come from the
  digit strip. open
- Suggestion: FloatingNumbersView.cs:123 `Enumerable.Range(0, 10)` repeats DigitStrip's private DigitCount; let
  DigitStrip expose its advances. open
- Suggestion: DigitStripImage is a record holding a float[] (reference equality, never compared); a private sealed
  class says what it is. open
