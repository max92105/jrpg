# PM4b. The Party members page's pictures and imports

Editor, about 150 lines (about 40 of them tests, about 30 of XAML). Needs PM4 (the page, its art boxes and marks).
Split from PM4 on 2026-11-17 (PM4's estimate passed 320 lines with the XAML). Design: sections 1, 3; decision 953; the
mode bar's WS12 recipe, steps 3 and 8.

## Goal
The Party members page previews the chosen member's portrait and battle sprite at 160 px wide, and each art box gets
an Import... beside it that copies one PNG into its folder (decision 953: a GIF strip has no use for a member) and
picks the copied file in that box.

## New types
None.

## Changes
- `Party/PartyMemberCatalogViewModel`: `TakeImport(string artFolder, SheetImportResult result)`: the folders read again
  (`ArtChoices.Refresh`) and the last copied file picked in that folder's box; nothing when nothing was copied.
- `Party/PartyMemberCatalogPage` (`.xaml` + `.xaml.cs`): the two previews, decoded at 160 px through a lookup the shell
  gives (`SpriteImageFile.LoadAtWidth`, only after `PngSize.IsReadable`, by folder and file), each folder's pictures
  held in a `Catalogs/DecodedArtImages` so an import that replaced a file forgets it (`Forget(result.Imported)`); the
  three Import... buttons, each opening a file dialog owned by `Window.GetWindow(this)`, filtered to PNG, then the
  shell's import, the result's `Summary` under the boxes, then `TakeImport`. An import copies files, never changes the
  draft, so it needs no `TryApplyShown` (the recipe's step 8). The preview width is a `double` constant (x:Static).
- `Shell/MainWindow.Database.cs`: the page's registration moves to `RegisterPicturePages`, given
  `_viewModel.ImportArtImages` and the picture lookup.
- `docs/map.md`, Editor `Party` row: the page's pictures and imports.

## Tests
- `Party/PartyMemberCatalogViewModelTests`: `TakeImport_CopiedPortrait_PicksItInThePortraitBox` (the battle sprite box
  unchanged); `TakeImport_NothingCopied_ChangesNothing`; `TakeImport_CopiedPortrait_RefreshesTheFolders`.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Party members: shown as a page", no `ERROR`.
2. Database > Party members: `omer` shows its portrait and battle sprite. Type `Omer2.png` in the portrait box: the
   preview empties and the mark names it. Revert.
3. Import... beside Battle sprite, choose a PNG outside the content folder: it is copied into `content/art/battlers/`
   and chosen in the box (its `art-sources.json` line is then needed, as for any import); the status line says so.
4. Import... again choosing the same name: the replacement is asked first and the preview shows the new picture.
- **Note from PM4 review:** `TakeImport` raises `MissingArtNote` itself after `ArtChoices.Refresh()` (a member already
  naming the imported file gets no field change), with a test; and the "Not in the art folders: ..." sentence is built in
  `PartyMemberCatalogViewModel` and `EnemyCatalogPage.xaml.cs`: give `BattleArtChoices` one method returning it for the
  three names and call it from both.
- **Done, with deviations:** the review's note as asked: `BattleArtChoices.MissingNoteOf` replaces `MissingOf` (its
  three tests check the sentence), and `TakeImport` raises the note. The box is filled by a new
  `PartyMemberViewModel.Pick`, as the enemy's; the file dialog is a new `ArtImageFileDialog.ChoosePng` (one PNG even
  over the battlers and effects, whose shared dialog offers GIFs). The previews are 160 px squares, a name the folder
  lacks never read (decision 895). 7 tests (a battlers/effects theory, the note raised, `Pick`'s throw). Sizes: page
  `.xaml` 121, `.xaml.cs` 95, `PartyMemberCatalogViewModel` 153, `PartyMemberViewModel` 143; tests 250 and 124.
  Review fix: `Sprites/SpriteImageFile`'s decode reads a PNG cut after its header (decoder refusal) as null, from a
  stream it closes, so the Items, Cinematics, Enemies and members previews never throw on one (78 lines; 3 tests in
  the new `SpriteImageFileTests`, 79); the import's header-only check is a backlog line.
