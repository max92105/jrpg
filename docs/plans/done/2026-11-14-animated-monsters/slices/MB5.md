# MB5. Import... takes a GIF as a battle sprite

Editor, about 200 lines (about 100 of them tests). Needs MB2, MB4. Decisions 631, 632, 637; design "Structure"
(`GifImportOptions`, `GifConversion`).

## Goal
In the Enemies window's Art tab, **Import...** beside the battle sprite offers "Images (*.png;*.gif)". A chosen GIF
becomes `<its name>.png` in `content/art/battlers/`: its whole frames scaled to 128 px high (the default; MB6 adds the
dialog), side by side, through Data's import with its replace question and width limit. The battle sprite box picks it
and the idle boxes fill (Animated, the frame count, the frame time); the status line says "Imported GnollWarrior.png (8
frames of 100 x 128 px, 100 ms)" and, when the GIF's delays differed, "(the GIF's frame times differed; 100 ms is the
most common)". A one-frame GIF is refused ("GnollWarrior.gif has one frame: import a PNG."), as is a GIF for any other
folder than battlers (and effects from MB8).

## New types
- `Editor/GifImport/GifImportOptions` (internal sealed record `(int BattleHeight, bool RemovesBackground)`): what one
  GIF import asks; `Default` is 128 px, background kept.
- `Editor/GifImport/GifConversion` (internal static): one GIF into a strip file and its loop: `GifReader` →
  `GifFrameComposer` → `BackgroundRemoval` when asked → `FrameScaler` → `FrameStrip.WritePng` into a fresh temporary
  folder under `Path.GetTempPath()` with the PNG name. `ConvertedGif Convert(string gifPath, GifImportOptions options)`
  returning the strip's path, its `TileAnimation` and the timing note; throws `InvalidDataException` naming the file for
  an unreadable or one-frame GIF.

## Changes
- `Editor/Tiles/SheetImportResult`: `IReadOnlyDictionary<string, TileAnimation> Animations { get; init; }` (empty by
  default): "the loop each strip imported from a GIF carries, by its PNG name"; the summary line takes the import's
  notes (the frames, the timing note).
- `Editor/Catalogs/ArtImageImport.Import` takes the `GifImportOptions` (a fourth parameter): a `.gif` for the battlers
  folder goes through `GifConversion`, then `ContentWriter.ImportArtImage` (its checks and atomic copy, unchanged), the
  temporary folder deleted in a `finally`; the replace question asks for the PNG's name; `InvalidDataException` joins
  the caught refusals. Its summary names GIFs.
- `Editor/Battle/EnemyArtSources.Import` and `Shell/MainViewModel.ImportArtImages` pass the options (the default until
  MB6); the Character sprites window's import passes the default (it never offers GIFs).
- `Editor/Battle/EnemyCatalogViewModel.TakeImport`: when the result carries an animation for the picked battle sprite,
  the chosen enemy's art takes it; a PNG import leaves the boxes as they are.
- `EnemyCatalogWindow.xaml.cs` `OnImportClicked`: the battlers' dialog filter adds `*.gif`.
- `docs/map.md`: `Catalogs/ArtImageImport`'s line names GIFs.

## Tests
- `ArtImageImportTests` (Glob first; a sibling past 250), each through `StaThread.Run`, MB3's fixture GIF written to a
  temp folder:
  - `Import_GifIntoBattlers_WritesAStripAndItsLoop` (the strip `<name>.png` 128 px high, its width the frames' scaled
    widths summed; the result's animation 3 frames at the fixture's time).
  - `Import_OneFrameGif_IsRefusedByName`.
  - `Import_GifIntoPortraits_IsRefusedByName`.
  - `Import_GifDeclinedReplace_CopiesNothing`.
  - `Import_BrokenGif_IsRefusedAndLeavesNoTemporaryFile`.
  - `Import_GifWithMixedDelays_NotesIt`.
- `GifConversionTests` (new): `Convert_RemovesBackgroundAsked_TopLeftColorIsTransparent`;
  `Convert_Default_KeepsTheBackground`.
- `EnemyCatalogViewModelTests`: `TakeImport_GifResult_FillsTheIdleBoxes`; `TakeImport_PngResult_LeavesTheBoxes` (an
  enemy already animated keeps its boxes).

## Verify by hand
1. Run the editor. **Database > Enemies...**, choose `goblin`, **Duplicate**, type id `gnoll_warrior`; **Art** tab.
2. **Import...** beside **Battle sprite**, change the file type to the images filter if needed, open
   `art-src\gifs\GnollWarrior.gif`: the status line names the strip, its frames and time; the box shows
   `GnollWarrior.png`; Animated is checked with the frames and time; the preview loops the gnoll at 128 px high.
3. Import a GIF beside **Portrait**: refused by name. **Cancel** (or keep it: MB6's check imports the five).

- **Done, with deviations:** `GifConversion` returns a `ConvertedGif` (the strip's path, its loop and the status note);
  `SheetImportResult` gains `Notes` beside `Animations` for the line's "(8 frames of ... px, 100 ms)". The GIF tests went
  to a new sibling, `ArtImageImportGifTests`, and share a collection with `GifConversionTests` so the count of temporary
  strip folders sees no other test's; the PNG read-back helper is shared as `PngPixels`. `EnemyArtViewModel.TakeLoop`
  fills the boxes, tested through `TakeImport`. A GIF for another folder is refused before the replace question.
  The mixed-delay note reads "(3 frames of ... px, 100 ms; the GIF's frame times differed, 100 ms is the most
  common)", one parenthesis with the frames. Review: a Critical fixed: no test covered the `finally` deleting the strip's
  folder; the GIF import and `Import_StripTooWide_IsRefusedAndLeavesNoTemporaryFile` (Data refusing after the
  conversion) now check no strip folder is left. `GifConversion` deletes its folder when the PNG can't be written;
  `GifReader` refuses a frame that fails as it is converted too; `Convert_BackgroundKept_TopLeftStaysRed` renamed.
