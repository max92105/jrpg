# IU7. The Items list and its undo step: add by category, unique included, duplicate, remove while new

Editor + Data, about 220 lines (about 110 of them tests). Needs IU6. Design: design.md, sections 2.4 and 5.1;
decisions 863, 877, 878, 885.

## Goal
`ItemCatalogViewModel` lists every item sorted by category then id, the ones added this session after; Add consumable,
Add equipment, Add unique and Add material add a new item of that category from its starting record and choose it;
Duplicate copies the chosen one, category included; Remove takes out only an item added this session. The problem line
is the chosen item's own first, then any item's by id (`RecordListProblem`), then Data's message for the items and what
reads them (`CatalogProblems.OfItems`); `Problem` is raised on every typed change, as the page's host reads it (IU8).
`ItemEditingViewModel` starts the session from the draft and applies it as one undo step (`CatalogDraft.SetItems`),
nothing when nothing changed.

## New types
- `Editor/Items/ItemCatalogViewModel` (internal sealed partial, `ObservableObject`): every item as the page lists, adds
  and removes them, and the problem line.
  - `ItemCatalogViewModel(ContentCatalog content)`; `Items` (`ObservableCollection<ItemViewModel>`), `SelectedItem`.
  - `AddItemCommand` (`RelayCommand<ItemCategory>`, the page's four buttons pass their category), `DuplicateCommand`
    (a chosen item), `RemoveItemCommand` (a chosen new item).
  - `Problem` and `ToItems()` (throws on a problem), as `DropTableCatalogViewModel`; the relay reacts to the items'
    typed fields only, never their `Label` or `Problem` notices (`.claude/rules/editor.md`). No `CanApply`:
    `CatalogDialog` greys OK on `Problem` (the mode bar's decision 937).
- `Editor/Items/ItemEditingViewModel` (internal sealed): `Start()` from `catalog.Current`, `Apply(edited)` through
  `SetItems`, as the pages' editing view models.

## Changes
- `Editor/Items/ItemCategories.StartingRecord(ItemCategory category)`: a new item's record (design.md, section 1,
  decision 885); a new unique is the new equipment's record with `IsUnique` true.
- `Editor/Catalogs/CatalogDraft.Database`: `SetItems(IReadOnlyList<ItemDefinition> items)`: one `CatalogEdit`, nothing
  when `HasSameRecords` (IU2's `ItemDefinition` equality); the summary says no refusal here (IU9 adds the map one).
  The class summary lists the items. **Recount:** 118 lines on 2026-11-15 plus row 7's steps; +15 here.
- `Data/Content/CatalogProblems.OfItems(IReadOnlyList<ItemDefinition> items, ContentCatalog content)`: public,
  `FirstProblem(() => CheckItems(items, content))` (IU1's check), its summary naming what it reads (decision 863).
  **Recount:** 130 lines on 2026-11-15, plus row 7's entries and IU1's 20; +8 here; past 250, an `.Items` partial.

## Tests
- `Editor.Tests/Items/ItemCatalogViewModelTests` (new; `ItemFixtures`):
  - `Items_Constructed_SortedByCategoryThenId` (fixtures given in the opposite order).
  - `AddItem_Equipment_AddsANewArmorLastAndChoosesIt`.
  - `AddItem_Unique_AddsANewUniqueWithNoFixedAffix`.
  - `Duplicate_ChosenUnique_AddsANewUniqueCopyLast`.
  - `RemoveItem_SavedItemChosen_CannotExecute` and `RemoveItem_NewItem_RemovesItAndChoosesItsNeighbor`.
  - `Problem_TwoNewItemsWithTheSameId_NamesIt`.
  - `Problem_PriceClearedOnAnItemAShopSells_IsDatasShopsMessage` (decision 863).
  - `Problem_TypedIdChanged_IsRaised` (the relay).
  - `ToItems_WithAProblem_Throws`.
- `Editor.Tests/Items/ItemEditingViewModelTests` (new): `Apply_NameChanged_IsOneUndoStep`,
  `Apply_NothingChanged_RecordsNoStep`, `Apply_Undo_RestoresTheItems` (its own test).
- `Editor.Tests/Catalogs/CatalogDraftItemTests` (new: `CatalogDraftTests` is at 234): `SetItems_SameRecords_RecordsNoStep`
  (a unique rebuilt with equal fixed affixes), `SetItems_FixedAffixesReordered_RecordsAStep`.
- `Data.Tests/Content/CatalogProblemsTests` (160 lines): `OfItems_ValidItems_IsNull`,
  `OfItems_MaterialStackBelowTheForgesCost_NamesRulesJson` (a material's max stack under a crafting quantity, built in
  the test).

## Verify by hand
- None: the page comes next (IU8).
