# AN7. Strip from sheet cells writes the strip into the Effect strips session

Editor, about 200 lines (about 100 of them tests). Decisions 988, 990. Design: design.md, section 2.6 ("Strips enter
the Effect strips session"). Builds on AN6 and AN4 (`TakeImport`).

## Goal
The tool's OK writes the picked cells as a strip PNG into `effects/` (each cell at its own size, padded to a square as
an effect GIF's frames), through Data's import check, asking before it replaces an image: a file copy, never undone,
as any import. The strip then enters the Effect strips page as a typed row at the tool's pace, chosen and playing; its
record lands with that page's OK (decision 988). A refusal stays on the tool's problem line with the tool open.

## New types
- `Editor/Animation/SheetCells` (internal static): cuts the picked cells out of a sheet's pixels into frames.
  `Cut(BitmapSource sheet, (int Width, int Height) cell, IReadOnlyList<SpriteCell> cells)`: the sheet as `Bgra32`
  once (`FormatConvertedBitmap`), each cell's pixels copied into a `uint[]` in pick order; a cell outside the sheet
  throws `ArgumentOutOfRangeException` (the view model's problem line rules it out).
- `Editor/Catalogs/CellStripImport` (internal sealed): writes cut frames as an effects strip through Data. `Write(
  IReadOnlyList<uint[]> frames, (int Width, int Height) cell, string fileName)`: `GifImport/FrameStrip.WriteSquarePng`
  into a fresh temporary folder, `ContentWriter.ImportArtImage(ArtFolders.Effects, ...)` with the draft's catalog, the
  folder deleted in a `finally` (as `ArtImageImport.ImportGif`); throws what the import throws.

## Changes
- `Editor/Shell/MainViewModel.Database.cs`: `SheetImportResult ImportCellStrip(CellStrip strip, IReadOnlyList<uint[]>
  frames)`: asks before replacing (`_confirm`, as `ImportArtImages`), writes through `CellStripImport` under the
  import's catch (`ContentValidationException`, `IOException`, `UnauthorizedAccessException`: a refusal in the
  result), refreshes the art warning; the result's `Animations` carry the strip's frames at its pace, the shape AN4's
  `TakeImport` reads. No draft step.
- `Editor/Animation/StripFromCellsWindow`: an OK button greyed through `CanWrite`; its click cuts with `SheetCells` and
  hands the strip to a constructor parameter `Func<CellStrip, IReadOnlyList<uint[]>, SheetImportResult> write`; a
  refusal stays on the problem line, a write closes the window with `DialogResult = true` and keeps the result for its
  opener (`Written`).
- `Editor/Battle/EffectStripCatalogPage`: AN6's `Action<Window> openStripFromCells` becomes `Func<Window,
  SheetImportResult?> makeStripFromCells`; a result is taken with `TakeImport` (AN4) and its summary shown. The page's
  constructor then holds four parameters (the strips, the image loader, AN4's import and this one), the limit.
- `Editor/Shell/MainWindow.Database.cs`: the opener shows the tool over the owner with `_viewModel.ImportCellStrip` and
  returns its `Written` result, null on Cancel.

## Tests
- `Editor.Tests/Animation/SheetCellsTests` (new; on `StaThread`, a 4 x 2 cell sheet built with `BitmapSource.Create`,
  each cell one color): `Cut_TwoPicks_FramesInPickOrder`, `Cut_RepeatedCell_CopiedTwice`,
  `Cut_CellOutsideTheSheet_Throws`, `Cut_LastColumnOfAnOddWidth_ReadsItsPixels` (the area's edge test of
  `.claude/rules/code-quality.md`).
- `Editor.Tests/Catalogs/CellStripImportTests` (new; a copied content root, frames built in the test):
  `Write_TwoTallCells_WritesASquarePaddedStrip` (read back with `PngPixels`: width twice the height),
  `Write_ImportRefuses_LeavesNoTemporaryFolder`.
- `Editor.Tests/Shell/MainViewModelCellStripTests` (new; a copied content root as `MainViewModelSheetImportTests`
  makes it): `ImportCellStrip_At120Ms_WritesTheStripAndReportsItsPace` (no draft step recorded),
  `ImportCellStrip_ReplaceDeclined_WritesNothing`.

## Verify by hand
1. Database > Effect strips..., Strip from sheet cells..., Open sheet... `content/art/effects/FireExplosion.png`, click
   the row's number then Remove last until four remain (frames 1 to 4), name `FireStart`, Frame time 90, OK: the page
   lists `FireStart.png` at 90 ms, chosen and playing its four frames.
2. Esc: the image stays (a file copy), no record (`git diff content/effect-strips.json` empty). Again with OK on the
   page and Save all: the record holds 90 ms; Ctrl+Z, Save all.
3. The tool again with the same name: Replace? No: nothing written, the tool stays open; Cancel.
4. Delete `content/art/effects/FireStart.png`.
- **Note from AN6:** the window builds its own `StripFromCellsViewModel` (no constructor parameter yet: `write` is the
  first); OK goes in the bottom bar beside Cancel, the problem line on its left; the opener is one lambda in
  `MainWindow.Database.cs`.
