# Drop table editor
Status: approved

Drafted on 2026-11-13 from the developer's request (verbatim):

> "missing part in enemy i can create edit loot tables."

Context: the new Database > Enemies... window (plan [`2026-11-12-shops-and-enemy-database`](../done/2026-11-12-shops-and-enemy-database/README.md), ADR 0045) has a Drop table box that picks one of the draft's drop tables, but nothing in the editor creates or edits a drop table: `content/drop-tables.json` is still hand-written.

Approved on 2026-11-13 with the devil's advocate's fixes: DT5's row selection through a focus handler; the Enemies window's Edit... button made core (Q1 became decision 596); the draft's DT6 (Duplicate, the average drops) cut to the backlog, its "Rolled by" line folded into DT4 and DT5; DT0 keeps `ContentWriter.cs` whole (DT1 takes it to about 293) and takes the `HasSameRecords` extraction; `DropTable` compares by content in Core; `DropTableEditing` set in the constructor like its siblings; every question answered as a decision. The draft's DT7 is now DT6.

Builds on ADR 0011 (content the only source of truth, the editor writes through Data), ADR 0012 and 0014 (editor, catalog undo), ADR 0030 (drop tables; decision 343: each entry rolls on its own, several enemies may share a table), ADR 0045 and decisions 580-591 (the Database windows), decision 559 (a save writes only the changed catalog files).

- Numbers: decisions 596-608. No new ADR; ADR 0045 gets one amendment line (decision 602).
- Core: `DropTable` compares its entries by content (`Equals`/`GetHashCode`, as `ShopDefinition`). Data: `CatalogProblems.OfDropTables`, the writer writes `drop-tables.json`, `minQuantity`/`maxQuantity` left out at 1. No save format change: saves never name a drop table.

## Goal
1. **Drop tables are set up in the editor.** Database > **Drop tables...**: each table's id (typed while new) and its entries in order: the item, its chance in whole percents, its quantity range. The Enemies window's Drop table box then offers it.
2. **From the enemy too**: an **Edit...** button beside the Enemies window's Drop table box opens the same window on the enemy's table; a table added there is offered in the box at once (never picked for the enemy).
3. **What a table means is shown honestly**: each entry rolls on its own (Core's `DropRoll`), so the window shows no total of the chances, only a fixed note saying so; and it says which enemies roll the chosen table, since several share one.

## Order, and why
1. **DT0, shared pieces first**: `TypedNumber` from `Editor/Battle` to `Editor/Catalogs` (the drop tables are its second feature folder; left in Battle it would make `Loot` read `Battle`), the `EnabledWhenChosen` panel style, already copied in four windows, into the theme, and `CatalogDraft`'s by-content check extracted from its three copies (`HasSameRecords`), so DT1's diff is only behavior.
2. **DT1, the file saves from the editor** (Core equality, Data, the draft's setter), as the shops plan's SH1: everything after it changes tables only in the draft.
3. **DT2-DT4, the rules headless**: one table and its entries as typed, then the entries' list editing, then the window's list of tables.
4. **DT5, the window and the menu**: the first hand check of the whole loop (create a table, pick it in the Enemies window, save, play), as early as the rules allow.
5. **DT6, the button in the Enemies window**, last because it needs the window and changes how the Enemies window reads the draft; core to the request ("in enemy").

## Guiding rules
- **One rule source** (ADR 0045): the problem line is the window's own checks (a typed id's shape, a box that isn't a whole number) and then `CatalogProblems.OfDropTables`; no editor code copies `DropTableCatalogValidator`'s rules, and the editor adds none it lacks (an item may be listed twice, a table may be rolled by no enemy).
- **A file the editor writes is laid out as the writer writes it** (`.claude/rules/content.md`): DT1 relays `drop-tables.json` once, with a load-write-load test, then adds it to the identical-bytes test (`ContentWriterRepositoryTests.CatalogFileBytes`).
- **Undo covers every edit** (ADR 0014): each window's OK is one `CatalogDraft` step (decision 591).
- **Ids typed while new, fixed once in the draft; only new records removed** (decisions 581, 582, 607).
- **No command or redraw calls what throws on the user's input** (`.claude/rules/editor.md`): the window reads the boxes through `TypedNumber.Parse`, never `ToTable()`, outside OK.
- **The list view model relays typed fields only** (`.claude/rules/editor.md`): a table's id and its entries' boxes, through the table's `EntriesChanged` event; never `Label` or `Problem` notices.
- **Headless rules, thin windows**: Editor slices about 200 lines with tests counted at about 10 lines each; Data + Editor about 220.
- **Files that must not grow past 300**: `Data/Content/ContentWriter.cs` (288 → about 293 in DT1); `Shell/MainViewModel.cs` (281 → 282 in DT5, decision 605); `Shell/MainWindow.xaml` (290 → 291); `tests/Jrpg.Data.Tests/Content/ContentWriterTests.cs` (254) takes no test: `ContentWriterDropTableTests` is new.

## Out of scope (follow-ups, in `docs/plans/backlog.md`)
- Removing a saved drop table (decision 607).
- Chances finer than whole percents (decision 608): permille is a backlog line.
- Duplicate in the Drop tables window, and an average-copies-per-defeat line (decision 606): backlog lines.
- Items, affixes and encounters as editor windows.
- A read-only side-by-side table of every enemy's drops (ADR 0045's first balancing tool).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when a slice is done and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the file and the rules
- [x] **DT0. Shared pieces first: `TypedNumber` into `Editor/Catalogs`, `EnabledWhenChosen` into the theme, `CatalogDraft.HasSameRecords`** (Editor, about 120 lines moved or folded, none new) → [slices/DT0.md](slices/DT0.md)
- [x] **DT1. Drop tables save from the editor: `DropTable` equal by content, `drop-tables.json` relaid and written, quantities of 1 left out, `CatalogProblems.OfDropTables`, `CatalogDraft.SetDropTables`** (Core + Data + Editor, about 235 lines) → [slices/DT1.md](slices/DT1.md)
- [x] **DT2. One drop table as typed: its id, its entries' chance and quantity boxes, `ToTable`** (Editor, about 210 lines) → [slices/DT2.md](slices/DT2.md)
- [x] **DT3. A table's entries edited: add from every item by name, remove, move, one change event** (Editor, about 210 lines) → [slices/DT3.md](slices/DT3.md)
- [x] **DT4. The Drop tables window's rules: tables in id order, Add, Remove while new, who rolls the chosen table, the problem line, one undo step** (Editor, about 235 lines) → [slices/DT4.md](slices/DT4.md)

### Phase 2: the windows
- [x] **DT5. The Drop tables window and Database > Drop tables...** (Editor, about 170 lines) → [slices/DT5.md](slices/DT5.md)
- [x] ADR 0045 amendment: one line under Decision for the nested window. Write it from the entry under "ADRs" below (decision 602).
- [ ] **DT6. Edit... beside the Enemies window's Drop table box: the Drop tables window over it, the new tables offered at once** (Editor, about 170 lines) → [slices/DT6.md](slices/DT6.md)

## Decisions
- 596: drop tables get their own window, opened from Database > **Drop tables...** and from an **Edit...** button beside the Enemies window's Drop table box (on the enemy's table); on return the button never picks a table for the enemy. It lives in `Editor/Loot/` (Core's folder name), laid out as the Shops window: tables left, the chosen table's id and entries right, the problem line, OK greyed through `CanApply`, the session one `CatalogDraft.SetDropTables` step.
- 597: an entry is a row: its item (shown "Name (id)", fixed once added: to change it, remove the entry and add the other item, as a shop's items), then **Chance %**, **Min** and **Max** boxes kept as typed (`TypedNumber`). A new entry is added after the chosen one from a box of every item by name then id, with the tunable defaults (design, "Tunables"). Entries move Up and Down: the order is the roll's (the random draws' sequence and the drops' first-drop order), so it is kept, never sorted.
- 598: the window's own problems are the typed id (`Objects/ObjectIds`' rule, while new) and a box that isn't a whole number, the entry named as Data names it: "Drop table slime: entries[2] (potion): Chance must be a whole number (was 'x')."; each row shows its `[i]` index from 0 so Data's `entries[i]` messages point at a visible row. Then `CatalogProblems.OfDropTables` for the whole list.
- 599: no undo refusal for drop tables: only enemies name a table, and every catalog step undoes in one LIFO order, so the step where an enemy took a table always comes after the step that added it and is undone first. Holds across window sessions and through DT6's nested window (the Enemies window applies after the nested OK). Pinned by `CatalogDraftDropTableTests.Undo_AfterEnemyTookAddedTable_UndoesTheEnemyStepFirst`.
- 600: the writer leaves `minQuantity` and `maxQuantity` out of `drop-tables.json` while 1, each on its own (`AbsentWhenOneFilter`, decision 586's pattern): a single copy reads as before, and the relay drops scrap's `"minQuantity": 1`.
- 601: `drop-tables.json` is written sorted by table id, each table's entries in their order.
- 602: the Enemies window reads the draft's tables when it opens, so a table applied before shows. From its Edit... button, the nested window's OK applies its own step at once and the Enemies window takes the draft's tables in place: new ids inserted into its box's list in id order (the list is never replaced, so no combo loses its selection), its problem line rechecked against them. This amends decision 591 ("a modal window needs no re-check of the draft under it") for this one path (ADR 0045 amendment).
- 603: the Drop tables window shows which of the draft's enemies roll the chosen table ("Rolled by: goblin, goblin_chief, stone_beetle", or "Rolled by no enemy"), since a table is shared (decision 343) and editing it changes every one of them. An open Enemies window's unapplied picks are not seen (DT6).
- 604: `CatalogDraft`'s whole-catalog setters compare by content through one helper (`HasSameRecords`), extracted in DT0 from its three copies (quests, shops, enemies); `DropTable` compares its entries by content in Core (`Equals`/`GetHashCode`, as `ShopDefinition` and `QuestDefinition`), so `SetDropTables` takes the `SetShops` shape. The windows' list view models keep their few glue lines (`CanApply`, the problem relay, Remove while new) per window: they wrap the shared pieces (`RecordListProblem`, `ListOrder`) and differ in type and fields.
- 605: `MainViewModel.DropTableEditing` is a `{ get; }` property set in `MainViewModel.cs`'s constructor beside `ShopEditing` and `EnemyEditing` (281 → 282 lines), declared in `MainViewModel.Database.cs`.
- 606: the window shows a fixed note, "Each entry rolls on its own; the chances don't add up.", and no total or average; Duplicate and an average-copies line are backlog lines (an average, if it comes back, belongs in `LootFormula`, not on the record).
- 607: only tables added in the window's session can be removed (decision 581, as shops and enemies); a saved table no enemy rolls stays in the file.
- 608: chances stay whole percents, 1 to 100 (Core's `DropEntry.Chance`, the file, `LootFormula`); finer chances (permille) are a backlog line.

## Open design questions (developer decides)
None left: every question was answered at approval.

## ADRs
- No new ADR: the window follows ADR 0045. **ADR 0045 amendment** (decision 602): a Database window may open another over it; the inner window's OK is its own undo step, and the outer one then reads the draft's records it offers again. Options weighed: (a) the inner OK applies its own step, the outer refreshes (chosen); (b) the inner edits fold into the outer session as one step (one undo for both, but a new combined setter and a Cancel that drops a table the developer OK'd); (c) no nesting (the menu only). Write it from this entry.
