# BG8. The Battle backgrounds page: the shared preview, cover-fit, PNG import, Apply as one undo step

Editor; about 200 lines with tests (about 150 of XAML, code-behind and wiring, 6 tests). Needs BG7 (with BG7b and
BG7c, its splits), the mode bar plan's stage 1 (its page recipe, `Workspaces/CatalogDialog`, `Shell/DatabaseMenu.xaml`)
and row 7's AN1 and AN6 (`Animation/FramePreview`, `PreviewFrames.OfCells`). Decisions 901, 902. Design: "Tunables and
placeholders" (the preview area).

## Goal
**Database > Battle backgrounds** shows a page (built on the mode bar plan's recipe, its WS12 file): the backgrounds
left, each with its kind; right, the chosen one's kind line, row 7's `FramePreview` playing it cover-fit in a 480 x 270
area (its frame time box the record's, enabled for an animated one), and **Import...** for PNGs. The bar's Apply
records one undo step; Ctrl+Z undoes it; Save all writes `animated-backgrounds.json`.

## New types
- `Backgrounds/BattleBackgroundEditingViewModel` (internal sealed, `Workspaces/ICatalogEditing<BattleBackgroundCatalogViewModel>`):
  `Start`/`Apply` of the page, as the effect strips' editing view model. `Start()` builds the list from the draft and
  the lookups; `Apply(list)` calls `CatalogDraft.SetAnimatedBackgrounds(list.ToRecords())` and returns false on a
  problem.
- `Backgrounds/BattleBackgroundPage` (`UserControl`, XAML + code-behind): the page, per the recipe (no OK, Cancel or
  problem line; theme resources only from `EditorTheme.xaml`). The preview: a `PreviewWidth` x `PreviewHeight` area
  (480.0 and 270.0, `double` constants read by `x:Static`) holding a `FramePreview` set to fill it cover-fit; its
  `Frames` are `PreviewFrames.OfCells(image, frameSize, row.PreviewCells)` for an animated row and the whole image for
  a still, rebuilt when the chosen row or its kind changes; `FrameTimeText` bound to the row's
  `FrameMillisecondsText`. Each image is decoded once per page through a lookup, after `PngSize.IsReadable` (the editor
  rule on decoding).

## Changes
- `Animation/FramePreview` (row 7's AN1 draws its image `Stretch="Uniform"`): a `Stretch` dependency property
  (default `Uniform`, so its other hosts are unchanged) passed to its image and clipped to its area, so this page asks
  `UniformToFill` (cover-fit, as the game draws it); its buttons and frame line stay under the area. A control has no
  headless test; the hand check's step 2 covers it.
- `Catalogs/CatalogDraft.Database` (about +16): `SetAnimatedBackgrounds(IReadOnlyList<AnimatedBackgroundDefinition>)`,
  one step, nothing when the same by content (`HasSameRecords` with the record's equality: `TileAnimation` is a record,
  so equal by value); no undo refusal (named by file). The class summary lists the animated backgrounds.
- `Backgrounds/BattleBackgroundViewModel`: `TakeStillImage((int Width, int Height)? size)`: the record dropped, the new
  size read (decision 902).
- `Backgrounds/BattleBackgroundCatalogViewModel`: `RefreshAfterStillImport(IReadOnlyList<string> files)`: an existing
  row takes the still image; a new file is inserted in name order among the folder's images; the chosen row becomes the
  last imported one.
- `Catalogs/ArtImageImport`: its summary names the Battle backgrounds page among its users (PNGs here; BG9 adds GIFs).
- Page registration (the recipe's steps 3 and 4): one `CatalogPage.Of("Battle backgrounds", <the Battle group>,
  editing)` line in `Shell/MainViewModel.Workspaces`, built with `ArtFolders.ImagesIn(ArtRoot, ArtFolders.Backgrounds)`
  and the readable-size lookup as the effect strips' are; one `Register<...>` line in `Shell/MainWindow.Database`
  passing the image loader. The page's **Import...** asks for PNG files ("PNG images (*.png)"), imports them through
  `MainViewModel.ImportArtImages(ArtFolders.Backgrounds, paths, GifImportOptions.Default)` and calls
  `RefreshAfterStillImport(result.Imported)`; refusals go to the status band as the other imports'. A file copy is not a
  draft change, so it needs no apply first (the recipe's step 8).
- `Shell/DatabaseMenu.xaml`: a "Battle backgrounds" entry after Effect strips, bound to `ShowDatabasePageCommand` as
  its neighbours, tooltip "Every battle background, still or animated: an animated one plays here at its frame time;
  import PNGs, or GIFs as animations. The folder also holds the title's and the menu's pictures."
- `.claude/scripts/open-editor-windows.ps1`: "Database>Battle backgrounds" joins the default list (a page: "shown as a
  page").
- `docs/map.md`: the `Backgrounds` row names the page and its editing view model; `Catalogs` names
  `SetAnimatedBackgrounds`.

## Tests
- `BattleBackgroundEditingViewModelTests` (Editor, new; a draft over a catalog built in the test with one record):
  - `Apply_FrameTimeChanged_RecordsOneStep`
  - `Apply_NothingChanged_RecordsNoStep`
  - `Apply_Undo_RestoresTheOldTime`
- `BattleBackgroundCatalogViewModelTests` (BG7's file):
  - `RefreshAfterStillImport_OverAnimated_MakesItStill`
  - `RefreshAfterStillImport_NewFile_InsertedInNameOrderAndChosen`
- `BattleBackgroundViewModelTests` (BG7's file): `TakeStillImage_Animated_DropsTheRecordAndReadsTheNewSize`

## Verify by hand
1. `.claude/scripts/open-editor-windows.ps1`: "Battle backgrounds" shown as a page, no `ERROR` line, no binding-error
   line.
2. **Database > Battle backgrounds**: `Forest.png` "Still, ..."; `ForestPulse.png` "Animated: 4 frames of 960 x 540 px
   in a 2 x 2 grid", darkening and brightening in the preview, filling its 16:9 area; Pause and step: four frames.
   Type 600 in the frame time: it slows at once. Type `x`: the bar's problem line names the frame time, Apply greys,
   the preview holds a frame.
3. Type 300, **Apply**, **Ctrl+S**: `animated-backgrounds.json`'s record reads 300. **Ctrl+Z**, **Ctrl+S**: back to
   200, `git status` clean.
4. **Import...** a PNG from outside the content folder (any 1920 x 1080 PNG): it appears in the list, still, chosen;
   the file is in `content/art/backgrounds/` at once. Delete that file afterwards (it is never committed).
5. Capture the page with the pulse visible (it holds no Mana Seed art).
- **Note from BG1:** the mode bar's WS1-WS6 hadn't landed when row 10 started (`Workspaces/` holds `IEditedCatalog`,
  `CatalogDialog`, `DialogBounds`): unless WS2 has landed by then, host the page in `CatalogDialog` through
  `MainWindow.Database.OpenInDialog`, as WD2-WD8 and IU15b did, and declare no `ICatalogEditing<T>`.
- **Note from BG7c:** `BattleBackgroundCatalogViewModel` watches the rows it starts with (`OnBackgroundChanged` relays
  a typed frame time to `Problem`); a row `RefreshAfterStillImport` inserts or replaces is subscribed (and the replaced
  one unsubscribed) there, as `EffectStripCatalogViewModel.TakeImportedImage` does, with a test typing into an imported
  row. The problem line's row part is `Catalogs/ArtImageRowProblem`, shared with the Effect strips and Character
  sprites pages.
