# DT4. The Drop tables window's rules

Editor, about 235 lines (about 145 of them tests). Design: key decisions 1, 3. Decisions 596, 598, 603, 607. Needs DT3.

## Goal
Every table as the window edits it: the draft's tables in id order, the ones added this session after them; Add, and Remove only while new (decision 607); the box of every item; which of the draft's enemies roll the chosen table (decision 603); the problem line and `CanApply`; the session applied as one step.

## New types
- `Editor/Loot/DropTableCatalogViewModel` (`ObservableObject`): every table as the window edits it, and the window's problem line.
  - `public DropTableCatalogViewModel(ContentCatalog content)`: the draft's tables (`DropTableViewModel.From`) by id, ordinal; `OfferedItems` (`IReadOnlyList<DropItemChoice>`, every item by name then id); the first table chosen.
  - `ObservableCollection<DropTableViewModel> Tables`, `SelectedTable` (notifying `RemoveTableCommand` and `RolledBy`).
  - `AddTableCommand` (a `New()` table, last, chosen); `RemoveTableCommand` (only `IsNew`, decisions 581, 607).
  - `string RolledBy`: "Rolled by: goblin, goblin_chief, stone_beetle" from the draft's enemies naming the chosen table, by id, ordinal; "Rolled by no enemy" when none; empty for a table added this session or while none is chosen.
  - `string? Problem`: `RecordListProblem.FirstOf(Tables, table => table.Id, table => table.Problem, "Drop table")`, then `CatalogProblems.OfDropTables(...)`. `bool CanApply`. `IReadOnlyList<DropTable> ToTables()` throwing on a problem.
  - Watches each table's `Id` changes and its `EntriesChanged` event to raise `Problem` and `CanApply` (never its `Label` or `Problem` notices); unhooks a removed table.
- `Editor/Loot/DropTableEditingViewModel`: starts the window's state from the draft and applies it as one step (`Start()`, `Apply(DropTableCatalogViewModel)` through `CatalogDraft.SetDropTables`), as `ShopEditingViewModel`.

## Changes
- `docs/map.md`: the `Loot` row gains the two types and the "Rolled by" line.

## Tests
- `DropTableCatalogViewModelTests` (new):
  - `Constructor_Draft_ListsTablesInIdOrder` (a test catalog given `zeta` then `alpha`: `alpha` first).
  - `OfferedItems_Catalog_SortsByNameThenId` (two items named alike, ids in the opposite order).
  - `AddTable_Always_AppendsANewTableAndChoosesIt`.
  - `RemoveTable_SavedTable_IsGreyed`; `RemoveTable_AddedTable_RemovesIt`.
  - `RolledBy_TableTwoEnemiesRoll_NamesThemById` (a test catalog's table named by `wolf` then `bat`, given in that order: "Rolled by: bat, wolf"; not the repository's goblin table, whose users change with the content).
  - `RolledBy_TableNoEnemyRolls_SaysSo` (a test catalog's table no enemy names).
  - `RolledBy_OtherTableChosen_IsRaised`.
  - `Problem_TwoTablesSameId_NamesTheDouble`.
  - `Problem_ChanceZero_ShowsDatasMessage` (asserts the whole line).
  - `Problem_EntryChanceTyped_IsRaised`.
  - `Problem_NewTableIdTyped_IsRaisedOnce` (the table's `Label` and `Problem` notices aren't relayed).
  - `Problem_RemovedTableTyped_IsNotRaised` (the unhook).
  - `CanApply_ProblemShown_IsFalse`; `CanApply_NewTableWithOneEntry_IsTrue` (the accepted side).
- `DropTableEditingViewModelTests` (new):
  - `Start_Draft_ListsItsTables`.
  - `Apply_AddedTable_IsOneUndoStep`.
  - `Apply_AddedTable_UndoRemovesIt` (its own test).
  - `Apply_Unchanged_RecordsNothing`.

## Verify by hand
None: the window comes in DT5.

- **Done, with deviations:** the tests build their own tables (`wolf_drops`, `spare`) and enemies over the test world, emptying the enemies where "Rolled by" shouldn't see the repository's; an extra `RolledBy_AddedTable_IsEmpty` pins the empty line for a new table; `CanApply_NewTableWithOneEntry_IsTrue` also pins the built table.
