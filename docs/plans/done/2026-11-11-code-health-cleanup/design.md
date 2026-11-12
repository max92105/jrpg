# Design: code health cleanup (editor and Data)

## Tunables and placeholders
None. The plan changes no value a player or the developer sees; the one convention it sets is decision 539's line length (130 columns for code, 120 for comments), enforced on changed lines by `verify.ps1` from V1.

## Design overview

### What exists (2026-11-11 counts)
Files over 300 lines or within ten of it, in the projects this plan covers:

| File | Lines | Grows through | This plan |
|---|---|---|---|
| `Editor/Shell/MainWindow.xaml.cs` | 425 | partials only (backlog rule) | S1, S2 → about 265 |
| `Editor/Shell/MainViewModel.cs` | 340 | partials only (backlog rule) | S2, S3 → about 258 |
| `Editor/Tiles/TileBrowserViewModel.cs` | 332 | the sheet view's features | waits (backlog, `SheetGrid`) |
| `Editor/Catalogs/CatalogDraft.cs` | 298 | every new catalog step | S4 → about 263 |
| `Editor/Shell/EditorTemplates.xaml` | 297 | a new control's template | waits (its rule: a new dictionary) |
| `Editor/Shell/MainWindow.xaml` | 293 | a new control | waits (its rule: a new user control) |
| `Editor/Objects/ObjectPlacingViewModel.cs` | 293 | every placing gesture | S5 → about 222 |
| `Data/Content/ContentWriter.cs` | 263 | every new write | X1 → about 240 |
| `Editor.Tests/Tiles/TileBrowserViewModelTests.cs` | 496 | | waits (decision 536) |
| `Editor.Tests/Objects/ObjectShapesViewModelTests.cs` | 347 | | waits (decision 536) |
| `Editor.Tests/Shell/MainViewModelTests.cs` | 292 | | S2, S3 → about 196 |
| `Editor.Tests/Shell/MainViewModelOpenMapsTests.cs` | 286 | | waits (its rule: a sibling file) |

Core has no file over 290 (`GameSession` 289, `BattleState` 283) and no change in this plan. The Game's files are the second plan's.

### Dead code survey
How each kind was searched on 2026-11-11, and what X1 adds:
- **`TODO`, `FIXME`, "obsolete", compatibility branches, commented-out code:** `Grep "TODO|FIXME|HACK|[Oo]bsolete|backward[- ]compat"` (the retired word is already refused by the verify script) and `Grep "^\s*//\s*(var |if \(|return |foreach|[A-Za-z_.]+\(.*\);\s*$|\}\s*$)"` over `src/` and `tests/`: no match.
- **Interfaces with one implementation:** every `interface` declaration in `src/` (`Grep "^\s*(public|internal) (partial )?interface"`), then its implementers (`Grep ":\s*(.*\b)?IName\b"`): `IBattleListChoice`, `IMenuPage`/`IMemberPage` and `ISaveFilesPage` have two each; `IScene`, `IMapEdit` and `IRandom` several. None to remove.
- **Members with no caller:** on 2026-11-11 only the shell's `Can...`/`Has...` properties were checked (each bound in `MainWindow.xaml`), which proves nothing about the rest. X1 does it in full: every `public` or `internal` member declared in `src/Jrpg.Data` and `src/Jrpg.Editor`, each name grepped as a word across `src/` (`.cs` and `.xaml`) outside its own file. One is already known: `ContentWriter.WriteMap` has no caller in `src/` (`Grep "\.WriteMap\("`: 20 calls, all in 8 `Data.Tests/Content/ContentWriter*Tests` files), since the editor saves through `WriteMaps` (decision 518).
- **Forwarding wrappers:** `ObjectPlacingViewModel.SelectObjectAt` forwards to `Picking.PickAt`, but the window and 28 test sites call it as the placing facade; removing it is churn with no reader gain. Left as is.
- **Duplicated helpers:** `MapLayout.AllEvents` written out again three times in Data (`MapEventValidator.AllEvents`, `MapEventSteps.Of`, `WorldStateRepair`); the enum-to-camelCase-JSON-name one-liner five times (`StatBonusesConverter`, `ResistancesConverter`, `CraftingRulesValidator.NameOf`, `LootRulesValidator.NameOf`, `MapEventValidator.JsonNameOf`); `new SheetCell(tile.SheetId, tile.Column, tile.Row)` twice in `TileImages` and once in `TileBrowserViewModel` beside `SheetCell.Of`; the test world's copy loop in `ContentWriterTests.CopyFolder` and `ContentWriterShopTests` beside `CopiedContentRoot`; `WithMap(catalog, map)` privately in `ContentWriterRenameTests` and `ContentWriterMapsTests`. D1, D2 and X1 take them.

### The code reviewer's recurring smells
- **Records holding lists compared by reference:** no live instance in the Shell or Maps. `QuestDefinition` (used by `CatalogDraft.SetQuests`' compare) overrides `Equals`; `MapTree.Shown`, `SheetUsage`, the `*Edit` records and `MapSelectionSource` are never compared.
- **Stale comments:** `MainViewModel`'s class summary still says "the maps to choose from and the open one, which it opens, saves and closes" since maps stay open (decision 497): S3 rewrites it. `MainViewModelTests`' summary lists "map settings", which S3 moves out. `MainWindow`'s summary says pan and zoom are "done here": S1 keeps it true.
- **Detached comments:** every move slice reads the context line above every moved member in `git diff`.
- **The last review's open items (MF6):** `MapNavigation.Follow` already removes then adds (fixed); `MapFolderChoiceViewModel.Problem` no longer nests a ternary (fixed); `docs/map.md`'s `MapListPanel` entry names Rename folder (fixed); `TryRenameFolder`'s refused write is B1; MF4's long lines are R1.
- **`docs/map.md`:** its Shell row doesn't name the window's existing partials (`.TilePainting`, `.ObjectEditing`, `.ObjectDragging`, `.ObjectCopying`, `.EventEditing`, `.TileAnimation`); S1 adds them with `.Viewport`.

### Core changes
None.

### Structure

New partial files (decision 540), each one feature of its class:

| File | Holds (moved from) |
|---|---|
| `Editor/Shell/MainWindow.Viewport.cs` | the map view's pan and zoom: the step constants, the pan and fit state, `HandleZoomKey`, `HandleViewportKey`, the Fit and Actual size handlers, `OnViewportSizeChanged`, `StartPanning`, `OnViewportMouseWheel`, `RefitMap`, `FitMap`, `ZoomAtViewCenter`, `ApplyViewport`, `CellAt`, `MapPointAt` (`MainWindow.xaml.cs`) |
| `Editor/Shell/MainWindow.SheetFiles.cs` | the Tiles panel's file questions: `OnImportSheetImageClicked`, `RedrawImportedImages`, `OnRemovalChanged` (`MainWindow.xaml.cs`) |
| `Editor/Shell/MainViewModel.SheetImport.cs` | Tiles > Import sheet images: `ImportStatus`, `SheetImagesImported`, `ImportSheetImages` (`MainViewModel.cs`) |
| `Editor/Shell/MainViewModel.MapSettings.cs` | Map > Map settings: `StartEditingSettings`, `TryApplySettings`, `TryResize` (`MainViewModel.cs`) |
| `Editor/Catalogs/CatalogDraft.Snapshots.cs` | the catalog with one record added, replaced or removed: `WithTile`, `WithTiles`, `WithoutTile`, `WithObject`, `WithoutObject`, `WithTilesheet`, `WithoutTilesheet` (`CatalogDraft.cs`); `IsSameObject` stays beside `SetObject`, a comparison, not a snapshot |
| `Editor/Objects/ObjectPlacingViewModel.Selection.cs` | the selection: `SelectedObjectUids`, `SelectObjectsIn`, `SelectObjectAt`, `ClearSelection`, `Select`, `IsSelected`, `MoveSelectedObjects`, `MoveSelectedObjectsOnGrid`, `DeleteSelectedObjects` (`ObjectPlacingViewModel.cs`); the open map's upkeep (`OnOpenMapChanged`, `OnOpenMapContentChanged`) stays beside `OpenMap` |

New members and test helpers:
- `Data/Content/ContentJsonOptions.EnumName<T>(T value)`: an enum value's name as content JSON writes it (`EnumName(StatKind.MaxHp)` is `"maxHp"`), read from the same naming policy the serializer's `JsonStringEnumConverter` is built with. D1.
- `Data.Tests/Content/MapContent.With(catalog, map)` (test-only, static): the content with one map added or replaced, for the writer tests; replaces the two private `WithMap` copies. X1.
- New test class `Editor.Tests/Shell/MainViewModelSheetImportTests` (S2).

### Folder dependencies
No new namespace edge, and no new cycle beyond those Data already has. Data's `Content/` folder is its hub and already reads the feature folders: `ContentJsonOptions` reads `Battle` and `Items` (their converters), `ContentWriter` reads `Art`, `Maps` and `Quests`, `GameRulesValidator` reads `Loot`, `ContentCatalogLoader` reads every validator folder. The feature folders read `Content/` back (`ContentValidationException`, `RangeProblems`), so `Battle` ↔ `Content` and `Items` ↔ `Content` are already two-way. D1 has `Battle/ResistancesConverter`, `Items/StatBonusesConverter`, `Crafting/`, `Loot/` and `Maps/` read `ContentJsonOptions.EnumName`: each of those folders already reads `Content/`, so no edge is added. `MapLayout.AllEvents` is Core's `Maps`, which Data's `Maps/` and `Saving/` already read; `SheetCell.Of` is the editor's `Catalogs`, which `Tiles` already reads.

### Key decisions
- **Pure moves by partial file, not new types**, for `MainWindow`, `MainViewModel`, `CatalogDraft` and `ObjectPlacingViewModel`: that is how both shell classes have grown since the map editing flows plan (`.Saving`, `.MapFolders`, `.TilePainting`), the members share the class's fields and bindings, and a new type would change the XAML bindings and every test site.
- **A generator hook stays with its property** (S5): `partial void OnOpenMapChanged` beside `[ObservableProperty] OpenMap`, so a reader finds the property's side effects where it is declared.
- **B2 bounds by counts, keeps the messages** (decision 542).
- **The enum name rule beside the serializer** (decision 549), not a new type: one place spells a value for files and messages alike.
- **D2 stops at the test-world copies**: `ContentCatalogLoaderShopTests` and `ContentCatalogLoaderCraftingTests` copy only the repository's `*.json`, while `CopiedContentRoot` copies every file; on the repository that means the music (6 to 23 MB per loop). They keep their loop, and their backlog line says why.

### Replacement mapping
| Today | After |
|---|---|
| `ContentWriter.WriteMap(map, catalog)` (tests only) | `WriteMaps(MapContent.With(catalog, map), [map.Id])` |
| `WithMap` in `ContentWriterRenameTests` and `ContentWriterMapsTests` | `MapContent.With` |
| `MapEventValidator.AllEvents(layout)`, `MapEventSteps`' inline concat, `WorldStateRepair`'s inline concat | `layout.AllEvents` (Core) |
| `JsonNamingPolicy.CamelCase.ConvertName(x.ToString())` in five Data files | `ContentJsonOptions.EnumName(x)` |
| `new SheetCell(tile.SheetId, tile.Column, tile.Row)` (3) | `SheetCell.Of(tile)` |
| `ContentWriterTests.CopyFolder`, `ContentWriterShopTests`' loop | `CopiedContentRoot` |
| `ArtReferenceCheck.IsCellInside`'s pixel sums and `LastFrameColumn` in the check | count bounds (decision 542) |
| `MainViewModel.MapsMoved` | `MovedMaps` |
| `TryRenameFolder`'s inline write | `TryWriteMovedMaps` |

### Extension points
None new. The partial files are where each feature grows next: a new map view gesture in `MainWindow.Viewport`, a new settings field's apply in `MainViewModel.MapSettings`, a new catalog record in `CatalogDraft.Snapshots`.

## Backlog survey
Every `docs/plans/backlog.md` line about the editor and Data, checked against the code on 2026-11-11. The Game's lines are surveyed in the second plan's design.

### Already done, or closed by the developer: B1 deletes these lines
1. "When next touched": "`MainViewModel` (484 lines ...): the sheet image-size cache (`SheetImageSizes`) and the import status text (`SheetImportResult.Summary`) could leave it." Both are their own types now (Mana Seed plan, X1).
2. Mana Seed reset: "The `MainViewModel` line above (image-size cache, import status) is done by the Mana Seed plan's X1, which needs the room."
3. Object composer plan: "Several tiles on one object cell (decision 456): taken by the object levels and sheet zoom plan" (done, ADR 0040).
4. Map editing flows: "`Shell/MainWindow.ObjectEditing.cs` lines 24 and 28 mix `Composing?.StartNew()` with `Composing!.TryApply`": both now read `Composing!.StartX()` then the flow-typed `Composing.TryApply`.
5. Map editing flows: "`ErasedCells` zips two `Select`s over the same tiles, and `ErasedOn` reads `TileIdAt` three times for one cell": `ErasedOn` reads it once into a tuple, and `ErasedCells` filters the layers by a dictionary.
6. Map fixes: "A session folder (decision 534) stays listed after its map moves out again (MF2, MF4): decide whether it should go." Decided: it stays (decision 546).

Also already done, never in the backlog: the MF6 review's `MapNavigation.Follow` order, `MapFolderChoiceViewModel.Problem`'s nested ternary and `docs/map.md`'s `MapListPanel` entry.

### Done by this plan (each slice removes or rewrites its lines)
| Backlog line | Slice |
|---|---|
| `MainViewModel.TryRenameFolder`'s refused-write branch has no test (MF6) | B1 |
| `Data/Art/ArtReferenceCheck.LastFrameColumn` wraps near the int limit (AE1) | B2 |
| A wheel zoom during a Shift + drag of the selection snaps the outlines back (MO2) | B3 |
| Editor: releasing Space over a focused button clicks it (demo retirement, F2) | B3 checks it by hand: `OnViewportMouseDown` now focuses the map, and a WPF button drops its Space press when it loses focus, so the line is expected gone; if it still clicks, B3 handles the Space up |
| `MainWindow.xaml.cs` (426): the viewport into a partial; the sheet import and removal handlers could join partials (two lines) | S1, S2 |
| `MainViewModel.cs` is 340 lines | S2, S3 |
| `MainViewModelTests`, part 2 | S2, S3 (decision 545) |
| `Catalogs/CatalogDraft` at 298: the `With...`/`Without...` helpers into a partial (two lines) | S4 |
| Files at the limit: `CatalogDraft.cs` 298, `ObjectPlacingViewModel.cs` 293 (and `EditorTemplates.xaml`, `MainWindow.xaml`) | S4, S5; R3 rewrites the line to the two XAML files and their rule |
| Data enumerates a layout's events three times | D1 |
| The enum-to-camelCase-name one-liner five times | D1 |
| A tile's sheet cell still worked out inline (`SheetCell.Of`) | D1 |
| `CopiedContentRoot` exists; older copy loops (two lines) | D2, the test-world copies; the line is rewritten for the two loader tests (reason above) and `EditorTestContent` (Editor.Tests can't see Data.Tests' internal fixture) |
| `ContentWriterTests.cs` (289): new tests in sibling files | D2 rewrites the count |
| `MainViewModel.MapFolders` lines run to 150-170 columns | R1 |
| `MainViewModel.ObjectFromMap` guards states the modal composer rules out | R1 (decision 544: one of the two guards is reachable and stays) |
| Comment lines over 120 columns from before the rule: `ArtReferenceCheck` (2) | R3; the line keeps `ExplorationState`, `GameSession`, `MessageIds` (the last for the Game plan) |
| `CatalogDraft.SetQuests`' `ToDictionary` throws on a repeated id | closed by decision 543 (R3 removes the line) |

### Stays, with its reason (R3 checks each still reads true)
- **Editor, not touched first by the next plans, or outside decision 536's scope:** `TileBrowserViewModel` (332) and its `SheetGrid` (the sheet's hit test changes only with a sheet feature); `TileBrowserViewModelTests` (496, rewritten as one line: split into Cut and re-cut classes over a shared setup when the tile browser is next touched); `ObjectShapesViewModelTests` (347; the shapes window is not on the next plans' path); the New NPC dialog's sprite check (a feature); `TileImages.Forget` and field sprites (no sprite import yet); `ObjectComposing.StartNew`'s column clip test (needs a wide test sheet); the "chosen cells' sheet has its image" query (two copies; a third moves it); `ObjectIds.ProblemOf` to `ContentIds` and its "(like tree_4)" message (the next Quests or objects change); `MainViewModelOpenMapsTests` 286 and `MainViewModelObjectFromMapTests` 251 (their rule: a sibling file); the editor's map animating and the levels' leftovers (features); `ContentWriter.WriteMaps` not checking the files events name (stills are art, which decision 442 lets an editor save miss; the voice half is a design call); `ContentWriter.RenameMap`'s delete failure (ADR 0042 accepts the state); `MainWindow.xaml` and `EditorTemplates.xaml` (rewritten into one line with their rule).
- **Data:** the six "lists 'x' more than once" checks and `NewGame`'s repeated entry checks (each copy is three lines in a validator the next plans don't open); `WholeChance` three times; `ResourceDefinition`'s nullable `Max`/`StartsAt`; `ClassCatalogValidator`'s requirement against the definition max; `SaveGameRepair`'s worn bonuses and saved positions; `ShopStockRepair`'s level cap; equipment's unread `maxStack`; `SoundSourcesTests`' loose options and output-copy reads; `ContentCatalogLoaderTests`' "potion" comment (waits for the maps' rebuild, still content work); the start map's door and its skipped test (content work, decision 454).
- **Core:** `PartyEquipment`'s refusal reason, the `TurnKept` event, `StatBonuses.Of`, `PartyStateTests` 322, `GameSessionSaveTests` 270, `BattleState` 283, `GameSession` 289 (each says what its next change moves out first).
- **Scripts and props:** `reflow-comments.pl`'s first line, the `Directory.Build.props` merge (decision 144; protected).
- **Out-of-scope lists of earlier plans:** they stay as pointers.

## Risks
- **A move that isn't one.** The order of field initializers across the files of a partial class is not specified by C#, so a moved initializer that reads another field could change behavior. Every field the moves touch is uninitialized, a constant, or initialized without reading another field: `_viewport`'s `= new()` stays in `MainWindow.xaml.cs` (S1), and `SelectedObjectUids`'s `= []` (S5) reads nothing.
- **Detached comments** on every move (the reviewer's recurring finding): each move slice reads the context line above every moved member in `git diff`.
- **The source generator and partials:** `[ObservableProperty]` and `[RelayCommand]` members keep working from any file of the class (the toolkit reads every part); a missing or doubled hook fails the build.
- **B1's read-only files.** `ContentWriter.WriteAll` moves files one by one after writing them aside; a failure on a later file leaves the earlier ones moved (its own comment says so). B1 makes both moved maps' layout files read-only, so the first move fails whatever the dictionary order and nothing is written; the test restores the attributes in a `finally`, as `Save_RefusedByTheFileWhileTheMapsPass_GivesTheMessageAlone` does.
- **X1's test moves.** `WriteMap` and `WriteMaps(content with the map, [id])` run the same validators on the same maps (every layout against the others, the named map's settings, the new game's start), so each moved test keeps its expected message. A test asserting what `WriteMap` ignored (its catalog's own version of the map) still holds, since `MapContent.With` replaces it.
- **V1 lands mid-plan.** After V1, any slice that changes a line also wraps it; R2 and R3 are written for it. A slice landing before V1 is not checked for code length; the reviewer reads its new lines.
- **Test speed:** D2 keeps the repository-content loader tests on their JSON-only copy (a full copy would copy the music).
- **Save format, determinism, content:** untouched.
