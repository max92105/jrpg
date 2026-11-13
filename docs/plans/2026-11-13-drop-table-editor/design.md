# Drop table editor: design

## Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| New entry's chance | `Editor/Loot/DropEntryRow.New` | 10 | The Chance % box of an entry just added; 100 would make every defeat drop it. Editor only, never content. |
| New entry's quantities | `Editor/Loot/DropEntryRow.New` | 1 to 1 | Min and Max of an entry just added: one copy, which the writer leaves out of the file. |

Both are starting values the developer retypes; not a game-design rule, so not a question.

## Design overview

### What exists
- **Core** `Loot/DropTable(Id, Entries)` and `Loot/DropEntry(ItemId, Chance)` with `MinQuantity`/`MaxQuantity` init properties defaulting to 1 (decision 343). `Loot/DropRoll.Roll` walks every defeated enemy's table entry by entry: each entry succeeds on its own (`ChanceRolls`, a permille roll that draws only when uncertain), then rolls its quantity (`RangeRoll`, no draw when min = max). So chances never add up to anything: a table's entries are independent, and their order is the order of the random draws and of the drops shown. `LootFormula.ChancePermille` raises a chance by drop luck, capped at 1000. `DropTable` compares its entries by reference today (a plain record); `ShopDefinition` and `QuestDefinition` already override equality by content.
- **Data** `Loot/DropTableCatalogValidator` (internal): at least one entry; each a known item, a chance within 1..100, a quantity range from 1 (a stackable's within its `maxStack`, gear uncapped, decision 345); no rule against the same item twice. `ContentCatalogLoader` reads `drop-tables.json` (`DropTablesFile`, private) once the items are loaded. `Battle/EnemyCatalogValidator` checks an enemy's `dropTableId` against the tables. `Content/CatalogProblems` (`FirstProblem` + `CheckShops`, `CheckEnemies`, run by the writer and the problem lines alike). `Content/ContentWriter` writes seven catalogs (288 lines). Five optional-field filters share `OptionalFieldFilter.SkipWhen`; none skips a 1.
- **Content** `content/drop-tables.json`: one table, `goblin`, eight entries, hand-written with each entry on one line (not the writer's layout); `goblin`, `goblin_chief` and `stone_beetle` roll it. Tests read it through the test world's copy of the repository's catalogs (`test-world`), so `goblin` is the table tests pick by id.
- **Editor** `Trade/` (the Shops window: `ShopViewModel`, `ShopCatalogViewModel`, `ShopEditingViewModel`, `ShopCatalogWindow`, `PricedItemChoice`), `Battle/` (the Enemies window; `EnemyCatalogViewModel.DropTableChoices` is built once from `content.DropTables` when the window opens; `TypedNumber`), `Catalogs/` (`CatalogDraft.Database.cs` with `SetShops`, `SetEnemies`; `SetQuests` in `CatalogDraft.cs`, the three writing out the same by-content check; `RecordListProblem`), `Events/ListOrder`, `Objects/ObjectIds`, `Shell/MainWindow.Database.cs`, `Shell/MainViewModel.Database.cs`. The `EnabledWhenChosen` panel style is copied in four windows' resources.

### Core changes
DT1: `Loot/DropTable` overrides `Equals`/`GetHashCode` to compare the id and the entries by content, in order, as `ShopDefinition` does (decision 604). `DropRoll` and the session never compare tables, so nothing in play changes; the draft's `SetDropTables` is its first user. No average or total is computed (decision 606).

### Data changes
DT1: `ContentCatalogLoader.DropTablesFile` becomes internal. `CatalogProblems.OfDropTables(IReadOnlyList<DropTable> tables, ContentCatalog content)` and the internal `CheckDropTables` (`DropTableCatalogValidator.Validate(DropTablesFile, tables, content.Items)`). `ContentWriter.cs` stays whole (288 → about 293): `CheckDropTables` runs before `CheckEnemies`, `SerializedCatalogs` gains `drop-tables.json`, `SortedDropTables` by id. `Content/AbsentWhenOneFilter` (new, the sixth filter): `DropEntry`'s `minQuantity` and `maxQuantity`, registered in `ContentJsonOptions` after `AbsentWhenZeroFilter`. The next catalog the editor writes moves the catalog list into a partial first (backlog).

### Content format
Unchanged in meaning. After the relay (DT1) the file reads:

```json
[
  {
    "id": "goblin",
    "entries": [
      {
        "itemId": "potion",
        "chance": 30
      },
      ...
      {
        "itemId": "scrap",
        "chance": 20,
        "maxQuantity": 2
      },
      ...
    ]
  }
]
```

### Structure and folder dependencies (Editor)

New folder `Editor/Loot/` (namespace `Jrpg.Editor.Loot`), named after Core's:

| Type | Responsibility |
|---|---|
| `DropEntryRow` (DT2, DT3) | One entry as typed: its item, its chance and quantity boxes, and why a box can't be read. |
| `DropTableViewModel` (DT2, DT3) | One table as edited: its id (typed while new) and its entry rows in order. |
| `DropItemChoice` (DT3) | One item the entry box offers: name and id. |
| `DropTableCatalogViewModel` (DT4, DT6) | Every table as the window edits it, and the window's problem line. |
| `DropTableEditingViewModel` (DT4) | Starts the window's state from the draft and applies it as one step. |
| `DropTableCatalogWindow` (DT5) | The window: tables left, the chosen table and its entries right. |

| From | To | Why |
|---|---|---|
| `Loot` | `Catalogs` | `CatalogDraft`, `RecordListProblem`, `TypedNumber` (moved there in DT0) |
| `Loot` | `Objects` | `ObjectIds.ProblemOf`, the id rule (as `Trade`, `Battle`) |
| `Loot` | `Events` | `ListOrder` (as `Trade`) |
| `Shell` | `Loot` | `MainViewModel`, `MainWindow.Database` open the window |
| `Battle` | `Catalogs` | `TypedNumber` after DT0 (was inside `Battle`) |
| `Battle` | nothing new | DT6's button calls back into the shell (`Action<Window, string>`), so `Battle` never names a `Loot` type |

`Catalogs` reads no feature folder (checked: `CatalogDraft.Database.cs` uses Core types only), so no cycle.

### Key decisions
1. **A window per catalog, the Shops window's shape** (decision 596): the same list-left, record-right layout, Add/Remove, the problem line and `CanApply`, so nothing new to learn; opened from the Database menu and from the Enemies window.
2. **Entries edited as rows of boxes** (decisions 597, 598): `DropEntryRow` keeps `ChanceText`, `MinQuantityText`, `MaxQuantityText` as typed; its `Problem` is only "not a whole number" (`TypedNumber.ProblemOf`); ranges, the stack cap and the unknown item are Data's, shown by the list line. Rows show `[i]` from 0, the index Data's messages use. A click into a row's box chooses the row through a `PreviewGotKeyboardFocus` handler (DT5), and the choice lives in the view model's `SelectedEntry`, which Up and Down keep.
3. **The table relays its entries through one event**: `DropTableViewModel.EntriesChanged` fires when a row's typed box changes or a row is added, removed or moved; the catalog subscribes to each table's `Id` changes and its `EntriesChanged`, never to `Label` or `Problem` (`.claude/rules/editor.md`). One subscription per table, unhooked on Remove.
4. **No undo refusal** (decision 599): `CatalogDraft` has one history for every catalog step; an enemy can only name a table once a step holding it was applied (the Enemies window checks against the draft, refreshed after DT6's nested OK), so undo always removes the enemy's pick before the table. No map, event, encounter or save names a table.
5. **The Enemies window's box list is updated in place** (decision 602, DT6): `DropTableChoices` becomes an `ObservableCollection<string>`; `TakeDropTables(ContentCatalog)` inserts ids the box lacks at their sorted place and swaps the content its problem line checks against. Replacing the `ItemsSource` would make every enemy row's combo drop its selection and push null into `DropTableId` through the two-way binding.
6. **Shared pieces first** (DT0, decision 604): the `TypedNumber` and style moves and the `HasSameRecords` extraction land before any drop table code, so DT1's diff is only behavior.

### Extension points
- A new `DropEntry` field (a condition, a rarity floor) costs one box on `DropEntryRow`, its copy in `ToEntry`, one XAML column and one test, beside the Core, Data and content work.
- Items as an editor window later: the entry box (`DropItemChoice` list) is built from the draft when the window opens, so items added in an earlier window session appear.
- Duplicate and an average-copies line (backlog): Duplicate would copy the rows' boxes as typed, never `ToTable()`; an average would be a `LootFormula` function, not a record method.

### Replacement mapping
| Today | After |
|---|---|
| `content/drop-tables.json` edited by hand, one line per entry | written by `ContentWriter` (relaid once in DT1), edited in Database > Drop tables... |
| `EnemyCatalogViewModel.DropTableChoices` (`IReadOnlyList<string>`, built once) | an `ObservableCollection<string>` updated by `TakeDropTables` (DT6) |
| the `EnabledWhenChosen` style in four windows' resources | one style in `Shell/EditorTheme.xaml` (DT0) |
| `Editor/Battle/TypedNumber` | `Editor/Catalogs/TypedNumber` (DT0, same code) |
| the "same by content" checks in `SetQuests`, `SetShops`, `SetEnemies` | `CatalogDraft.HasSameRecords` (DT0) |
| `DropTable`'s record equality (entries by reference) | equality by content (DT1) |

## Risks
- **The relay diff.** DT1's commit rewrites `drop-tables.json` (eight entries become multi-line objects, scrap loses `"minQuantity": 1`). The load-write-load test proves the meaning is kept; the developer's uncommitted content changes don't touch the file (checked on 2026-11-13: `art-sources.json`, `field-sprites.json`, `milestone.json`, `objects.json`).
- **Determinism.** Entry order is the order of the random draws; the editor keeps the order typed and the writer keeps it, so seeded tests reading the goblin's drops (Core and Game) see the same sequence. The relay changes no value.
- **Data's zero-based `entries[i]`.** Messages name an entry from 0 while people count from 1; the rows' `[i]` labels (decision 598) bridge it. If it reads badly in the hand check, the follow-up is the rows' label, not Data's message (pinned by `DropTableCatalogValidatorTests`).
- **Row selection in the entries list** (DT5): a focus trigger would lose the selection when a button takes the focus and when `ObservableCollection.Move` rebuilds a row's container; the `PreviewGotKeyboardFocus` handler and the view model's `SelectedEntry` keep it. DT5's hand check types in a row, then clicks Up twice and Remove.
- **A stale "Rolled by" line** (decision 603): from DT6's button, the open Enemies window's unapplied picks don't count. The line's tooltip says so (DT5).
- **Nested modal windows** (DT6): the Drop tables window's OK applies a step even if the Enemies window is then cancelled; that is what OK means, and Ctrl+Z takes it back. Two `ShowDialog` levels with `Owner` set keep focus right; the hand check closes both in both orders.
- **The combo's selection** (key decision 5): mitigated by updating in place; DT6 tests that the list instance is kept and each enemy's table survives `TakeDropTables`.
- **Hand edits.** A hand edit to `drop-tables.json` while the editor is open is refused at the next save that changes it (decision 559), as for every catalog.
- **File sizes.** `ContentWriter.cs` 288 → about 293 (the next catalog splits it first), `MainWindow.xaml` 290 → 291, `MainViewModel.cs` 281 → 282, `EnemyCatalogWindow.xaml` 197 → about 200, `EnemyCatalogViewModel.cs` 141 → about 160, `EnemyCatalogViewModelTests.cs` 120 (untouched: DT6's tests go to the new `EnemyDropTableRefreshTests`), `Shell/EditorTheme.xaml` 145 → about 153.
- **No save impact:** saves hold item ids and pieces, never a table id.
