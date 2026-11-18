# BT3. Database > Balance table...: the read-only page

Editor + script, about 170 lines (XAML about 130, wiring and the read-only members about 40). Decisions 992, 994, 996.
Design: design.md, section 1; the mode bar's [page recipe](../../2026-11-15-mode-bar/page-recipe.md), step 8 (a
read-only page). Builds on BT2 and BT2b (phase 4, after row 12, so after the mode bar's stage 3: re-read the recipe for
the registration in force).

## Goal
The balance table is a read-only Database page over the draft as it is (unsaved edits included): the level, modifier
and Power boxes on top; three tabs: Skills (the class skill rows, then the enemy skill rows), Classes and enemies,
Tiers. Its `Problem` is the boxes' only and never stops a move; Apply records nothing. He compares Fireball at levels
1, 10 and 20 with the goblin's HP without opening a file.

## New types
- `Editor/Balance/BalanceTablePage` (`UserControl`, the recipe's page): the rows are `ItemsControl`s inside a
  `ScrollViewer`, each row a `Grid` of `SharedSizeGroup` columns under a header row in the same scope
  (`Grid.IsSharedSizeScope`), the dark theme's text brushes, a heal's amounts in the muted brush (decision 994); the
  level headers read the typed levels ("Level 10").
- `Editor/Balance/BalanceTableEditingViewModel` (internal sealed): `Start()` (a `BalanceTableViewModel` over the
  draft's catalog) and `Apply` recording nothing, in the `ICatalogEditing` shape.

## Changes
- `Editor/Balance/BalanceTableViewModel`: implements `Workspaces/IEditedCatalog` (`Problem` the boxes', `ChosenKey`
  null, `Choose` a no-op: a table has no records), and whatever the recipe's step 8 asks of a read-only page then
  (`CatalogPage.IsReadOnly` from the mode bar's stage 3).
- `Editor/Shell/...`: the page's registration as the recipe gives it then (one `CatalogPage.Of` line, one `Register`
  line) and the "Balance table..." entry in `Shell/DatabaseMenu.xaml`, tooltip "Read only: each skill's damage or
  healing at chosen levels, classes' and enemies' HP and Power, and the tier sheet, by the battle's own formulas."
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Balance table...'` in the default list.
- `docs/map.md`: an Editor `Balance` row.

## Tests
- `Editor.Tests/Balance/BalanceTableViewModelTests`: `Choose_AnyKey_KeepsNothingChosen`, `ChosenKey_IsNull`;
  `Editor.Tests/Balance/BalanceTableEditingViewModelTests` (new): `Apply_AfterTypingALevel_RecordsNoStep`.

## Verify by hand
1. Database > Balance table (2026-11-18's content): Skills lists each class's skills; the mage's Fireball (`fire_1` x2,
   Power 12 +2 a level) reads 24, 60 and 100 at levels 1, 10 and 20; the monk's Flurry (`blunt_1` x2, Power 14 +2)
   reads "28 x 4 = 112", "64 x 4 = 256" and "104 x 4 = 416"; Classes and enemies: the goblin's Max HP 30. No class
   skill has a level above 1 today: to see "-", give one level 5 on the Classes page, apply, and come back.
2. Modifier -25: Fireball 18, 45, 75 and Flurry "21 x 4 = 84" at level 1; heals (muted) stay.
3. Level 0: the problem names it and the rows empty; back to 1.
4. Tiers: `fire_1` at Powers 1, 10, 50 reads 2, 20, 100; modifier 473 and Power 1: 11 (the sheet's example).
5. Database > Effects, `fire_1` Per Power 3, apply; back to the table: Fireball reads 36, 90, 150. Ctrl+Z.
6. Run `.claude/scripts/open-editor-windows.ps1`: the table reported, no `ERROR` or binding-error line.
- **Note from BT1:** the mode bar's stage 3 landed without `CatalogPage.IsReadOnly`: today a page is one
  `CatalogPage.Of("Balance table", CatalogGroup.Battle, new BalanceTableEditingViewModel(content))` line in
  `Shell/MainViewModel.Workspaces.cs`'s `CreateDatabaseWorkspace` (then "the nineteen pages" in `docs/map.md`'s
  Shell and Workspaces rows), one `pageViews.Register<BalanceTableViewModel>(edited => new BalanceTablePage(edited))`
  line in `Shell/MainWindow.Database.cs`, and one `Shell/DatabaseMenu.xaml` entry, `Command="{Binding
  ShowDatabasePageCommand}" CommandParameter="Balance table"` (the title, not a `Tag`). A read-only page fits
  `IEditedCatalog` as `Progression/ExperienceTableViewModel` does for one record (`ChosenKey` null, `Choose` keeps
  all), with `Problem` always null (the recipe's step 8): `DatabaseWorkspaceViewModel` greys Apply and refuses every
  move on it (`CanApply`, `ApplyOrRefuse`), so the boxes' line is BT1c's `BoxesProblem`, shown on the page itself.
  `Apply` records nothing; Apply and Revert stay on the bar unless BT3 adds `CatalogPage.IsReadOnly` hiding them,
  with its own tests. The session starts again on every `CatalogDraft.Current` change (decision 921), so an undo
  while the table is shown puts its boxes back to 1, 10, 20; check that with the developer. Hand check 1's numbers
  (Fireball 24, Flurry "28 x 4 = 112") are from 2026-11-15's content: read them again from today's.
- **Done, with deviations:** the menu entry is "Balance table", no "...", like every other page entry since the mode bar
  (the smoke script's `'Database>Balance table'`); no `CatalogPage.IsReadOnly`, so Apply and Revert stay on the bar,
  Apply recording nothing; the four lists are plain (unvirtualized) `ItemsControl`s, two per tab in one
  `ScrollViewer`, a few dozen rows; the page is listed after Statuses in the list and the menu. The `IEditedCatalog`
  choice tests are in `BalanceTableViewModelChoiceTests` (23 lines), beside `Problem_LevelRefused_StaysNull` and
  `Start_TierChangedInTheDraft_ReadsTheChange` (not in the plan); the Power refusal theories are merged into one.
  Lines: `BalanceTablePage.xaml` 289 (estimated 130), `.xaml.cs` 16, `BalanceTableEditingViewModel` 19,
  `BalanceTableViewModel` 198, `BalanceTableViewModelTests` 285, `BalanceTableEditingViewModelTests` 37.
