# BG9. AN2's GIF dialog gains a background kind: the GIF plays at the chosen height before it is written

Editor; about 250 lines with tests (about 130 of code, 12 tests). Needs BG6, BG8 and row 7's AN2
(`Battle/GifImportWindow.AskFor`, `GifImportOptionsViewModel` with its pace box, `ComposedGif`,
`PreviewFrames.OfPixels`). Decisions 898, 902, 903, 904. Design: "Tunables and placeholders", "Animated backgrounds
(ADR 0059)" (the cost table the sheet line reports), "Risks" (the view model's size).

## Goal
On the Battle backgrounds page, **Import...** also takes a GIF. It opens AN2's GIF dialog in its background kind: the
GIF plays at once in the shared preview at the chosen height and the typed frame time (the developer's "always a
preview and be able to tweak them: frame rate"); the height box offers the GIF's own height then the divisors of 1080
below it; the background-clearing box is hidden; a line names the sheet that would be written and its video memory;
the problem line greys OK. OK writes the sheet into `backgrounds/` and the page shows it animated and chosen, its
record in the page's session until Apply.

## New types
None.

## Changes
- `Battle/GifImportOptionsViewModel` (AN2's; recount, and put the kind in a `GifImportOptionsViewModel.Background.cs`
  partial if the file would pass 250): a background kind, `static ForBackground(ComposedGif gif)`:
  - `bool IsBackground`; `IReadOnlyList<int> HeightChoices`: the GIF's height capped at 1080, then each of
    `ScreenDivisors` (1080, 540, 360, 270, 216, 180; the comment: the 1080 px screen's whole divisors, decision 898)
    below it;
  - `SelectedHeight`: the GIF's own when `FrameGrid.MostFrames` at its size holds every frame, else the largest choice
    that does, else the GIF's own (the problem line then says so); notifies `SheetLine`, `Problem`, `CanApply` and the
    preview's frames;
  - `string SheetLine`: "24 frames of 480 x 270 px: a 3840 x 810 sheet (8 x 3), 12.4 MB of video memory" (`FrameGrid`'s
    size; 4 bytes a pixel, MB of 1,000,000 bytes, one decimal, invariant culture); empty while the frames don't fit;
  - `Problem` adds, after the pace: "24 frames don't fit a 4096 px sheet at 1080 px (6 at most): choose a smaller
    height.";
  - `PreviewFrameSource`: the GIF's frames at the chosen height (`FrameScaler.ScaleAveraged` below its own, decision
    904), worked out once per height while the dialog is open; the battlers' and effects' kinds keep AN2's frames;
  - `ToOptions`: the chosen height as `BattleHeight`, `RemovesBackground` false, the typed pace.
- `Battle/GifImportWindow` (AN2's): in the background kind the height combo shows (labels "443 px (the GIF's)", "360
  px"), the typed height box and the clearing box hide, the sheet line shows; the preview's frames come from
  `PreviewFrameSource` through `PreviewFrames.OfPixels`. `AskFor(owner, paths, ArtFolders.Backgrounds)` builds the
  background kind for the first GIF.
- `GifImport/` none beyond BG6.
- `Catalogs/ArtImageImport`: `TakesGifs` adds the backgrounds; a background GIF goes through
  `GifConversion.ConvertToBackground(path, ComposedGif.Read(path), options)` and Data's `ImportArtImage`, its record
  in the result's new `Backgrounds` (file to `AnimatedBackgroundDefinition`, as `Animations` holds a battler's loop);
  the refusal "only a battle sprite or an attack effect is imported from a GIF" names backgrounds too. The summary
  names the third kind.
- `Catalogs/SheetImportResult`: `IReadOnlyDictionary<string, AnimatedBackgroundDefinition> Backgrounds` (init, empty by
  default).
- `Backgrounds/BattleBackgroundViewModel`: `TakeAnimation(AnimatedBackgroundDefinition record, (int Width, int Height)
  size)`: the record, its time box and the sheet's size.
- `Backgrounds/BattleBackgroundCatalogViewModel`: `TakeImportedAnimation(AnimatedBackgroundDefinition record, (int
  Width, int Height) size)`: the row of that file takes it, or a new row is inserted in name order; it becomes chosen.
- `Backgrounds/BattleBackgroundPage` and its registration in `Shell/MainWindow.Database`: **Import...** offers "Images
  (*.png;*.gif)"; the PNGs go through BG8's path; the first GIF opens `GifImportWindow.AskFor` (any other GIF chosen
  with it is named in the status band: "Import one background GIF at a time."); asked before replacing an image already
  there (decision 125, the words `ImportArtImages` uses); on OK, `ImportArtImages(ArtFolders.Backgrounds, [gif],
  options)`, then `TakeImportedAnimation` for the result's record with the sheet's size. A file copy and a session
  change, not a draft change: no apply first (the recipe's step 8).
- `docs/map.md`: the `GifImport` row says a background becomes a grid sheet; the `Battle` row's GIF dialog names its
  background kind; `Backgrounds` says the page imports GIFs.

## Tests
- `GifImportOptionsViewModelBackgroundTests` (Editor, new sibling of AN2's `GifImportOptionsViewModelTests`; a
  `ComposedGif` built in the test from flat-color frames, no file):
  - `HeightChoices_GifOf443_AreItsOwnThen360Down`
  - `HeightChoices_GifTallerThan1080_StartAt1080`
  - `SelectedHeight_GifOf443WithEightFrames_IsItsOwn`
  - `SelectedHeight_GifOf1080With24Frames_Is540` (6 fit at 1080, 28 at 540)
  - `SheetLine_24FramesOf480By270_NamesSheetGridAndMemory` ("3840 x 810", "(8 x 3)", "12.4 MB")
  - `Problem_TooManyFramesAtTheHeight_NamesTheMost`
  - `SelectedHeight_Changed_RaisesSheetLineAndProblem`
  - `ToOptions_Background_NeverClearsAndCarriesThePace`
- `ArtImageImportGifTests` (Editor, existing; through `StaThread`, `GifFixtures`):
  `Import_BackgroundGif_WritesTheSheetAndReturnsItsRecord`.
- `BattleBackgroundCatalogViewModelTests` (BG7's file):
  - `TakeImportedAnimation_NewFile_AddsAnAnimatedRowAndChoosesIt`
  - `TakeImportedAnimation_OverAStill_MakesItAnimated`
- `BattleBackgroundViewModelTests` (BG7's file): `TakeAnimation_Still_BecomesAnimatedWithTheRecordsTime`

## Verify by hand
`open-editor-windows.ps1` can't open the dialog (it needs a chosen file); the steps below open it.
1. **Database > Battle backgrounds**, **Import...**, choose `art-src\gifs\FireExplosion.gif` (any GIF works; this one
   is in the repository). The dialog opens playing the explosion at its own 443 px; the height box offers "443 px (the
   GIF's)", 360, 270, 216, 180; no background box; the sheet line names 8 frames of 443 x 443 px and the memory.
2. Type 400 in the frame time: it slows at once. Choose **180 px**: the preview turns softer at once (averaged). Type
   `0`: the problem line, OK greys. Type 110.
3. **OK**: the page lists `FireExplosion.png` animated and chosen, playing at 110 ms; the PNG is in
   `content/art/backgrounds/`. **Revert** on the bar: the PNG stays (a file copy), no record is applied.
4. Delete `content/art/backgrounds/FireExplosion.png` (never committed); `git status` clean.
5. The developer, with a GIF of his own background: the same steps, **Apply**, then name it in a map's **Map >
   Settings...** **Battle background** and fight there (the game's side is BG5's).

- **Note from BG6 review:** `GifImportOptions.BattleHeight` now also means a background sheet's frame height; consider
  a neutral name (`FrameHeight`) once the dialog's background kind settles.
