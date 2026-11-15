# BG6. A GIF into a background sheet, headless

Editor (`GifImport/`); about 190 lines with tests (about 80 of code, 11 tests). Needs BG3 and row 7's AN2 (`GifImport/ComposedGif`). Decisions 896,
897, 904. Design: "Animated backgrounds (ADR 0059)", "Replacement mapping".

## Goal
A GIF read once (AN2's `ComposedGif`) can be written as a background sheet at a chosen frame height and frame time: its
whole frames kept at their own height or averaged down (never nearest neighbor when shrinking, never enlarged; decision
904), laid row by row in as many columns as fit 4096 px, as a PNG of the GIF's name in a temporary folder, with the
`AnimatedBackgroundDefinition` that describes it. No background color is cleared. No dialog yet: BG9 calls it.

## New types
- `GifImport/FrameGrid` (internal static): frames written row by row into one sheet within the texture limit.
  - `int ColumnsFor(int frameWidth, int frameCount)`: `min(frameCount, TextureLimit.MaxSide / frameWidth)`;
  - `int MostFrames(int frameWidth, int frameHeight)`: how many frames a 4096 px sheet holds at that size;
  - `(int Width, int Height) SheetSize(int frameWidth, int frameHeight, int frameCount)`;
  - `void WritePng(IReadOnlyList<uint[]> frames, int frameWidth, int frameHeight, string path)`: throws
    `ArgumentException` when the frames don't fit (the dialog refuses first); the last row's empty cells transparent.
- `GifImport/PngWriter` (internal static): BGRA pixels written as a PNG file; the encoder lines moved out of
  `FrameStrip.WritePng`, which now calls it (a pure move: `FrameStripTests` pass unchanged).

## Changes
- `GifImport/FrameScaler`: the private `Averaged` path is reached through a public `ScaleAveraged(frames, width,
  height, targetHeight)` (always the alpha-weighted average, whatever the factor); `Scale` is unchanged for battlers
  and effects. Its summary names the background's use.
- `GifImport/GifConversion` (about +25): `public static ConvertedGif ConvertToBackground(string gifPath, ComposedGif
  gif, GifImportOptions options)`: refuses a one-frame GIF as `Prepared` does; frames at the GIF's height kept, below it
  `FrameScaler.ScaleAveraged`; above it `ArgumentOutOfRangeException` (the dialog never offers it, decision 898); writes
  through `FrameGrid.WritePng` into a fresh temporary folder (the existing `Written` helper); the frame time is
  `options.FrameMilliseconds ?? gif.FrameMilliseconds` (AN2's typed pace).
- `GifImport/ConvertedGif`: an `AnimatedBackgroundDefinition? Background` (init, null for battlers and effects): the
  record a background sheet's import hands back (`File` the GIF's name as `.png`, the frame size, `Animation` of the
  frame count and time). Its summary names the third kind.
- `docs/map.md`: the `GifImport` row names `FrameGrid`, `PngWriter` and the background sheet.

## Tests
All through `StaThread`, GIFs written with `GifFixtures`, PNGs read back with `PngPixels` (as the folder's tests do).
- `FrameGridTests` (new):
  - `ColumnsFor_FewerFramesThanFit_IsTheFrameCount`
  - `ColumnsFor_MoreFramesThanFit_IsWhatFits` (480 px frames: 8)
  - `MostFrames_960By540_Is28`
  - `SheetSize_TenFramesInFourColumns_IsThreeRowsHigh`
  - `WritePng_TenFramesInFourColumns_PutsFrameFiveAtColumnOneRowOne` (each fixture frame one flat color)
  - `WritePng_LastRowPartial_LeavesTheEmptyCellsTransparent`
  - `WritePng_FramesThatDontFit_Throws`
- `GifConversionTests` (40 lines, plus AN2's):
  - `ConvertToBackground_AtTheGifsHeight_WritesTheGridAndItsRecord` (the record's frame size, count and file)
  - `ConvertToBackground_HalfTheHeight_AveragesTwoByTwo` (a fixture frame of alternating black and white pixels: every
    written pixel is the grey average, which nearest neighbor would never give)
  - `ConvertToBackground_TypedFrameTime_IsTheRecords` (typed 150 ms on a 100 ms GIF)
  - `ConvertToBackground_TopLeftColorKept_NothingCleared`

## Verify by hand
None: no dialog calls it yet (BG9 does). The suite runs green.
