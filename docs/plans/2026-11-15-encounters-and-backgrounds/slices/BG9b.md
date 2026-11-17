# BG9b. The GIF dialog's background look, the backgrounds' GIF import and the page's Import... taking GIFs

Editor; BG9's split: BG9 landed the dialog view model's background kind (`Battle/GifImportOptionsViewModel.Background`:
`ForBackground`, `HeightChoices` of `Battle/BackgroundHeightChoice`, `SelectedHeight`, `SheetLine`, the sheet's
problem, `PreviewAtChosenHeight`), `Tiles/SheetImportResult.Backgrounds` and
`Backgrounds/BattleBackgroundCatalogViewModel.TakeImport(SheetImportResult)` (the still import's step, now also
building an animated row from a GIF's record). About 200 lines (about 50 of `ArtImageImport`, 45 of XAML and
code-behind, 10 of wiring, 3 tests). Decisions 898, 902, 903, 904, 1005.

## Goal
BG9's goal, the parts a window shows: on the Battle backgrounds page, **Import...** also takes a GIF. It opens AN2's
GIF dialog in its background kind: the GIF plays at once at the chosen height and the typed frame time; the height box
offers the GIF's own height then the divisors of 1080 below it; the typed height box, its factor note and the
background-clearing box are hidden; a line names the sheet and its video memory; the problem line greys OK. OK writes
the sheet into `backgrounds/` and the page shows it animated and chosen, its record in the page's session until Apply.

## New types
None.

## Changes
- `Catalogs/ArtImageImport` (a draft was written during BG9 and parked outside the repository; rewrite it from this
  list): `TakesGifs` adds `ArtFolders.Backgrounds` (so `ArtImageFileDialog` offers "Images (*.png;*.gif)" there); a
  background GIF goes through `GifConversion.ConvertToBackground(path, ComposedGif.Read(path), options)` and Data's
  `ImportArtImage` inside the existing temporary-folder step (`ImportGif` switching on the folder), its record in the
  result's `Backgrounds` by the PNG's name, its note in `Notes`; the refusal names the third kind ("only a battle
  sprite, an attack effect or a battle background is imported from a GIF."); one background GIF an import: every GIF
  after the first chosen is refused "X.gif: import one background GIF at a time." (decision 903; the import line under
  Import... shows it, as the dialog covers the status band). The refusals in one private `GifRefusalOf(artFolder,
  path, firstGif)` so `Import` stays under the complexity bound. The class summary names the third kind.
- `Battle/GifImportWindow` (XAML + code-behind): in the background kind (`IsBackground`) a "Frame height" `ComboBox`
  over `HeightChoices` (`DisplayMemberPath="Label"`, `SelectedValuePath="Height"`, `SelectedValue` bound to
  `SelectedHeight`), the `SheetLine` under it (muted, wrapping); the typed height box, the factor note and the
  clearing box in one panel collapsed for the background kind (a `DataTrigger` style on the panel, not on the
  `CheckBox`, so the theme's styles stay); a window resource `BooleanToVisibilityConverter` for the background panel.
  The preview's frames: `PreviewAtChosenHeight` in the background kind (re-shown on its notice), else AN2's
  (`ShowGif` as today). Focus the combo on load in the background kind. The window summary names the kind.
- `Backgrounds/BattleBackgroundPage`: the import function takes the options
  (`Func<IReadOnlyList<string>, GifImportOptions, SheetImportResult>`); **Import...** asks
  `GifImportWindow.AskFor(owner, paths, (first, _) => GifImportOptionsViewModel.ForBackground(first))` (no dialog when
  no GIF is chosen) and hands the result to `TakeImport`. Its summary and the button's tooltip name GIFs ("PNG
  pictures, each a still background, or one GIF as an animated one; ...").
- `Shell/MainWindow.Database.OpenBattleBackgrounds`: `(paths, options) => _viewModel.ImportArtImages(ArtFolders.Backgrounds,
  paths, options)`.
- `Shell/DatabaseMenu.xaml`: the tooltip's "import PNGs." becomes "import PNGs, or GIFs as animations." (BG8b's note).
- `docs/map.md`: `Catalogs`' `ArtImageImport` names the backgrounds' GIFs; the `Battle` row's `GifImportWindow` its
  background kind; `Backgrounds`' page that Import... takes GIFs.

## Tests
- `ArtImageImportGifTests` (Editor, existing, 209 lines; a sibling if it would pass 250):
  - `Import_BackgroundGif_WritesTheSheetAndReturnsItsRecord` (a 4 x 4 fixture GIF of 3 frames at its own 4 px: the
    sheet's size, the record's frame size, count and time, its note in the summary);
  - `Import_TwoBackgroundGifs_RefusesTheSecond` (the second named, the first imported);
  - `Import_GifIntoPortraits_IsRefusedByName`: its expected refusal names the third kind.

## Verify by hand
`open-editor-windows.ps1` on `'Database>Battle backgrounds...'` and `'Help>Raise a test error'`: no `ERROR` line, no
binding error. It can't open the GIF dialog (it needs a chosen file): BG9's "Verify by hand" steps 1-5 open it.
- **Note from BG9 (the split):** BG9 counted about 435 lines with tests at its start (past 320), so the window, the
  page's wiring and `ArtImageImport` moved here. `ArtImageImport` came along because widening `TakesGifs` offers GIFs
  in the page's file dialog at once, and the page would then import one with the default 128 px options, which
  `ConvertToBackground` refuses with a throw for a GIF under 128 px.
