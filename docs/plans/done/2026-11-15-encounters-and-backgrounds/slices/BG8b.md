# BG8b. The Battle backgrounds page and Database > Battle backgrounds

Editor; BG8's split: BG8 landed `Backgrounds/BattleBackgroundEditingViewModel` (`Start`/`Apply`),
`CatalogDraft.SetAnimatedBackgrounds` and `BattleBackgroundCatalogViewModel.RefreshAfterStillImport`. About 200 lines
(about 135 of XAML and code-behind, 30 of `FramePreview`, 30 of wiring, 2 tests). Needs BG8, the mode bar plan's stage 1
(`Workspaces/CatalogDialog`, `Shell/DatabaseMenu.xaml`) and row 7's AN1 and AN6 (`Animation/FramePreview`,
`PreviewFrames.OfCells`). Decisions 901, 902. Design: "Tunables and placeholders" (the preview area).

## Goal
BG8's goal: **Database > Battle backgrounds...** opens the page in `Workspaces/CatalogDialog` (BG1's note: no
`ICatalogEditing<T>` until the mode bar's WS2 lands): the backgrounds left, each with its kind; right, the chosen one's
kind line, its frame time box (enabled for an animated one), row 7's `FramePreview` playing it cover-fit in a 480 x 270
area, and **Import...** for PNGs. OK records one undo step; Ctrl+Z undoes it; Save all writes
`animated-backgrounds.json`.

## New types
- `Backgrounds/BattleBackgroundPage` (`UserControl`, XAML + code-behind): BG8's "New types" entry for it, unchanged:
  the recipe's page (no OK, Cancel or problem line; theme resources only from `EditorTheme.xaml`; typed boxes on
  `PropertyChanged`); `PreviewWidth` and `PreviewHeight` (480.0 and 270.0, `double` constants read by `x:Static`); the
  preview's `Frames` are `PreviewFrames.OfCells(image, frameSize, row.PreviewCells)` for an animated row and the whole
  image for a still, rebuilt when the chosen row changes (an import builds a new row, so a kind changes only with the
  row); each image decoded once per page through the shell's lookup (`SpriteImageFile.LoadFrom`, after
  `PngSize.IsReadable`), an imported file's dropped first. **Import...** under the list (`ArtImageFileDialog`, "PNG
  images (*.png)" for this folder), its line under it (`result.Summary`, refusals included, as the Enemies page's:
  the dialog covers the status band), then `RefreshAfterStillImport(result.Imported)`.

## Changes
- `Animation/FramePreview`: a `Stretch` dependency property (default `Uniform`, so its other hosts are unchanged)
  passed to its image, centered and clipped to its area, so this page asks `UniformToFill` (cover-fit, as the game
  draws it); and a `PictureHeight` (default unset, the area taking what its host leaves) so the area is 480 x 270 with
  the buttons and frame line under it. A control has no headless test; the hand check's step 2 covers it.
- `Backgrounds/BattleBackgroundViewModel`: `FrameMilliseconds` (the typed box as a number, null while it isn't one), the
  preview's frame time, raised with `FrameMillisecondsText`; tests `FrameMilliseconds_Typed_IsTheNumber` and
  `FrameMillisecondsText_Changed_RaisesFrameMilliseconds`.
- `Shell/MainViewModel.Database`: `BattleBackgroundEditing`, built with the backgrounds folder's readable images and
  their sizes; `MainViewModel.cs` sets it (+1, 291 lines on BG8). The readable images and sizes of a folder are their
  third copy (Effect strips, Character sprites): extract them there (`ReadableImagesIn(artFolder)`,
  `ReadableSizeOf(artFolder, file)`) and let the older two call them.
- `Shell/MainWindow.Database.cs` (240 lines): `["Battle backgrounds"] = OpenBattleBackgrounds`, `OpenInDialog("Battle
  backgrounds", editing.Start, editing.Apply, edited => new BattleBackgroundPage(edited, loadImage, import))`, the
  import `_viewModel.ImportArtImages(ArtFolders.Backgrounds, paths, GifImportOptions.Default)`; returns when the editing
  is null. A file copy is not a draft change, so it needs no apply first (the recipe's step 8).
- `Shell/DatabaseMenu.xaml`: "Battle backgrounds..." with `Tag="Battle backgrounds"` after Effect strips, its tooltip
  "Every battle background, still or animated: an animated one plays here at its frame time; import PNGs. The folder
  also holds the title's and the menu's pictures." (BG9, which brings the GIFs, adds ", or GIFs as animations" after
  "import PNGs").
- `Catalogs/ArtImageImport` and `Catalogs/ArtImageFileDialog`: their summaries name the Battle backgrounds page among
  their users (PNGs here; BG9 adds GIFs).
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Battle backgrounds...'` in the default `-Windows`, after
  Effect strips, as a page.
- `docs/map.md`: the `Backgrounds` row names the page; `Animation`'s `FramePreview` its `Stretch` and `PictureHeight`;
  `Shell`'s readable-image lookups if named there.

## Tests
- `BattleBackgroundViewModelTests`: the two above.

## Verify by hand
BG8's "Verify by hand" list, all of it (the script's line, the Forest and ForestPulse lines and preview, typing 600 and
`x`, Apply / Ctrl+S / Ctrl+Z / Ctrl+S, an Import of an outside 1920 x 1080 PNG deleted afterwards, the capture).
- **Note from BG8 (the split):** BG8 counted about 375 lines with tests at its start (past 320, no split point named),
  so the page, the preview's cover-fit, the row's `FrameMilliseconds` and the menu wiring moved here, as IU15b.
- **Note from BG8 review:** add a test importing a PNG named like a missing image's record (`Gone.png`): its record
  is dropped and its row moves ahead of the missing-image rows (in a sibling of `BattleBackgroundCatalogViewModelTests`,
  which is at 248 lines).
- **Done, with deviations:** `Backgrounds/BattleBackgroundPage` (58 lines of XAML, 85 of code: the list shows each
  row's kind line under its file, a missing image's in the problem color; the import's line under Import...);
  `Animation/FramePreview` 37 + 137 (`Stretch` and `PictureHeight`, the image centered so a cover-fit frame shows its
  middle); `BattleBackgroundViewModel.FrameMilliseconds` (124 lines; three tests, one more than planned: a box that
  isn't a number is null; `BattleBackgroundViewModelTests` 233); the review note's two tests in
  `BattleBackgroundImportTests` (51). The readable images and sizes are `MainViewModel.Database`'s `ReadableImagesIn`
  and `ReadableSizeOf` (172 lines), the Effect strips, Items and Character sprites pages' copies gone
  (`ReadableEffectImages` inlined; `Data/Art/ArtFolders.FieldSpriteImagesIn` lost its only caller and went, its test
  now `ImagesIn_RepositoryFieldSprites_ListsPlayerAndPlaceholder`). The decode-once image lookup was its third copy
  here: `Catalogs/DecodedArtImages` (36 lines, `Of`, `Forget`; `DecodedArtImagesTests` 62, 4 tests), held by this
  page, `Battle/EffectStripCatalogPage` (95) and `Catalogs/ArtPictureConverter` (22), a file that can't be read still
  kept as null. `MainWindow.Database.cs` 258, `MainViewModel.cs` 292. The menu tooltip ends "import PNGs." until BG9.
  About 400 lines with tests, past the 200 planned (the page's code-behind, the preview's two properties with their
  docs, the review's extraction). Tests: 7262. The smoke script opened the Effect strips and Battle backgrounds pages
  with no `ERROR` and no binding error.
