# DT1. Drop tables save from the editor

Core + Data + Editor, about 235 lines (about 150 of them tests). Design: "Core changes", "Data changes", "Content format", key decision 4. Decisions 599, 600, 601, 604. Needs DT0 (`HasSameRecords`).

## Goal
The draft can replace the drop tables as one undo step; a save writes `drop-tables.json` (relaid once) sorted by id, checked against the items as the loader checks it, with quantities of 1 left out; `CatalogProblems.OfDropTables` gives Data's message for an edited list. An Enemies window opened after the step offers the new table.

## New types
- `Data/Content/AbsentWhenOneFilter` (internal static): leaves a whole-number field out of a file while it is 1, the field's default; today `DropEntry`'s `minQuantity` and `maxQuantity` (decision 586's pattern, decision 600). `SkipOneOptionalFields(JsonTypeInfo)` through `OptionalFieldFilter.SkipWhen`, `value => value is 1`.

## Changes
- Core `Loot/DropTable`: `Equals(DropTable?)` comparing the id and the entries by content (`SequenceEqual`; `DropEntry` is a record of values) and `GetHashCode` from the id and the entry count, with the comment `ShopDefinition` carries ("a record compares a list by reference; two tables read from the same content are one table"). Its first user is `SetDropTables` below (decision 604).
- `Data/Content/ContentJsonOptions`: registers `AbsentWhenOneFilter.SkipOneOptionalFields` after the zero filter.
- `Data/Content/ContentCatalogLoader`: `DropTablesFile` becomes `internal`.
- `Data/Content/CatalogProblems`: `public static string? OfDropTables(IReadOnlyList<DropTable> tables, ContentCatalog content)` (summary: a table's own rule or an entry naming an item the content lacks; null when the loader would take them) and `internal static void CheckDropTables(...)` (`DropTableCatalogValidator.Validate(DropTablesFile, tables, content.Items)`); the class summary unchanged (it already says "for the writer and the problem line alike").
- `Data/Content/ContentWriter.cs` (288 → about 293, kept whole): `CheckCatalogs` calls `CatalogProblems.CheckDropTables(SortedDropTables(catalog), catalog)` before `CheckEnemies`; `SerializedCatalogs` gains `drop-tables.json`; `SortedDropTables` by id, ordinal. `WriteMapCatalogs`' summary names the drop tables among the catalogs.
- `content/drop-tables.json`: relaid in the writer's layout (by hand or a throwaway test, as SH1 did); the identical-bytes test proves it. The commit's content diff holds only this file.
- `Editor/Catalogs/CatalogDraft.Database.cs`: `SetDropTables(IReadOnlyList<DropTable>)` in `SetShops`' shape (`HasSameRecords` with record equality; one step, nothing when equal by content). Its summary: no undo refusal, only enemies name a table and catalog steps undo in order (decision 599). The partial's summary names the drop tables.
- `docs/plans/backlog.md`: the `ContentWriter.cs` size line (updated at approval) gets the real count after this slice.
- `docs/map.md`: the Data `Content` row names the sixth filter and `OfDropTables`; the `Catalogs` row names `SetDropTables`; the Core `Loot` row says `DropTable` compares by content.

## Tests
- Core `DropTableTests` (new file in `tests/Jrpg.Core.Tests/Loot/`; Glob on 2026-11-13 found `LootFormulaTests`, `DropRollTests`, `GearRoll…Tests`, none for the table):
  - `Equals_SameEntriesInNewLists_IsTrue` (and the hash codes equal).
  - `Equals_EntryChanceDiffers_IsFalse`.
  - `Equals_EntriesInOtherOrder_IsFalse` (the order is the roll's).
- `ContentWriterDropTableTests` (new file; Glob found none), on the test world:
  - `WriteMapCatalogs_DropTables_LoadTheSameAfterRelay` (load, write, load, every table equal by content, entries in order).
  - `WriteMapCatalogs_SingleCopyEntry_LeavesQuantitiesOut` (the goblin's `potion` entry object has neither key).
  - `WriteMapCatalogs_EntryOfTwoToThree_WritesBothQuantities` (an entry `MinQuantity = 2, MaxQuantity = 3` writes both).
  - `WriteMapCatalogs_EntryOfOneToTwo_WritesOnlyMaxQuantity` (the relaid `scrap`: the other side of the filter).
  - `WriteMapCatalogs_NewTables_WritesThemSortedWithEntriesInOrder` (tables `zeta` then `alpha` given in that order, `alpha`'s entries `scrap` then `potion`: the file lists `alpha` first, its `scrap` before `potion`).
  - `WriteMapCatalogs_UnknownDropItem_WritesNothing` (message pinned: `drop-tables.json: drop table 'goblin': entries[0]: itemId has unknown item id 'no_such_item'.`; the file's bytes unchanged).
- `ContentWriterRepositoryTests`: the identical-bytes test's `CatalogFileBytes` gains `drop-tables.json` (after the relay).
- `CatalogProblemsTests` (92 lines): `OfDropTables_TestWorldTables_IsNull`; `OfDropTables_ChanceZero_NamesTableAndEntry`; `OfDropTables_ChanceHundred_IsNull` (the accepted bound).
- `CatalogDraftDropTableTests` (new file):
  - `SetDropTables_ChangedChance_IsOneUndoStep`.
  - `SetDropTables_Undo_RestoresTheTable` (its own test, per the tests rule).
  - `SetDropTables_EqualByContent_RecordsNothing` (entries copied into new lists).
  - `Undo_AfterEnemyTookAddedTable_UndoesTheEnemyStepFirst` (add `slime`, then `SetEnemies` with goblin naming it; one undo: `slime` still there, goblin back on `goblin`; decision 599).
- `EnemyEditingViewModelTests` (50 lines): `Start_AfterSetDropTables_OffersTheNewTable` (the box lists `""`, `goblin`, `slime`: the new table after the saved one, in id order).

## Verify by hand
None: no window changes drop tables yet (DT5). `git diff content/drop-tables.json` shows only the relaid layout, and scrap's `"minQuantity": 1` gone.

- **Done, with deviations:** `drop-tables.json` relaid by a throwaway test (deleted), only layout in its diff and `scrap`'s `minQuantity: 1` gone; `ContentWriter.cs` ends at 294 lines; the Core equality test adds `Equals_OtherId_IsFalse`; `SetDropTables_ChangedChance_IsOneUndoStep` pins the raised chance and the single step; the Enemies window's test names its new table `zz_slime` so it sorts last whatever the repository adds.
