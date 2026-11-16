# AN4. The Effect strips page imports PNGs and GIFs, an effect GIF's pace becoming its row's

Editor, about 200 lines (about 90 of them tests). Decisions 987, 988, 989. Design: design.md, section 2.6 ("Strips
enter the Effect strips session"). Builds on AN2.

## Goal
The Effect strips page gets Import...: PNGs and GIFs into the effects folder through the shared import (the GIF dialog
with its preview, its pace box typed for one GIF). The imported strips appear in the page's list at once, chosen and
playing: a new one as a row, a replaced one with its new frames; an effect GIF's typed pace is that row's typed frame
time (a replaced row keeps its swing and its hit frame below the new frames, else 0). Nothing reaches
`effect-strips.json` until the page's OK, as any typed row (decision 988). This is where a skill's strip gets its pace.

## Changes
- `Editor/Battle/GifImportOptionsViewModel`: `ForEffectStrips(ComposedGif? first, int gifCount)` (the pace typed with
  one GIF, as `ForBattleSprites`).
- `Editor/GifImport/GifConversion.ConvertToEffect`: with a typed pace, the result's `Loop` is the strip's frames at it
  (null without, as today, so the Enemies page's import is unchanged); `EffectNoteOf` says "paced at 110 ms" then.
- `Editor/Catalogs/ArtImageImport`: an effect GIF's loop goes to `SheetImportResult.Animations` as a battle sprite's
  does; the summary says the effects' loop is a pace for the Effect strips session.
- `Editor/Battle/EffectStripCatalogViewModel`: `TakeImport(SheetImportResult result, ContentCatalog current,
  Func<string, (int Width, int Height)?> sizeOf)`: each imported file's row replaced (or inserted in name order) from
  the draft's record, or the unlisted pace, at its new size; a file with a loop in `result.Animations` gets that
  frame time typed and its hit frame kept below the new frame count, else 0; the swing choices gain the new names in
  place; the last imported one chosen. A replaced row's earlier typed edits are dropped (the summary says so).
- `Editor/Battle/EffectStripCatalogPage`: an Import... button under the list (tooltip "PNG strips of square frames, or
  GIFs made into strips; a GIF plays first so its pace can be set"); a constructor parameter
  `Func<IReadOnlyList<string>, GifImportOptions, SheetImportResult> import`; the click reads the file dialog
  (`*.png;*.gif`, owned by `Window.GetWindow(this)`), asks `GifImportWindow.AskFor(..., ForEffectStrips)`, imports,
  then calls `TakeImport` and shows the import's summary on its status line.
- `Editor/Shell/MainWindow.Database.cs`: the Effect strips registration passes `paths, options =>
  _viewModel.ImportArtImages(ArtFolders.Effects, paths, options)` and the size lookup.

## Tests
- `Editor.Tests/Battle/EffectStripImportTests` (new: `EffectStripCatalogViewModelTests` is about 240 after the mode
  bar's WD6; images, sizes and records built in the test):
  - `TakeImport_NewImage_InsertedInNameOrderAndChosen`.
  - `TakeImport_NewGifWithAPace_RowTypedAtItsPace` (and listed, so OK writes its record).
  - `TakeImport_ReplacedImage_KeepsSwingAndHitWithinTheNewFrames`.
  - `TakeImport_ReplacedImageWithFewerFrames_HitBackToZero`.
  - `TakeImport_NewImage_OfferedAsASwing`.
- `Editor.Tests/Catalogs/ArtImageImportGifTests` (192 lines): `Import_EffectGifWithAPace_ReportsItsLoop`,
  `Import_EffectGifWithoutAPace_ReportsNoLoop`.
- `Editor.Tests/GifImport/GifConversionTests`: `ConvertToEffect_TypedPace_LoopTakesIt`.

## Verify by hand
1. Copy `art-src/gifs/GoldenSlashImpact.gif` outside the repository as `TestSlash.gif`. Database > Effect strips...,
   Import..., `TestSlash.gif`: the dialog plays it; type 70, OK: `TestSlash.png` is listed at 70 ms, chosen and
   playing. Esc: no record written (`git diff content/effect-strips.json` empty).
2. Again, OK this time, Save all: `effect-strips.json` holds `TestSlash.png` at 70 ms. Ctrl+Z, Save all.
3. Delete `content/art/effects/TestSlash.png`.
4. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` or binding-error line.
- **Note from AN2:** `ForEffectStrips` goes beside `ForBattleSprites`: the constructor is private, the factories set
  `IsPaceTyped` or `PaceNote` by `init`; pick its several-GIF note (`ForBattleSprites` says "each keeps its own pace").
  `GifConversion.PaceNoteOf` already words "the typed 110 ms"; `EffectNoteOf` can follow it.
