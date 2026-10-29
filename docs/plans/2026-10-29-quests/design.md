# Quests and the quest log: design

Part of [the quests plan](README.md).

## Tunables and placeholders

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Tab position | `Flow/SceneFactory.CreateGameMenu` page order | after Status, before Options | Q9 |
| Tab icon | `Ui/TabGlyphShape` `[TabGlyph.Quests]` | a 12 x 12 scroll mask | placeholder icons, decision 254 |
| Journal order | `Core/Quests/QuestJournal` | main before side; active newest first, completed oldest first | Q6 |
| Out-of-order steps | `Core/Quests/QuestLog` | Q3's picks | |
| Unknown saved stage | `Data/Saving/WorldStateRepair.KeepKnownQuests` | back to the quest's first stage | Q12 |
| Page texts | `content/text/ui.json` (`menu_quests`, `menu_hint_quests`, `quests_none`, `quest_kind_main`, `quest_kind_side`, `quests_active`, `quests_completed`, `quest_completed_note`, `quests_objective`) | "Quests", "No quests yet.", "Main", "Side", "Active", "Completed", "Completed", "Objective" | G1 to G4 |
| Notice texts | `ui.json` `quest_started`, `quest_updated`, `quest_completed` | "Quest started: {title}", "Quest updated: {title}", "Quest completed: {title}" | G5, Q2 |
| Main quest mark | `Ui/UiTheme` (G4) | a small diamond in a pale gold, side quests a dim one | Q5 |
| Placeholder quest | `content/quests.json` `placeholder_errand` | side; stages `ask_innkeeper`, `return_to_giver` | Q13; the developer replaces it |
| Placeholder NPC | `content/maps/start` | the shared `villager` sprite (ADR 0015), cell picked off the tests' paths | N1 |

## Design overview

### What exists
- `Core/World/WorldState`: done "once" events and object swaps, kept across maps, saved as two lists (`SaveGame.CompletedEvents`, `ObjectSwaps`); a no-argument constructor (most test sites) and a restore constructor (`GameSession.Restore`, `WorldStateTests`).
- `Core/Exploration/ExplorationContext(Catalog, Inventory, World, Random)`: what `EventRun` reaches. `EventRun.ApplyAndCheckWait` is an exhaustive switch over step types (`default` throws); a step that applies at once returns false (`SwapObjectStep`, `VoiceStep`).
- `Core/Events/MapEvent`: `OnlyWhenObjectId` (`CanRunOn`) and `OnlyWhenLeaderId` (`IsForLeader`). The leader and "once" checks are written three times: `InteractionFinder.FindInFront` (line 40), `ArrivalEvents.IsRunnable`, `ExplorationState.TryStartCellEvent` (line 255). `ActiveMap` holds the `WorldState` and answers `IsDone`. The first event of an object that may run wins.
- `Core/Content/ContentCatalog`: required init properties, built only by `ContentCatalogLoader` and `tests/Jrpg.Core.Tests/Builders/ContentCatalogBuilder`.
- Data: `ContentListValidator<T>` (nulls, ids, repeats, then each entry's problem); `ContentTypeDiscriminators` names each step type; `Maps/MapContentReferences` (what map events may name; built by the loader, `ContentWriter.ContentReferencesOf` and two test files); `Maps/MapEventValidator.FindStartProblem` / `FindStepProblem`; `AbsentWhenNullFilter` keeps files byte for byte when an optional field added later is null; `Saving/SaveFile.CurrentFormatVersion` (5), `WorldStateRepair`, `SaveGameRepair`.
- Game: `Menus/IMenuPage` (title id, glyph, hint, `Open`, `Advance`, `Update`, `Draw`), built in `Flow/SceneFactory.CreateGameMenu`; `Ui/TabGlyph` with one mask each in `Ui/TabGlyphShape`; seven tabs fit (the body starts at 128 px, 74 px a tab: 646 of 976 px). `Exploration/EventPresenter` shows waiting steps; `GiveItemStep` reads `ExplorationState.CurrentItemReceipt` through `Dialogue/FoundItemText`.
- Editor: `Events/EventStepKind`, `EventStepViewModel` (`Label`, `Problem`, `From`, `ToStep`, one field set per kind), `EventViewModel` (`AddX` commands, `OnlyWhen...` strings, `ToEvent`), `StepChoices` (lists the dialogs offer, built in `EventEditingViewModel.Choices` from `CatalogDraft.Current`), `EventPresets`, `EventListViewModel.ApplyXPreset`, `EventStepEditor.xaml`, `ObjectEventsWindow.xaml`, `CellEventWindow.xaml`.

### Core changes
- **`Quests/` (new folder, a leaf)**:
  - `QuestDefinition` (QD): one quest as content: id, title, description, kind, stages in order. `Stage(string stageId)`, `HasStage`, `FirstStage`, `LastStage`.
  - `QuestStage` (QD): one stage as content: its id and the text the page shows while the quest is at it.
  - `QuestKind` (QD): `Main`, `Side`.
  - `QuestStatus` (QL): `NotStarted`, `Active`, `Completed`.
  - `QuestLog` (QL, internal): each started quest's stage and status; changed only by `Start`, `AdvanceTo`, `Complete` (Q3's rules). SV adds the start order, `Progress` and the restore from `QuestProgress`.
  - `QuestProgress` (SV): one quest's progress as a save keeps it: quest id, stage id, completed.
  - `QuestCondition` (QC): the quest status an event waits for; `IsMetBy(QuestLog)`.
  - `QuestJournal` (J1): the log as the player reads it: active and completed quests in the page's order.
  - `QuestEntry` (J1): one quest in the journal with the stage it is at.
- `World/WorldState.Quests` (QL); restore constructor gains the quests (SV).
- `Events/StartQuestStep`, `AdvanceQuestStep`, `CompleteQuestStep` (QS); `MapEvent.OnlyWhenQuest` (QC).
- `Exploration/EventRun` applies the steps (QS); `ActiveMap.CanStart` (QC).
- `Content/ContentCatalog.Quests` (QD), `Saving/SaveGame.Quests` (SV), `Session/GameSession.Quests` (J1).
- G5 only (after R1 splits `ExplorationState`): `QuestLog`'s three commands return a `QuestChange?` (what changed), `EventRun.CurrentQuestChange`, `ExplorationState.CurrentQuestChange`.

### Structure and folder dependencies

Core (each edge checked: `Quests` names no other Core folder; `World` today names only itself):

| Folder | Depends on | New |
|---|---|---|
| `Quests` | (none) | `QuestDefinition`, `QuestStage`, `QuestKind`, `QuestStatus`, `QuestLog`, `QuestProgress`, `QuestCondition`, `QuestJournal`, `QuestEntry` (G5: `QuestChange`, `QuestChangeKind`) |
| `World` | `Quests` (new edge: `WorldState.Quests`) | |
| `Events` | `Quests` (new edge: `MapEvent.OnlyWhenQuest`) | `StartQuestStep`, `AdvanceQuestStep`, `CompleteQuestStep` |
| `Exploration` | `Quests` (new edge), `World`, `Events`, `Content` (exist) | `EventStarts` (R1, only with G5) |
| `Content` | `Quests` (new edge: `ContentCatalog.Quests`) | |
| `Saving` | `Quests` (new edge: `SaveGame.Quests`) | |
| `Session` | `Quests` (new edge: `GameSession.Quests`) | |

`QuestJournal` takes the log and the quest definitions (`IReadOnlyDictionary<string, QuestDefinition>`), never `ContentCatalog`, so `Quests` stays a leaf (the loot retro's rule). The folder is `Quests` and no type is named `Quests` (CA1724).

Data:

| Folder | Depends on | New |
|---|---|---|
| `Data/Quests` (new) | Core `Quests`; Data `Content` (`ContentListValidator`, `ContentValidationException`) | `QuestCatalogValidator` |
| `Data/Content` | `Data/Quests` (loader) | `quests.json` read before the maps |
| `Data/Maps` | Core `Quests` (new edge: `MapContentReferences.Quests`, `MapEventValidator`) | |
| `Data/Saving` | Core `Quests` | `WorldStateRepair.KeepKnownQuests` |

Game:

| Folder | Depends on | New |
|---|---|---|
| `Menus` | Core `Quests`, `Session` (new edge to `Quests`) | `QuestsPage` (G1); `QuestList`, `QuestDetail` (G2); `QuestRow` (G3); `QuestListView`, `QuestDetailView` (G4) |
| `Ui` | never `Menus` | `TabGlyph.Quests` (G1); the main-quest mark's colors in `UiTheme` (G4) |
| `Dialogue` | Core `Quests` (G5) | `QuestNoticeText` (G5) |
| `Exploration` | `Dialogue` (exists) | |

Editor: `Events` → Core `Quests` (new edge). New: `QuestStageChoice`, `QuestConditionChoice` (E1, E2).

### Key decisions
- **The log keeps stage ids, not positions**: a quest at `ask_innkeeper` stays there when content inserts a stage before it. Q3's "advance back" needs no position either.
- **`QuestLog` takes the definition, not the id**: `Start(QuestDefinition)`, so `WorldState` stays buildable with no catalog (every `new WorldState()` test site unchanged); `EventRun` looks the quest up in `_context.Catalog.Quests`.
- **One gate**: whether an event may start now (leader, "once" done, quest) is `ActiveMap.CanStart`; whether it fits the object's current look stays `MapEvent.CanRunOn` (it needs the object, which cell and arrival events lack).
- **The prompt follows the quest**: `ExplorationState.RefreshInteraction` already runs after an event ends, so an NPC's "Talk" prompt and the event behind it follow the quest the event just changed.
- **Conditions as one list entry in the editor**: per quest, "not started", "active", "at <stage>" for each stage, "completed"; a `QuestConditionChoice` wraps a `QuestCondition?` (null for "always") with its label. The steps: Start and Complete pick a quest id; Advance picks a `QuestStageChoice` ("quest: stage").
- **Save format 6, format 5 refused** (ADR 0022): `SaveGame.Quests` is required, so a format 6 file always has it.

### Extension points
- Per-step conditions or a branch step (Q1's (b), (c)) reuse `QuestCondition.IsMetBy`.
- Quest rewards (Q11), failed quests (a fourth `QuestStatus`), a journal of passed stages (Q8): changes in `QuestLog` and `QuestJournal`, then the page.
- A quest catalog window (Q10): the editor plan's two-slice shape (rules and catalog step, then the window); `CatalogUsage` would refuse removing a quest a map's event names.

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| The leader and "once" checks in `InteractionFinder.FindInFront`, `ArrivalEvents.IsRunnable`, `ExplorationState.TryStartCellEvent` | `ActiveMap.CanStart(mapEvent, leaderId)`, which also checks the quest condition | QC |
| `SaveFile.CurrentFormatVersion = 5` | 6; the format tests' 4 (older) becomes 5, 6 (newer) becomes 7 | SV |
| `WorldState(completedEvents, objectSwaps)` | `WorldState(completedEvents, objectSwaps, quests)` | SV |
| `QuestsPage`'s plain draw (G1 to G3) | `QuestListView` + `QuestDetailView` | G4 |
| `ExplorationState`'s event starting (arrival pending, touch armed, cell events, the running `EventRun`) | `Exploration/EventStarts` | R1 |
| `QuestLog.Start`/`AdvanceTo`/`Complete` (void) | return `QuestChange?` | G5 |

## Risks
- **Event order is content**: with the event gate, the first event that may run wins, so a conditional event must come before an unconditional fallback on the same object. The editor shows the order and the preset (E3) makes it right; a wrong order shows at once in play (the fallback always wins). No validator can tell which order was meant.
- **Seeded and walking tests**: no randomness is added, so no seeded test shifts. N1's NPC must stand off the paths the Game tests walk (the forge plan's list: row 3 right of (10, 3); row 14 left to the forest warp; the door (5, 14); the chest (19, 21); the villager (12, 4); the innkeeper (30, 26); the forge master (25, 14)). The innkeeper's new quest event is gated on the quest being at `ask_innkeeper`, so tests talking to the innkeeper with no quest started still reach the rest event.
- **Save format**: every save from format 5 is refused after SV, the developer's test saves included (ADR 0022's rule until release). Between QL and SV, quest progress isn't saved, but no content starts a quest before N1, after SV.
- **`ExplorationState` at 296 lines**: QC replaces line 255's condition with a call, so the file doesn't grow; G5's property would take it to 300, so R1 splits it first (the backlog's line).
- **`EventPresenter` at 258**: G5 adds about four lines (the step cases); its tests go in a new `EventPresenterQuestTests`.
- **`MessageIds` at 648**: about eleven constants more; its split is a backlog item, not this plan's.
- **CA1812** (an internal class nothing builds): `QuestsPage` is built by `SceneFactory` in G1; `QuestLog`'s restore constructor arrives with its caller in SV; `QuestStageChoice` and `QuestConditionChoice` with their dialogs.
- **Determinism**: nothing random; the journal's order comes from the log's start order, kept by saves.
- **A quest step in a skipped intro** applies (not presentation, ADR 0016); with G5, a step that waits for its notice stops the skip, as a give-item step does.
