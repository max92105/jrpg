# Quests and the quest log: design

Part of [the quests plan](README.md). Written against the code as the gold and shops plan leaves it (it runs first, decision 374).

## Tunables and placeholders

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Tab position | `Flow/SceneFactory.CreateGameMenu` page order | first, before Inventory | decision 393 |
| Tab icon | `Ui/TabGlyphShape` `[TabGlyph.Quests]` | a 12 x 12 scroll mask | placeholder icons, decision 254 |
| Journal order | `Core/Quests/QuestJournal` | main before side; active newest first, completed oldest first | decisions 390, 391 |
| Out-of-order steps | `Core/Quests/QuestLog` | decision 388 | |
| Unknown saved stage | `Data/Saving/WorldStateRepair.KeepKnownQuests` | back to the quest's first stage | decision 396 |
| Page texts | `content/text/ui.json` (`menu_quests`, `menu_hint_quests`, `quests_none`, `quest_kind_main`, `quest_kind_side`, `quests_active`, `quests_completed`, `quests_none_completed`, `quest_completed_note`, `quests_objective`) | "Quests", "No quests yet.", "Main", "Side", "Active", "Completed", "No completed quests.", "Completed", "Objective" | G1 to L3 |
| Notice texts | `ui.json` `quest_started`, `quest_updated`, `quest_completed` | "Quest started: {title}", "Quest updated: {title}", "Quest completed: {title}" | G4, decision 387 |
| Main quest mark | `Ui/UiTheme` (L1) | a small diamond in a pale gold, side quests a dim one | |
| Page list layout | `Ui/PageListLayout` (L1) | margins 28 and 20, rows 52 px | the values every page list uses today |
| Placeholder quest | `content/quests.json` `placeholder_errand` (QD) | side; stages `ask_innkeeper`, `return_to_giver` | decision 397; the developer replaces it |
| Placeholder giver | `content/map-layouts/start.json` (N1) | the shared `villager` sprite (ADR 0015), a cell off the tests' paths | |

## Design overview

### What exists (after the gold and shops plan)
- `Core/World/WorldState`: done "once" events and object swaps, kept across maps, saved as two lists (`SaveGame.CompletedEvents`, `ObjectSwaps`); a no-argument constructor (most test sites) and a restore constructor (`GameSession.Restore`, `WorldStateTests`).
- `Core/Exploration/ExplorationContext(Catalog, Inventory, World, Random)`: what `EventRun` reaches. `EventRun.ApplyAndCheckWait` is an exhaustive switch over step types (`default` throws), at the CA1502 limit; the gold plan's I1 moves the rest case into `ApplyRest` and E1 adds `OpenShopStep` to the waiting case. A step that applies at once returns false (`SwapObjectStep`, `VoiceStep`).
- `Core/Events/MapEvent`: `OnlyWhenObjectId` (`CanRunOn`) and `OnlyWhenLeaderId` (`IsForLeader`). The leader and "once" checks are written three times: `InteractionFinder.FindInFront`, `ArrivalEvents.IsRunnable`, `ExplorationState.TryStartCellEvent`. `ActiveMap` holds the `WorldState` and answers `IsDone`; `CellEventAt` returns a cell's first touch event. The first event of an object that may run wins.
- `Core/Exploration/ExplorationState`: 299 lines after the gold plan's I1 (`CurrentInnStay`).
- `Core/Content/ContentCatalog`: required init properties, built only by `ContentCatalogLoader` and `tests/Jrpg.Core.Tests/Builders/ContentCatalogBuilder`.
- Data: `ContentListValidator<T>`; `ContentTypeDiscriminators` names each step type; `Maps/MapContentReferences` (five positional members plus the gold plan's `ShopIds` init member; sites: the loader, `ContentWriter.ContentReferencesOf`, three test files); `Maps/MapEventValidator.FindStartProblem` / `FindStepProblem` (helpers per step kind for CA1502); `AbsentWhenNullFilter`; `ContentFileWriterTests.Write_EveryEventStepKind_ReadsBackAsTheSameSteps`; `Saving/SaveFile.CurrentFormatVersion` (6 after the gold plan), `FileSaveStore.CheckNoNullEntries`, `WorldStateRepair`, `SaveGameRepair`; `ContentWriter.WriteMapCatalogs` (tilesheets, tiles, objects, all or none, every map checked against them).
- Game: `Menus/IMenuPage`, built in `Flow/SceneFactory.CreateGameMenu`; `Ui/TabGlyph` with one mask each in `Ui/TabGlyphShape`; seven tabs fit (the body starts at 128 px, 74 px a tab: 646 of 976 px). `Exploration/EventPresenter` shows waiting steps (`GiveItemStep` through `Dialogue/FoundItemText`). The pointed tabs are drawn twice (`Battle/VictoryDropsView`, the gold plan's `Trade/ShopListView`); a page list's margins (28, 20) and row height (52) are private constants in about eight views.
- Editor: `Events/EventStepKind`, `EventStepViewModel`, `EventViewModel(MapEvent)` (5 construction sites), `StepChoices` (four positional members plus the gold plan's `ShopIds`), `EventEditingViewModel.Choices` (read from `CatalogDraft.Current` when a dialog opens), `EventStepEditor.xaml`, `ObjectEventsWindow.xaml`, `CellEventWindow.xaml`. `Catalogs/CatalogDraft` (`SetObject`, `ObjectsRemovedByUndo`, one `CatalogEdit` per change), `CatalogUsage`, `Shell/CombinedHistory.RefusalOfCatalogUndo`, `MainViewModel.Save` (catalogs first when dirty, then the map).

### Core changes
- **`Quests/` (new folder, a leaf)**:
  - `QuestDefinition` (QD): one quest as content: id, title, description, kind, stages in order.
  - `QuestStage` (QD): one stage as content: its id and the text the page shows while the quest is at it.
  - `QuestKind` (QD): `Main`, `Side`.
  - `QuestLog` (QL, internal): each started quest with its definition, stage and completion; changed only by `Start`, `AdvanceTo`, `Complete` (decision 388). QC adds `HasReached`; SV the start order, `Progress` and the restore; G4 the commands' `QuestChange?` results.
  - `QuestCondition`, `QuestConditionKind` (QC): what an event waits for (decision 389).
  - `QuestProgress` (SV): one quest's progress as a save keeps it.
  - `QuestJournal`, `QuestEntry` (J1): the log as the player reads it; `Completed` arrives in G3.
  - `QuestChange`, `QuestChangeKind` (G4): what a step changed.
- `World/WorldState.Quests` (QL); the restore constructor takes a restored `QuestLog` (SV).
- `Events/StartQuestStep`, `AdvanceQuestStep`, `CompleteQuestStep` (QS); `MapEvent.OnlyWhenQuest` (QC).
- `Exploration/EventRun` applies the steps (QS), waits on a changed one (G4); `ActiveMap.CanStart` (QC); R1's `MapEventStarter`.
- `Content/ContentCatalog.Quests` (QD), `Saving/SaveGame.Quests` (SV), `Session/GameSession.Quests` (J1).

### Structure and folder dependencies

Core (each edge checked: `Quests` names no other Core folder; `World` today names only itself):

| Folder | Depends on | New |
|---|---|---|
| `Quests` | (none) | `QuestDefinition`, `QuestStage`, `QuestKind`, `QuestLog`, `QuestCondition`, `QuestConditionKind`, `QuestProgress`, `QuestJournal`, `QuestEntry`, `QuestChange`, `QuestChangeKind` |
| `World` | `Quests` (new edge: `WorldState.Quests`) | |
| `Events` | `Quests` (new edge: `MapEvent.OnlyWhenQuest`) | `StartQuestStep`, `AdvanceQuestStep`, `CompleteQuestStep` |
| `Exploration` | `Quests` (new edge), `World`, `Events`, `Content` (exist) | `MapEventStarter` (R1) |
| `Content` | `Quests` (new edge: `ContentCatalog.Quests`) | |
| `Saving` | `Quests` (new edge: `SaveGame.Quests`) | |
| `Session` | `Quests` (new edge: `GameSession.Quests`) | |

The log holds each quest's definition, so neither `QuestCondition` nor `QuestJournal` takes the catalog and `Quests` stays a leaf (the loot retro's rule). `GameSession.Restore` builds the log (`QuestLog.Restore(saved, catalog.Quests)`) and hands it to `WorldState`. The folder is `Quests` and no type is named `Quests` (CA1724).

Data:

| Folder | Depends on | New |
|---|---|---|
| `Data/Quests` (new) | Core `Quests`; Data `Content` (`ContentListValidator`, `ContentValidationException`) | `QuestCatalogValidator` |
| `Data/Content` | `Data/Quests` (loader, `ContentWriter`) | `quests.json` read before the maps; written with the catalogs (QE1) |
| `Data/Maps` | Core `Quests` (new edge: `MapContentReferences.Quests`, `MapEventValidator`) | |
| `Data/Saving` | Core `Quests` | `WorldStateRepair.KeepKnownQuests` |

Game:

| Folder | Depends on | New |
|---|---|---|
| `Menus` | Core `Quests`, `Session` (new edge to `Quests`) | `QuestsPage` (G1); `QuestList`, `QuestDetail` (G2); `QuestCategory` (G3); `QuestListView` (L1), `QuestDetailView` (L3) |
| `Ui` | never `Menus` | `TabGlyph.Quests` (G1); `PageListLayout`, the main mark's colors (L1); `PointedTabsView` (L2) |
| `Dialogue` | Core `Quests` (G4) | `QuestNoticeText` |
| `Exploration` | `Dialogue` (exists) | |

Editor:

| Folder | Depends on | New |
|---|---|---|
| `Events` | Core `Quests` (new edge) | `QuestStageChoice` (E1), `QuestGateViewModel` (E2) |
| `Quests` (new) | `Catalogs`, `Events` (`ListOrder`); Core `Quests` | `QuestViewModel`, `QuestStageViewModel` (QE2), `QuestCatalogViewModel`, `QuestEditingViewModel` (QE3), `QuestCatalogWindow` (QE4) |
| `Catalogs` | Core `Quests` (new edge); never `Events` | (`CatalogDraft.SetQuests`, `CatalogUsage.UsersOfQuest`, QE1) |
| `Shell` | `Quests` (new edge) | `MainViewModel.QuestEditing` (QE3), `MainWindow.QuestEditing` partial (QE4) |

`Events` already reads `Catalogs` (`EventEditingViewModel` takes the `CatalogDraft`), so `Catalogs` never reads `Events`: `CatalogDraft.QuestStagesRemovedByUndo` returns `(string QuestId, string StageId)` pairs, not `Events/QuestStageChoice`. `Events` never reads the editor's `Quests` folder: the dialogs get quest ids and stages from `StepChoices`, built from `CatalogDraft.Current`.

### Key decisions
- **The log keeps stage ids, not positions**: a quest at `ask_innkeeper` stays there when content inserts a stage before it. Only `HasReached` compares positions, in the quest's current order (decision 389's flagged reading).
- **`QuestLog` takes the definition, not the id**: `Start(QuestDefinition)`; `EventRun` looks the quest up in `_context.Catalog.Quests`. `new WorldState()` stays as it is at every test site.
- **One gate**: whether an event may start now (leader, "once" done, quest) is `ActiveMap.CanStart`; whether it fits the object's current look stays `MapEvent.CanRunOn` (it needs the object, which cell and arrival events lack).
- **The prompt follows the quest**: the interaction is refreshed after an event ends, so an NPC's "Talk" and the event behind it follow the quest the event just changed.
- **No shadowing** (QV): an object event that can always start, and isn't "once", hides every later event of its activation on that object forever; content refuses it rather than leaving the developer to find out in play.
- **A step waits only for its line** (G4): it applies when it becomes current; if it changed the log, it waits with its `QuestChange`; otherwise it doesn't wait.
- **The editor's condition is three boxes** (the quest, the kind, the stage), held by one `QuestGateViewModel` with its own problem; changing the quest clears the stage.
- **Quests are a catalog in the editor** (decision 394, ADR 0014): `CatalogDraft.SetQuests` records the window's whole session as one `CatalogEdit`; `quests.json` is written with the other catalogs by Ctrl+S, checked by the loader's validator and every map checked against it; an undo that would remove a quest or stage an event names is refused, as for objects and tiles.
- **Save format 7, format 6 refused** (ADR 0022): `SaveGame.Quests` is required.

### Extension points
- Per-step conditions or a branch step reuse `QuestCondition.IsMetBy`.
- Failed quests (a new `QuestChangeKind` and a third category), a journal of past stages, rewards: `QuestLog`, `QuestJournal`, then the page.
- General story flags (decision 398): their own plan, beside the log.
- Removing saved quests or stages in the window: `CatalogUsage.UsersOfQuest` already answers who names them.

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| `ExplorationState`'s event starting (arrival pending, touch armed, cell events, the running `EventRun` and its result properties) | `Exploration/MapEventStarter` | R1 |
| The leader and "once" checks in `InteractionFinder.FindInFront`, `ArrivalEvents.IsRunnable`, the cell event start | `ActiveMap.CanStart(mapEvent, leaderId)`, which also checks the quest condition | QC |
| `SaveFile.CurrentFormatVersion = 6` | 7; the format tests' "six" become "seven", the older version 5 becomes 6, the newer 7 becomes 8 | SV |
| `WorldState(completedEvents, objectSwaps)` | `WorldState(completedEvents, objectSwaps, quests)` | SV |
| `QuestsPage`'s plain draw (G1 to G3) | `QuestListView` (L1), `Ui/PointedTabsView` (L2), `QuestDetailView` (L3) | L1-L3 |
| `QuestLog.Start`/`AdvanceTo`/`Complete` (void) | return `QuestChange?` | G4 |
| `ContentWriter.WriteMapCatalogs` (three files) | also `quests.json` (the name stays: ten test sites) | QE1 |

## Risks
- **Between N1 and SV a save forgets quest progress** (the review's order: the placeholder in play before the save): loading such a save shows the errand not started. SV lands right after N1; no other slice comes between them.
- **Event order is content**: the first event of an object that may start wins, so a conditional event must come before a broader one (for the placeholder giver: completed, then reached `return_to_giver`, then active). QV refuses the one order that can never work (an ungated event first); other wrong orders show at once in play.
- **Seeded and walking tests**: no randomness is added. N1's NPC must stand off the paths the Game tests walk (the forge plan's list, plus the forge master (25, 14) and the gold plan's shopkeeper). The innkeeper's quest event is gated on `ask_innkeeper`, so tests talking to the innkeeper with no quest started still reach the rest event (priced, after the gold plan).
- **Save format**: every format 6 save is refused after SV, the developer's test saves included (ADR 0022 until release).
- **`ExplorationState` at 299**: R1 splits it before QC touches it; nothing in this plan grows it before R1.
- **CA1502 in `EventRun.ApplyAndCheckWait`**: one case for the three quest steps calling `ApplyQuestStep`; G4 changes only that helper.
- **The menu opens on Quests** (decision 393): the selected tab at opening moves from Inventory to Quests; no test found that depends on the first tab (`GameMenuScene*Tests` build their own pages).
- **`MessageIds`**: about thirteen constants more; its split is a backlog item.
- **CA1812** (an internal class nothing builds): `QuestsPage` is built by `SceneFactory` in G1; `QuestLog.Restore` arrives with its caller in SV; `QuestStageChoice` and `QuestGateViewModel` with their dialogs; `QuestViewModel` is built by its tests only in QE2 (if the analyzer flags it, QE3's `QuestCatalogViewModel` lands with it), `QuestEditingViewModel` by `MainViewModel` in QE3, the window in QE4.
- **The editor's draft and hand-written quests**: the editor loads content once; a quest written by hand in `quests.json` while it runs appears after a restart, and an editor save (Ctrl+S, catalogs dirty) would write the draft's quests over it. Quests made in the window appear at once.
- **A quest step in a skipped intro** applies (not presentation, ADR 0016); after G4, a step that waits for its line stops the skip, as a give-item step does.
