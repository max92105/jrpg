# PM4. Database > Party members: the page with its pictures and imports

Editor, about 70 lines (about 30 of them tests) plus about 120 of XAML. Needs PM3 and row 11's mode bar (stage 3).
Design: sections 1, 3; decisions 942, 953, 962; the mode bar's WS12 recipe, steps 3-8 and 10.

## Goal
Database > Party members shows the Party members page under the Characters group: the members left (id: name) with Add
and Remove; the chosen member right: id (editable while new), name, class (a combo of the draft's classes by name),
summary (wrapping, four lines high), then three art rows, each an editable combo over its folder's files (battlers,
portraits, effects: decision 589's picker), Import... beside it and a missing mark when the folder lacks the name
(decision 442); the portrait and the battle sprite previewed at 160 px wide. The bar shows Apply, Revert and the
problem line.

**Recount first:** the files below on the day; one that would pass 300 gets its move in this slice (decision 962).

## New types
- `Party/PartyMemberCatalogPage` (`.xaml` + `.xaml.cs`, a `UserControl`): the Party members page, the list left and the
  chosen member's boxes and pictures right. Its pictures load through a lookup the shell gives
  (`SpriteImageFile.LoadAtWidth`, decoded at 160 px, only after `PngSize.IsReadable`); each Import... opens a file
  dialog owned by `Window.GetWindow(this)`, filtered to PNG (decision 953), calls the shell's import, then
  `PartyMemberCatalogViewModel.TakeImport`. An import copies files, never changes the draft, so it needs no
  `TryApplyShown` (the recipe's step 8).

## Changes
- `Battle/BattleArtChoices.cs` (62 lines): `MissingOf(EnemyArtViewModel art)` becomes `MissingOf(string battleSpriteFile,
  string portraitFile, string attackEffectFile)`, so the member's names are checked by the same rule without `Battle`
  seeing a member; `Battle/EnemyCatalogPage.xaml.cs` (row 11's WS7) passes the three names (its one caller).
- `Party/PartyMemberCatalogViewModel`: `BattleArtChoices ArtChoices` (from the shell's folder lookup, as the Enemies
  page's); `IReadOnlyList<string> MissingArt` for the chosen member (raised on its art names and on the selection);
  `TakeImport(string artFolder, SheetImportResult result)`: the folders read again and the last copied file picked in
  that folder's box, nothing when nothing was copied.
- `Party/PartyMemberEditingViewModel`: takes the art folder lookup (`Func<string, IReadOnlyList<string>>`), as
  `EnemyEditingViewModel` does.
- `Shell/MainViewModel.Workspaces.cs`: `CatalogPage.Of("Party members", CatalogGroup.Characters, ...)` after Classes.
- `Shell/MainWindow.Database.cs`: `Register<PartyMemberCatalogViewModel>(members => new PartyMemberCatalogPage(members,
  _viewModel.ImportArtImages, LoadPicture))`.
- `Shell/DatabaseMenu.xaml`: `<MenuItem Header="Party members" Command="{Binding ShowDatabasePageCommand}"
  CommandParameter="Party members" ToolTip="Each character's name, class, summary and battle pictures; events recruit
  them and the new game offers them as leaders." />`.
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Party members'` joins the default list.
- `docs/map.md`, Editor `Party` row: the Party members page.

**Fallback (row 11 moved after this row):** `Party/PartyMemberCatalogWindow` (880 x 600) with OK, Cancel and the
problem line, a `Database > Party members...` entry and `OnEditPartyMembersClicked` in `Shell/MainWindow.Party.cs`; the
`MissingOf` caller is then `Battle/EnemyCatalogWindow.xaml.cs`.

## Tests
- `Battle/BattleArtChoicesTests` (80 lines): the three `MissingOf` calls pass three names (no test added or removed).
- `Party/PartyMemberCatalogViewModelTests` (PM2's file, about 150 lines, about 190 after):
  - `MissingArt_PortraitNotInFolder_NamesIt` (a lookup built in the test without the member's portrait).
  - `MissingArt_ArtNameTyped_RaisesChanged`.
  - `TakeImport_CopiedPortrait_PicksItInThePortraitBox` (the battle sprite box unchanged).
  - `TakeImport_NothingCopied_ChangesNothing`.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Party members: shown as a page", no `ERROR`, no
   binding-error line. The page shows painted portraits, not Mana Seed art, so its screenshot may be opened.
2. Database > Party members: nine members in id order; `omer` shows Mage, its portrait and battle sprite.
3. Type `Omer2.png` in the portrait box: the mark names "portrait Omer2.png" and the preview empties; Apply stays lit
   (missing art doesn't stop a save, decision 442). Revert.
4. Add: id `ranger`, name `Ranger`, class Rogue, a summary, any battler, portrait and effect; Apply, Save all:
   `party-members.json` holds `ranger` in id order. In World mode add a Recruit step naming `ranger` to a map's NPC
   event, Save all; run the game and trigger it: Ranger joins with the rogue's skills.
5. Back in the editor, Ctrl+Z until the members' step: "Can't undo: party member ranger is used on map ..." in the
   status bar. Remove the recruit step, then the undo takes Ranger out. Save all; `git status` shows no change left.
6. Import... beside Battle sprite, choose a PNG outside the content folder: it is copied into `content/art/battlers/`
   and chosen in the box (its `art-sources.json` line is then needed, as for any import).
- **Note from CL1:** rows 6-11 landed as this plan expected, so this is a page in the Database mode, never a dialog:
  `Workspaces/CatalogDialog` is gone and the Fallback paragraph is void. The recipe is
  `docs/plans/done/2026-11-15-mode-bar/page-recipe.md` (what this file calls "the WS12 recipe"). Its lines on 2026-11-17:
  one `CatalogPage.Of(title, CatalogGroup.Characters, editing)` in `Shell/MainViewModel.Workspaces.cs`'s
  `CreateDatabaseWorkspace` (143 lines), the editing view model built there from its `content` parameter rather than
  as a new `XEditing` property (`MainViewModel.cs` is at 295 and each property costs a constructor line there); one
  `pageViews.Register<TEdited>(...)` in `Shell/MainWindow.Database.cs`'s `CreatePageViews` (96 lines; a page with
  pictures in `RegisterPicturePages`), and the map's Workspaces row count ("registers the fourteen") moves; the
  `Shell/DatabaseMenu.xaml` entry passes the title as `CommandParameter` (the recipe's "`Tag`" is stale);
  `CatalogGroup.Characters`' summary ("Character sprites.") gains the title; and
  `MainViewModelDatabaseWorkspaceTests.DatabaseWorkspace_ContentLoaded_ListsEveryPageInGroupOrder` gains it after the
  page before it, so "Tests: none new" still changes that one assertion. The smoke script's default list is its lines
  32-35.
- **Note from CL1:** the pictures and imports follow the pages as built on 2026-11-17: `MainViewModel.ImportArtImages`
  takes `(artFolder, paths, gifOptions)` and returns a `SheetImportResult`; the Enemies page gets its import, picture
  lookup and clock as one `Battle/EnemyArtSources` record registered in `RegisterPagesOnTheAnimationClock`, the Items
  and Cinematics pages a `SpriteImageFile.LoadAtWidth` lookup in `RegisterPicturePages`. With no idle loop, the members'
  page belongs in `RegisterPicturePages`; reuse `EnemyArtSources` only if its clock is harmless there.
- **Note from PM3 review:** check that Remove can't drop a member an open map already recruits: a member applied on
  this page restarts as saved (Remove greyed), so only an unapplied one is removable; confirm with a test if cheap.
- **Split on 2026-11-17:** with the XAML, the estimate passed 320 lines, so the previews, the three Import... buttons,
  `TakeImport` and the move to `RegisterPicturePages` are PM4b; this slice is the page with its boxes and art marks.
  Its hand check is steps 2 to 5 above, without the pictures.
- **Done, with deviations:** split as above. The mark is `MissingArtNote`, the line the page binds ("Not in the art
  folders: portrait Omer2.png."), in place of a list the code-behind joins; its tests are named after it, and two more
  check it empty and raised on choosing another member. `Problem_TypedChange_RaisesProblemChanged` now expects the
  note raised too. The PM3 review's Remove check is the editing test
  `Start_AfterApplyingAnAddedMember_CannotRemoveIt`. The smoke script shows the page, no `ERROR`, no binding error.
  Sizes: `PartyMemberCatalogPage.xaml` 91, `.xaml.cs` 17, `PartyMemberCatalogViewModel` 133, `BattleArtChoices` 64;
  tests 187 and 45 (5 new).
