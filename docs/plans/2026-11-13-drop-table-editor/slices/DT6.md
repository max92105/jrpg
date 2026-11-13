# DT6. Edit... beside the Enemies window's Drop table box

Editor, about 170 lines (about 80 of them tests). Design: key decision 5, "Structure and folder dependencies" (Battle never names a Loot type), "Risks" (nested modal windows, the combo's selection). Decisions 596, 599, 602. Needs DT5. Before it: the ADR 0045 amendment step in the README. Core to the request ("in enemy"), not cuttable.

## Goal
In the Enemies window's Stats tab, an **Edit...** button beside the Drop table box opens the Drop tables window over it, on the chosen enemy's table. Its OK applies its own undo step at once; back in the Enemies window the box offers every table the draft now holds, each enemy keeps the table it had (nothing is picked for it, decision 596), and the problem line checks against the new tables.

## Changes
- `Editor/Battle/EnemyCatalogViewModel`:
  - `DropTableChoices` becomes an `ObservableCollection<string>` (still `""` first, then ids in order); `_content` is no longer `readonly`.
  - `public void TakeDropTables(ContentCatalog current)`: inserts each table id the list lacks at its ordinal place (never replacing the list: the combos keep their selection, design key decision 5), takes `current` as what its problem line checks against, and raises `Problem` and `CanApply`. The class summary says the draft is taken again after a nested Drop tables OK (decision 602).
- `Editor/Battle/EnemyEditingViewModel`: `RefreshDropTables(EnemyCatalogViewModel edited)` (`edited.TakeDropTables(catalog.Current)`); its summary names the second job.
- `Editor/Loot/DropTableCatalogViewModel`: `ChooseTable(string id)`: chooses the table with that id; the first stays chosen when no table has it (an enemy with none passes `""`).
- `Editor/Battle/EnemyCatalogWindow` (`.xaml` 197 → about 200; `.xaml.cs`): an **Edit...** button after the Drop table box (tooltip "Create or edit drop tables; a table added there is offered here"), and a fourth constructor parameter `Action<Window, string> editDropTables` (the owner and the chosen enemy's table id), called by `OnEditDropTablesClicked` with `this`. The window never names a `Loot` type.
- `Editor/Shell/MainWindow.Database.cs`: the Drop tables opening extracted into `bool EditDropTables(Window owner, string chosenTableId)` (start, `ChooseTable`, show with that owner, apply on OK, return whether it applied), called by `OnEditDropTablesClicked` with `this` and `""`, and by the Enemies window's callback, which then calls `enemyEditing.RefreshDropTables(edited)`.
- `docs/map.md`: the Editor `Battle` row names the button and `TakeDropTables`.

## Tests
- `EnemyDropTableRefreshTests` (new sibling of `EnemyCatalogViewModelTests`, which stays at 120 lines):
  - `TakeDropTables_TableAdded_InsertsItInIdOrder` (the draft gains `aardvark`, which sorts before `goblin`: the list reads `""`, `aardvark`, `goblin`; an append would fail).
  - `TakeDropTables_TableAdded_KeepsTheSameList` (`Assert.Same` on the collection: the box's source isn't replaced).
  - `TakeDropTables_TableAdded_KeepsEachEnemysTable` (goblin still names `goblin`).
  - `TakeDropTables_EnemyNamingTheNewTable_ClearsTheProblem` (goblin's box set to `slime` before: Data's unknown-table message; after `slime` is added and taken: null).
  - `TakeDropTables_Always_RaisesProblemAndCanApply`.
  - `RefreshDropTables_AfterSetDropTables_OffersTheNewTable` (through `EnemyEditingViewModel`, on a draft given `SetDropTables` after `Start`).
- `DropTableCatalogViewModelTests` (DT4's file): `ChooseTable_KnownId_ChoosesIt` (a test catalog with `alpha` and `zeta`: `zeta`, not the first); `ChooseTable_EmptyId_KeepsTheFirst` (the no-op's test).

## Verify by hand
1. `dotnet run --project src/Jrpg.Editor`, **Database > Enemies...**, choose `goblin`, Stats tab: beside **Drop table** (showing `goblin`) click **Edit...**: the Drop tables window opens over the Enemies window with `goblin` chosen.
2. **Add table**, id `slime`, pick **Potion (potion)**, **Add entry**, **OK**: the Drop tables window closes; goblin's box still shows `goblin`; open the box: `slime` is listed after `goblin`.
3. Pick `slime` for goblin, **OK**.
4. **Ctrl+Z** once: reopen **Database > Enemies...**: goblin's table is `goblin` again, and the box still offers `slime` (the table step is older). Close with **Cancel**. **Ctrl+Z** again: **Database > Drop tables...** no longer lists `slime`. **Ctrl+Y** twice.
5. Cancel path: **Database > Enemies...**, **Edit...**, add table `bat_loot` with one entry, **OK**, then **Cancel** the Enemies window: **Database > Drop tables...** still lists `bat_loot` (its own OK applied it); **Ctrl+Z** removes it.
6. Focus: with the Drop tables window open over the Enemies window, click the main window: the Drop tables window stays in front; closing it (OK or Cancel) returns to the Enemies window, not the map.
7. Revert with `git checkout -- content` if this was saved only as a check.
