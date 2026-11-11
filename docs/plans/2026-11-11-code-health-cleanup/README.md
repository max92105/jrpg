# Code health cleanup: editor shell, content writer, maps
Status: approved

Developer's request (2026-11-11), verbatim: "it's time to do all the health check listed and do a clean up phase. dead code optimisation using clean code convention and microsoft convention. to be most human readable possible. using clear strucutre and segmentation and intention. everything work really well so no regression."

The developer approved the plan on 2026-11-11 with the devil's advocate's changes. It is the editor and Data half of the cleanup (decision 536); the Game's backlog lines are in [2026-11-11-game-code-health-cleanup](../2026-11-11-game-code-health-cleanup/README.md).

The Game plan (approved the same day, decisions 550-558) follows this one.

## Goal
At the end of this plan:
1. **The known bugs in the code the next plans touch are fixed, and the untested branch is tested:**
   - Rename folder's refused write has a test (the MF6 review);
   - a tile cell or an animation past the image is refused by the startup art check and by Import even near the int limit, the bounds written as counts (the AE1 review);
   - a zoom during a Shift + drag of the selection keeps its marks with the mouse (the MO2 review).
2. **No editor file the next plans touch is over 300 lines or at the limit:** `MainWindow.xaml.cs` (425), `MainViewModel.cs` (340), `CatalogDraft.cs` (298) and `ObjectPlacingViewModel.cs` (293) are split along their features, by pure moves.
3. **No dead member and no duplicate helper is left in the maps' code:** every public or internal member of Data and the editor with no caller is listed with its fate (`ContentWriter.WriteMap` deleted); one `MapLayout.AllEvents`, one enum JSON name rule, one `SheetCell.Of`, one copied-content fixture in the writer tests.
4. **The lines that do two things in the Shell, the editor's Maps folder and the art check each say one**, and `verify.ps1` refuses a changed code line over 130 columns from then on (decision 539).
5. **`docs/plans/backlog.md` says, for every line this plan doesn't do, why it waits**, and the lines found already done are gone.

## Order, and why
1. **Bugs and the untested branch (B1-B3).** Each comes before the split or the cleanup of its file: B1 before R1 rewrites `TryRenameFolder`; B3 before S1 moves `ApplyViewport` into `MainWindow.Viewport`. B1 also deletes the backlog lines found already done or closed by the developer's answers, so later slices start from a true list.
2. **Splits (S1-S5).** Pure moves, each its own slice, read with `git diff --color-moved`. The window first (S1, S2), since S2 moves the sheet import out of both `MainWindow` and `MainViewModel`; then `MainViewModel`'s map settings (S3); then the two files at the limit (S4, S5).
3. **Dead code and duplicates (X1, D1, D2).** X1 first, so a duplicate isn't merged into a member about to be deleted, and its `WriteMap` test moves land before D2 changes the same test files' fixture.
4. **Readability (R1, V1, R2, R3).** Last, on the files as they stay. V1, the developer's `verify.ps1` edit, comes before R2 so R2 and R3 run under the new check.

Each phase ends green: every slice builds, passes every test, and passes `verify.ps1`.

## Guiding rules
- **A refactor slice changes no behavior.** The guard is the test runner's total per project (`dotnet test --solution Jrpg.slnx`) before and after, equal, plus the list of moved test names in the Done note; never a count of test methods (a Theory runs several cases). A moved test keeps its body except for a mechanical prefix on a shared fixture.
- **A bug slice shows its test can fail** (decision 538). B2's tests fail before the fix; B1 tests correct code, so the slice shows the test failing with the branch's `return false` deleted for one run. Window code (B3) is reproduced by hand before and after; no type is added only to test window code.
- **A move is only a move** (decision 541). A moved comment over 120 columns is rewrapped and a moved code line over 130 columns wrapped (both are changed lines to `verify.ps1`); nothing else changes. A split never fixes what it finds: it notes it in `backlog.md`.
- **A readability change touches only a line that does two things** (decision 539): it gets two named steps. A line that is only long waits for the next slice that changes it, which V1's check then makes wrap it. A player-facing or validator message is wrapped only by splitting its interpolated string with `+`, never rebuilt, unless a test pins it with `Assert.Equal` first.
- **One slice owns a file at a time**, in the order above; a later slice works on the file as the earlier one left it.
- **Each slice** removes or rewrites the `backlog.md` lines it covers, updates `docs/map.md` when it adds a partial file or a type (the Shell row lists the partials by name), and stays within `verify.ps1` (300 lines, 4 parameters, 120-column comments, and after V1 130-column code).
- **No NuGet package, nothing in `content/`, no Core change.** The one behavior change is R1's (decision 544): a state the modal composer rules out throws instead of skipping.

## Out of scope
- **The Game's backlog lines**: the second plan, [2026-11-11-game-code-health-cleanup](../2026-11-11-game-code-health-cleanup/README.md).
- **Everything the next editor and Data plans won't touch first**, which stays in `backlog.md` with its reason (design.md, "Backlog survey"). Among them:
  - the Data validators' repeated checks (the six "lists 'x' more than once", `WholeChance` three times);
  - `TileBrowserViewModel` (332 lines) and its `SheetGrid`; `TileBrowserViewModelTests` (496) and `ObjectShapesViewModelTests` (347), Editor test files outside this plan's scope rule (decision 536);
  - `MainWindow.xaml` (293) and `EditorTemplates.xaml` (297), which the backlog's own rule already guards: the next control goes in its own user control, the next template in its own dictionary;
  - the `ObjectIds` to `ContentIds` rename and its message;
  - the `Directory.Build.props` merge (decision 144, a protected file).
- **New features named in the backlog:** the New NPC dialog's sprite check, the editor's map animating, a voice-file check on the editor's save, a depth offset, `ContentWriter.RenameMap`'s delete failure naming the pair to delete (ADR 0042 accepts the state).
- **A repository-wide line rewrap** (decision 539).

## Slices

One line per slice; each slice's goal, files, moves and checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Backlog survey, design overview and risks: [design.md](design.md).

Sizes are hand-written lines, tests included; "moved" lines are cut and pasted (`git diff --color-moved`) and counted apart.

### Phase 1: bugs and the untested branch
- [x] **B1. Rename folder's refused write is tested; done backlog lines deleted** (Editor tests, docs, about 35 lines) → [slices/B1.md](slices/B1.md)
- [x] **B2. Tile cells and animations past the image are refused, as counts** (Data, about 70 lines) → [slices/B2.md](slices/B2.md)
- [x] **B3. A zoom mid-drag keeps the selection's marks** (Editor window, about 5 lines, hand check) → [slices/B3.md](slices/B3.md)

### Phase 2: splits (pure moves)
- [x] **S1. Moves only: the map view's pan and zoom into `MainWindow.Viewport`** (Editor, about 125 lines moved) → [slices/S1.md](slices/S1.md)
- [x] **S2. Moves only: the sheet import into `MainViewModel.SheetImport` and `MainWindow.SheetFiles`** (Editor and tests, about 110 lines moved) → [slices/S2.md](slices/S2.md)
- [ ] **S3. Moves only: the map settings into `MainViewModel.MapSettings`** (Editor and tests, about 110 lines moved) → [slices/S3.md](slices/S3.md)
- [ ] **S4. Moves only: the catalog snapshots into `CatalogDraft.Snapshots`** (Editor, about 35 lines moved) → [slices/S4.md](slices/S4.md)
- [ ] **S5. Moves only: the selection into `ObjectPlacingViewModel.Selection`** (Editor, about 75 lines moved) → [slices/S5.md](slices/S5.md)

### Phase 3: dead code and duplicates
- [ ] **X1. Dead code with evidence; `ContentWriter.WriteMap` deleted** (Data, Editor, Data tests, about 90 lines) → [slices/X1.md](slices/X1.md)
- [ ] **D1. One `MapLayout.AllEvents`, one enum JSON name, one `SheetCell.Of`** (Data, Editor, about 45 lines) → [slices/D1.md](slices/D1.md)
- [ ] **D2. The writer tests copy content through `CopiedContentRoot`** (Data tests, about 60 lines, net fewer) → [slices/D2.md](slices/D2.md)

### Phase 4: readability
- [ ] **R1. Shell: the folder rename's write extracted; the composer's ruled-out states thrown** (Editor, about 70 lines; one behavior change on a bug path) → [slices/R1.md](slices/R1.md)
- [ ] **V1. `verify.ps1` refuses a changed code line over 130 columns (the developer's edit)** (script, about 10 lines) → [slices/V1.md](slices/V1.md)
- [ ] **R2. Editor Maps: the lines that do two things** (Editor, about 50 lines) → [slices/R2.md](slices/R2.md)
- [ ] **R3. The art check's lines that do two things; the backlog closed** (Data, docs, about 50 lines) → [slices/R3.md](slices/R3.md)

## Decisions
- **536. Two cleanup plans** (the developer's scope (a)): this one is the editor and Data phase, scoped to what the next plans touch first (the editor shell, the content writer, the maps); [2026-11-11-game-code-health-cleanup](../2026-11-11-game-code-health-cleanup/README.md) covers the Game's backlog lines. Every other line stays in `backlog.md` with its reason.
- **537. The refactor guard is the runner's total per project, before and after, with the moved test names listed**; never a count of test methods. A moved test changes only by a mechanical prefix.
- **538. Bugs before the refactor of their file, each showing its test can fail** (B1 by deleting the branch's `return false` for one run, B2 before its fix); window code by hand (B3).
- **539. The line rule:** code lines at most 130 columns, comments at most 120. `verify.ps1` refuses a changed code line over 130 columns as it refuses a changed comment over 120; the developer makes the script edit (V1, the script is protected). No repository-wide rewrap: about 250 Editor and 140 Data lines run past 130 today, and they wrap when a slice next changes them. A readability slice changes only lines that do two things; a message is wrapped by splitting its interpolated string with `+`, unless pinned by `Assert.Equal` first.
- **540. The names:** partial files `MainWindow.Viewport`, `MainWindow.SheetFiles`, `MainViewModel.SheetImport`, `MainViewModel.MapSettings`, `CatalogDraft.Snapshots`, `ObjectPlacingViewModel.Selection`; test class `MainViewModelSheetImportTests`; the test helper `Data.Tests/Content/MapContent.With`; `ContentJsonOptions.EnumName`; `MainViewModel.MapsMoved` renamed `MovedMaps`, and the extracted `TryWriteMovedMaps`.
- **541. A move wraps what it moves**: a moved code line over 130 columns is wrapped and a moved comment over 120 rewrapped; no other edit rides along.
- **542. B2 writes the art check's bounds as counts**: a cell is inside when `column < sheet.ColumnsIn(width)` and `row < sheet.RowsIn(height)`, an animation when `frameCount <= sheet.ColumnsIn(width) - column`; `CellLeft`'s pixel arithmetic leaves the check (same result for every in-range value). The messages keep their text; the last frame's column is printed from a `long`.
- **543. `CatalogDraft.SetQuests`'s `ToDictionary` stays.** The Quests window greys OK on a repeated id (`QuestCatalogViewModel.Problem`), and the editor rule says a view model doesn't guard a state its window rules out: a repeated id there is a bug that should throw. The backlog line is closed.
- **544. `MainViewModel.ObjectFromMap`'s guards:** `StartObjectFromMap`'s `CurrentMap is not { } shown` stays (content with no map opens none, so it is reachable); `TryApplyObjectFromMap`'s `_mapSelectionSource is { } source && CurrentMap is { } map` become values (`_mapSelectionSource!`, `CurrentMap!`). This is a behavior change on a bug path: a state the modal composer rules out now throws instead of skipping the replacement (R1).
- **545. `MainViewModelTests` needs no second split after S2 and S3**: it ends near 196 lines, so its "part 2" backlog line is closed.
- **546. A session folder stays listed for the session**, a map moved in or not (the developer's answer; a note on decision 534, "a new folder lives for the session until a map moves in"): moving the map out again, or undoing the move, leaves the folder in the tree, empty. No code; B1 deletes the backlog line.
- **547. The map settings and the sheet import leave `MainViewModel.cs`** (S2, S3; the developer took the recommendation), which ends near 258 lines; the New NPC members stay.
- **548. `ContentWriter.WriteMap` is deleted** (X1): no production caller since the editor saves through `WriteMaps` (decision 518); its 20 test calls move onto `WriteMaps(MapContent.With(catalog, map), [map.Id])`, which runs the same checks.
- **549. The enum JSON name rule lives in `ContentJsonOptions`** (D1): `EnumName<T>(value)` reads the same naming policy the serializer's enum converter is built from, so a message and the file spell a value alike.

## Open design questions (developer decides)
None: the developer answered them on approval (decisions 536, 539, 546, 547).

## ADRs needed
None: the plan moves and renames code under existing decisions, and the line rule is a convention the script enforces (decision 539).
