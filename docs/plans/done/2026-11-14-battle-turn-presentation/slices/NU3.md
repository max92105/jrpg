# NU3. The digits drawn from the strips; a critical from its own bigger strip

Game, about 200 lines (about 90 of them tests). Needs NU1 and NU2. Decisions 652, 653; tunables "Digit strips",
"Critical".

## Goal
The numbers draw from NU2's strips: each digit its own cell's opaque columns, tight with the row's spacing, tinted by its
tone (white stays white; red-gold, green and purple tint the lavender part darker); a critical from
`CriticalDigits.png` at 1:1 (1.4x the normal size, never scaled). The strips are measured once when the battle screen
is built; a strip whose width isn't ten cells or with an empty cell stops the battle screen with the file named. The
"WEAK" and "RESISTED" cues keep the body font.

## New types
- `Game/Battle/DigitStrip` (internal sealed): each digit's columns in a digit strip image, measured once from its pixels.
  `static DigitStrip Measure(ReadOnlySpan<byte> alpha, Point imageSize, string file)` (throws
  `InvalidOperationException` naming `file` for a width not divisible by 10 or a cell with no opaque column);
  `Rectangle SourceOf(int digit)`; `float AdvanceOf(int digit)`; `int Height`.

## Changes
- `Game/Ui/UiArtFiles.Fonts` (`DamageDigits = "DamageDigits.png"`, `CriticalDigits = "CriticalDigits.png"`) and
  `Data/Art/ArtFolders.Fonts` (`"fonts"`, the folder `Rendering/FontLibrary` reads its TTFs from; check `ArtFoldersTests`
  for a list of every folder).
- `Game/Battle/FloatingNumbersView` (about 120 → 135): both strips and their `DigitStrip`s replace the 54 px font for the
  digits (alpha read once from each texture for `Measure`); `DigitRow.Of` takes the strip's advances; each digit drawn
  from `SourceOf` at its place, tinted `FloatingNumber.ColorOf(tone)`; a critical row from the critical strip. The 54 px
  constant and the font-measured digits go.
- `FloatingNumber.SizeOf` goes (the critical strip carries the size); grep its callers.

## Tests
- `DigitStripTests` (new), alpha arrays built in the test (Game.Tests has no graphics device, so the real files are
  checked by the hand check and `UiArtFilesTests` only): `Measure_NarrowDigit_FindsItsColumns` (a "1" cell with columns
  15-24 opaque: source x 15 of its cell, width 10); `Measure_DigitTouchingItsCellsEdges_KeepsTheWholeCell`;
  `Measure_EmptyCell_ThrowsNamingTheFile`; `Measure_WidthNotTenCells_Throws`; `AdvanceOf_IsTheMeasuredWidth`.
- `UiArtFilesTests.EveryConstant_ExistsInItsFolder`: a row `(typeof(UiArtFiles.Fonts), ArtFolders.Fonts)`
  (`EveryGroup_IsCheckedAbove` fails without it).
- `FloatingNumberTests`: `SizeOf_Critical_IsBigger` and `SizeOf_Damage_IsTheNormalSize` go with `SizeOf` (the strip
  now carries the size; said in the Done note).

## Verify by hand
1. Hit a goblin: the number is the strip's digits, white with the lavender lower part and the dark edge, tight, hopping
   as in NU1; F12 and compare with the clip.
2. A critical: bigger, red-gold, crisp (not scaled); a heal (green) and Poison's tick (purple) keep the gradient
   readable.
3. A "1" in a number ("31", "117"): no gap around it.

- **Done, with deviations:** the strips are measured on the view's first draw (it gets the texture library only
  there), not when the battle screen is built, so a broken strip stops the battle at its first number; a private
  `DigitStripImage` in the view holds each strip's texture, measured digits and advances. `ArtFolders.Fonts` is a
  constant only: `FontLibrary` keeps its own `art/fonts` path. The `SizeOf` tests went with `SizeOf`.
  Review: no Critical; `DigitRow`'s summary and param say the strip's measured widths; `DigitStrip.Advances` (a span,
  replacing `AdvanceOf`) so the view no longer repeats the ten; `DigitStripImage` holds no array.
