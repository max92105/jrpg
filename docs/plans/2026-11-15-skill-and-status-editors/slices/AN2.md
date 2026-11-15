# AN2. The GIF import plays the GIF before it writes, its pace editable for a battle sprite

Editor, about 220 lines (about 90 of them tests). Decisions 987, 988 (the Enemies page's effects). Design: design.md,
sections 2.4 and 2.9. Builds on AN1b and the mode bar's WD5 (`Battle/EnemyCatalogPage`; if WD5 hasn't landed, the same
change goes into `EnemyCatalogWindow`).

## Goal
Choosing GIFs to import opens the dialog with the first chosen GIF playing in the shared control, composed as the
import composes it, its background cleared live when the box is ticked. For one battle sprite GIF the pace box is
filled with the GIF's most common delay (decision 632) and editable, and the loop takes the typed pace; with several
GIFs it greys ("each keeps its own pace"); for an effect GIF imported from the Enemies page it greys with "set its pace
in Effect strips" (decision 639 kept there). Cancel writes nothing.

## New types
- `Editor/GifImport/ComposedGif` (internal sealed record): a GIF's whole frames at its own size and its most common
  frame time, read once for the dialog's preview. `ComposedGif(IReadOnlyList<uint[]> Frames, int Width, int Height,
  int FrameMilliseconds)`; `static Read(string path)` through `GifReader`, `GifFrameComposer` and `GifFrameTiming`
  (throws as `GifReader` does; a one-frame GIF is read, and the conversion refuses it later as today).

## Changes
- `Editor/GifImport/GifImportOptions`: `int? FrameMilliseconds { get; init; }` ("the pace typed in the dialog; null for
  each GIF's most common delay"); `Default` keeps null.
- `Editor/GifImport/GifConversion.Convert`: the loop's frame time is the options' when set; the note says "the typed
  110 ms" in place of "the most common".
- `Editor/Battle/GifImportOptionsViewModel` (57): built by two factories, `ForBattleSprites(ComposedGif? first, int
  gifCount)` (the pace typed with one GIF) and `ForEnemyEffects(ComposedGif? first, int gifCount)` (the pace greyed);
  AN4 adds `ForEffectStrips`. Its height note reads the GIF's height (was `gifHeight`); `FrameMillisecondsText`
  (filled with the GIF's frame time, "100" without a GIF), `FrameMilliseconds` (`int?`, for the preview), `IsPaceTyped`,
  `PaceNote` ("each keeps its own pace", "set its pace in Effect strips", or empty); `Problem` adds "Frame time must be
  a whole number from 1." while the pace is typed; `ToOptions` carries the typed pace only while `IsPaceTyped`;
  `ComposedGif? Gif` for the window.
- `Editor/Battle/GifImportWindow` (31 + 26 XAML): a `FramePreview` (256 x 300) beside the boxes, `FrameMilliseconds`
  bound one-way; its frames `PreviewFrames.OfPixels` of the GIF's frames, through `BackgroundRemoval.Apply` while
  `RemovesBackground` is ticked, rebuilt when the box changes; the pace box enabled by `IsPaceTyped`, `PaceNote` beside
  it.
  - `static GifImportOptions? AskFor(Window owner, IReadOnlyList<string> paths, Func<ComposedGif?, int,
    GifImportOptionsViewModel> optionsFor)`: moved from the Enemies page's `AskGifOptions` and `HeightOf` (the default
    options when no GIF is chosen; null on Cancel); the first GIF read with `ComposedGif.Read` under one catch of
    `InvalidDataException`, `IOException`, `UnauthorizedAccessException` and `NotSupportedException` (WPF's decoder),
    the dialog then showing no preview and the import naming the file among its refusals (`.claude/rules/editor.md`).
- `Editor/Animation/PreviewFrames`: `OfPixels(IReadOnlyList<uint[]> frames, int width, int height)`: each frame as a
  frozen `Bgra32` `BitmapSource`.
- `Editor/Battle/EnemyCatalogPage.xaml.cs` (205 as a window): `AskGifOptions` and `HeightOf` go; `OnImportClicked` calls
  `GifImportWindow.AskFor(Window.GetWindow(this), paths, ...)` with `ForBattleSprites` for the battlers folder and
  `ForEnemyEffects` for the effects folder (about -35).

## Tests
- `Editor.Tests/GifImport/ComposedGifTests` (new; `GifFixtures` on `StaThread`):
  `Read_TwoFrameGif_ComposesBothAtItsSize`, `Read_DelaysDiffer_TakesTheMostCommon`.
- `Editor.Tests/Battle/GifImportOptionsViewModelTests` (92 lines):
  - `FrameMillisecondsText_OneBattleGif_FilledWithItsFrameTime`.
  - `ToOptions_OneBattleGifTypedPace_Carried`.
  - `ToOptions_SeveralGifs_CarriesNoPace` (the review's C5).
  - `ToOptions_EnemyEffect_CarriesNoPaceAndSaysSo`.
  - `Problem_PaceZero_SaysSo`.
- `Editor.Tests/GifImport/GifConversionTests` (40 lines): `Convert_TypedPace_LoopTakesIt`.
- `Editor.Tests/Animation/PreviewFramesTests`: `OfPixels_TwoFrames_ImagesOfTheirSize`.

## Verify by hand
1. Database > Enemies..., the gnoll's Art tab, Import... into battlers, `art-src/gifs/GnollArcher.gif`: the dialog
   shows the archer playing at its frame time.
2. Tick the background box: the near-white background goes in the preview at once (MB3's finding); untick it.
3. Type 200: it slows; Pause and step through its frames. Cancel: nothing is written (`git status` clean).
4. Again with 150, OK, then Replace: the enemy's loop boxes read 150 ms. Cancel the Enemies page and restore the image
   with `git checkout content/art/battlers`.
5. Import... two gnoll GIFs at once: the first plays, the pace box greys with "each keeps its own pace"; Cancel.
6. Import... into effects `art-src/gifs/FireExplosion.gif`: the pace box greys with "set its pace in Effect strips";
   Cancel.
7. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` or binding-error line.
