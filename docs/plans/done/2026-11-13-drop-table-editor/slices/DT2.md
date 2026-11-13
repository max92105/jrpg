# DT2. One drop table as typed

Editor, about 210 lines (about 100 of them tests). Design: "Structure and folder dependencies", key decision 2. Decisions 597, 598. Needs DT0 (`Catalogs/TypedNumber`).

## Goal
A drop table read from its record into boxes and built back: its id (typed only while new), each entry's chance and quantities as typed, numbered as Data numbers them, and the row's own problem when a box isn't a whole number. No list editing yet (DT3), no window (DT5).

## New types (folder `Editor/Loot/`, new)
- `DropEntryRow` (`ObservableObject`): one entry as typed: its item, its chance and quantity boxes, and why a box can't be read.
  - `public static DropEntryRow From(DropEntry entry, string itemName)`.
  - `ItemId` (fixed, decision 597), `ItemLabel` ("Potion (potion)"), `Index` (an `[ObservableProperty]` the table sets, shown as `[2]`), `ChanceText`, `MinQuantityText`, `MaxQuantityText` (`[ObservableProperty]`, notifying `Problem`).
  - `string? Problem`: the first box in the row's order that isn't a whole number, `TypedNumber.ProblemOf("Chance", …)`, then "Min", then "Max"; null when the entry can be built.
  - `DropEntry ToEntry()`: throws `InvalidOperationException` on a problem (the window never calls it then).
- `DropTableViewModel` (`ObservableObject`): one table as edited: its id (typed while new) and its entry rows in order.
  - `public static DropTableViewModel From(DropTable table, IReadOnlyDictionary<string, ItemDefinition> items)`; `public static DropTableViewModel New()` (no id, no entries, `IsNew`).
  - `IsNew`, `Id` (notifying `Label`, `Problem`), `Label` ("goblin", or "(new table)" while empty), `ObservableCollection<DropEntryRow> Entries` (each row's `Index` its position from 0), `SelectedEntry`.
  - `string? Problem`: the typed id while new (`ObjectIds.ProblemOf(Id, [])`), then the first row's problem as "entries[2] (potion): Chance must be a whole number (was 'x')."; null otherwise.
  - `DropTable ToTable()`: throws on a problem.

## Changes
- `docs/map.md`: a new Editor row, `Loot`: "The Drop tables window's rules (ADR 0045): one table as edited, its entries' boxes kept as typed (`Catalogs/TypedNumber`); `Problem` is a typed id or a box that isn't a number, then Data's message (`CatalogProblems.OfDropTables`)", with the two types.

## Tests
- `DropEntryRowTests` (new, `tests/Jrpg.Editor.Tests/Loot/`):
  - `From_ScrapEntry_ShowsItsNumbersAsTyped` (20, 1, 2).
  - `Problem_ChanceNotANumber_NamesChance`; `Problem_MaxNotANumber_NamesMax` (the first and last box: the order matters).
  - `Problem_WholeNumbers_IsNull` (with chance 0, which Data refuses: the row checks only the shape).
  - `ToEntry_TypedQuantities_BuildsThem` (2 and 3: values that differ from the defaults of 1).
  - `ToEntry_ProblemShown_Throws`.
- `DropTableViewModelTests` (new):
  - `From_GoblinTable_ListsEntriesInFileOrder` (`potion` first, `essence` last, by id).
  - `From_Entries_NumbersRowsFromZero` (`Index` 0 and 7).
  - `Problem_NewTableWithBadId_NamesTheId`; `Problem_SavedTable_IgnoresTheIdRule`.
  - `Problem_ThirdRowChanceEmpty_NamesEntries2AndItsItem` (asserts the whole line).
  - `ToTable_EditedChance_KeepsEntryOrder`.

## Verify by hand
None: no window yet (DT5).

- **Done, with deviations:** the tests build their own items and a four-entry table (`DropTableFixtures`) instead of the repository's goblin table, which is live content; an entry naming an item the content lacks shows by its id (tested); `ToTable_Unchanged_EqualsTheTable` pins the round trip.
