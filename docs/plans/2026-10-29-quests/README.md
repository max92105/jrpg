# Quests and the quest log
Status: draft

Drafted on 2026-10-29, before Act 1's content is written: the game needs quest progression and a menu page that tracks it. The developer writes the real quests, story and maps; this plan builds the mechanism and one placeholder quest to test it by hand.

- Numbers: none yet. Decisions are listed under "Decisions (proposed, numbered at approval)", questions as Q1, Q2..., ADR candidates without a number; real numbers come at approval.

## Goal
1. **Quest definitions in content**: `quests.json` (id, title, description, main or side, ordered stages each with an id and a text), validated by Data.
2. **Progression through event steps**: `startQuest`, `advanceQuest` (to a named stage) and `completeQuest`; Core decides what each does in every state.
3. **Events gated on a quest**: an event runs only while a quest is not started, active (at any stage or at a given one) or completed, so an NPC's lines change as the quest advances.
4. **Saved**: quest progress lives in `WorldState`, kept by saves in format 6; format 5 is refused (ADR 0022).
5. **A Quests page** in the game menu: active quests then completed ones, main before side, the selected quest's description and its current stage's text, in the menu's look.
6. **Editor support**: the three steps and the condition in the event dialogs, quest and stage ids picked from lists, a "Quest giver" preset; the map save refuses an unknown quest or stage.
7. **One placeholder quest** on the start map, so every slice from N1 on is checked by hand.

## Order, and why
1. **Phase 1: quests in Core and content (QD, QL, QS, QC, QV, SV).** The definitions, the log, the steps, the condition and its checks, then the save, before any content can make quest progress appear in play (the loot retro: the save shape lands before the data shows up).
2. **Phase 2: a page to see them (J1, G1).** Core's journal (what the page lists, in Core's order), then a bare Quests tab listing the active quests: from here a quest started in a test save can be seen.
3. **Phase 3: authoring and the placeholder (E1, E2, E3, N1).** The editor opens and adds the steps and the condition, then a preset; only then does content use them (the editor throws on a step it can't open). N1 wires the placeholder quest on the start map: from here every slice is checked by hand.
4. **Phase 4: the whole page (G2, G3, G4).** The chosen quest's detail, then the completed quests, with a plain draw; then the look.
5. **Phase 5: notices (R1, G5).** Only if Q2 says quest steps show a line; both are cut otherwise. R1 first splits `ExplorationState` (296 lines), which G5 grows.

## Guiding rules
- **Every quest rule is Core's** (`.claude/rules/game.md`): what a step does in each state (`Quests/QuestLog`), whether an event may run (`Quests/QuestCondition`, through one gate `ActiveMap.CanStart`), and the order the page lists quests in (`Quests/QuestJournal`). The page and the editor only format.
- **Ids, never indexes** (`.claude/rules/content.md`): stages are referenced by id, in steps, conditions and saves.
- **Quest text is content**: titles, descriptions and stage texts live in `quests.json`, as item names live in `items.json`; only the page's labels and notices are `ui.json` messages (ADR 0010).
- **Steps follow ADR 0006**: the quest steps apply at once, like a swap (whether they then wait for a notice is Q2); they are not presentation steps, so skipping a cutscene still applies them (ADR 0016).
- **Pure presenter + thin view** (`docs/map.md`); one selection look (decision 252), every list through `SelectionMotion`; shared pieces from `Ui/`.
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. A Game slice that adds a view type gives its raw estimate and the 1.6x; Editor slices aim at about 200 lines.
- **Files near 300 lines:** new tests go in new files beside `EventRunTests` (269), `GameSessionSaveTests` (270), `ContentCatalogLoaderTests` (272), `EventStepViewModelTests` (260), `EventListViewModelTests` (258), `EventPresenterTests` (305). Watched: `Core/Exploration/ExplorationState` (296, must not grow: QC keeps it there, R1 splits it before G5 adds to it), `Game/Exploration/EventPresenter` (258), `Ui/MessageIds` (648, a table of constants, backlog).
- **Save format 6**, the old format refused, no migration (ADR 0022). **No NuGet package.**

## Out of scope (follow-ups)
- **Quest rewards** (Q11): a completion event gives items with the existing `giveItem` step; no reward field on a quest.
- **A quest catalog window in the editor** (Q10): quests are written in `quests.json` by hand; the editor picks their ids from lists and the save refuses unknown ones.
- **Map markers, a quest tracker on the field, failed or abandoned quests, quest-started battles.**
- **Per-step conditions or branch steps** (Q1, ADR candidate): this plan gates whole events.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: quests in Core and content
- [ ] **QD. Quest definitions in `quests.json`, validated and in the catalog** (Core + Data + content, about 220 lines) → [slices/QD.md](slices/QD.md)
- [ ] **QL. The quest log in `WorldState`: start, advance, complete** (Core, about 235 lines) → [slices/QL.md](slices/QL.md)
- [ ] **QS. The `startQuest`, `advanceQuest` and `completeQuest` steps** (Core + Data, about 200 lines) → [slices/QS.md](slices/QS.md)
- [ ] **QC. Events gated on a quest's status, through one start gate** (Core, about 190 lines) → [slices/QC.md](slices/QC.md)
- [ ] **QV. The condition in content: read, checked, left out of files while unset** (Data, about 110 lines) → [slices/QV.md](slices/QV.md)
- [ ] **SV. Quests in saves, format 6, repaired against content** (Core + Data, about 230 lines) → [slices/SV.md](slices/SV.md)

### Phase 2: a page to see them
- [ ] **J1. The quest journal: active and completed quests in Core's order** (Core, about 140 lines) → [slices/J1.md](slices/J1.md)
- [ ] **G1. A Quests tab lists the active quests** (Game + text, raw 145, about 230 lines at 1.6x) → [slices/G1.md](slices/G1.md)

### Phase 3: authoring and the placeholder
- [ ] **E1. The event dialogs show and add the three quest steps** (Editor, about 210 lines) → [slices/E1.md](slices/E1.md)
- [ ] **E2. The event dialogs set an event's quest condition** (Editor, about 190 lines) → [slices/E2.md](slices/E2.md)
- [ ] **E3. The Quest giver preset** (Editor, about 140 lines) → [slices/E3.md](slices/E3.md)
- [ ] **N1. A placeholder quest on the start map** (content + tests, about 100 lines) → [slices/N1.md](slices/N1.md)

### Phase 4: the whole page
- [ ] **G2. Choosing a quest shows its detail** (Game + text, about 190 lines) → [slices/G2.md](slices/G2.md)
- [ ] **G3. The completed quests, under their band** (Game + text, about 160 lines) → [slices/G3.md](slices/G3.md)
- [ ] **G4. The Quests page's look: columns, bands, kind marks, wrapped text** (Game, raw 150, about 240 lines at 1.6x) → [slices/G4.md](slices/G4.md)

### Phase 5: notices (only if Q2 answers (b) or (c))
- [ ] **R1. `ExplorationState` split: starting map events moves out** (Core, refactor, about 150 lines moved) → [slices/R1.md](slices/R1.md)
- [ ] **G5. A quest step that changed the log shows a line** (Core + Game + text, about 210 lines) → [slices/G5.md](slices/G5.md)

## Decisions (proposed, numbered at approval)
- **Quest text lives in `quests.json`** (title, description, each stage's text), as item names live in `items.json`; `ui.json` holds only the page's labels and the notices.
- **Stages are referenced by id** in steps, conditions and saves, never by position, so inserting a stage in content keeps saves and events pointing at the right one.
- **The quest log lives in `WorldState`** (`WorldState.Quests`, a `QuestLog`): each started quest's current stage id and whether it is completed, in the order quests were started. Saves keep it as a list of `QuestProgress` in that order (format 6).
- **The three quest steps apply at once** (ADR 0006's "applies at once" kind, like a swap) and are not presentation steps (ADR 0016): skipping a cutscene still applies them.
- **The quest condition is a gate on the whole event** (`MapEvent.OnlyWhenQuest`), beside `onlyWhenObjectId` and `onlyWhenLeaderId`: an NPC holds one event per quest situation, and the first one that may run wins (as today). Pending Q1 and its ADR.
- **One start gate**: `ActiveMap.CanStart(mapEvent, leaderId)` (leader, "once" done, quest condition) replaces the three copies of the leader and "once" checks in `InteractionFinder`, `ArrivalEvents` and `ExplorationState`.
- **Core orders the journal** (`QuestJournal`): main quests before side quests, each kind in the order the quests were started (pending Q6); the page only formats it.
- **The editor picks quest and stage ids from lists** built from the loaded catalog; a quest condition is one list entry ("herb_errand: at ask_innkeeper"), so the dialog can't hold half a condition. The map save already validates through Data, so it refuses an unknown quest or stage.

## Open design questions (developer decides)
- **Q1. Where do quest conditions live?** (a) on the whole event (`onlyWhenQuest`; the NPC holds one event per situation, first runnable wins); (b) on each step (a step runs or is skipped by the quest's status); (c) a branch step that jumps by the quest's status. Planner's pick: (a), the simplest to author and to read in the editor, and the pattern the leader gate already uses; (b) or (c) can come later on the same `QuestCondition`. ADR needed.
- **Q2. Do quest steps show a line?** (a) none: they change the log silently, and the dialogue says what happened; (b) start and complete show "Quest started: X" / "Quest completed: X", an advance shows nothing; (c) all three, an advance showing "Quest updated: X". Planner's pick: (c), each in the dialogue box like "Obtained Potion", and only when the step changed the log. G5 does it (after R1); (a) cuts both.
- **Q3. What does each step do out of order?** Planner's picks: start on a started or completed quest does nothing; advance on a quest not started starts it at that stage; advance may move back to an earlier stage; advance on a completed quest does nothing; complete on a quest not started completes it at its last stage; complete on a completed quest does nothing.
- **Q4. Which conditions?** Planner's pick: not started, active at any stage, active at a given stage, completed; one quest condition per event. Not included: "at this stage or later", several quests at once.
- **Q5. The page's layout.** (a) one list, an ACTIVE band then a COMPLETED band, main before side in each, a small mark telling main from side; (b) two categories switched with Left and Right (like the Victory screen's tabs), Active and Completed; (c) a rail like the Inventory's. Planner's pick: (a), with the detail column beside the list.
- **Q6. The order within main and within side quests.** (a) the order they were started, oldest first; (b) most recent first; (c) `quests.json` order. Planner's pick: (b) for active quests, so the newest errand is on top, and (a) for completed ones.
- **Q7. What a completed quest shows.** (a) its description and a "Completed" note; (b) the text of the stage it was completed at; (c) a new optional `completedText` in content. Planner's pick: (a).
- **Q8. The text of past stages.** Planner's pick: only the current stage's text, as asked; a list of passed stages (a journal) is a follow-up.
- **Q9. The Quests tab's place.** Planner's pick: right after Status, before Options (Octopath II puts its journal second, under the world map; ours has no world map).
- **Q10. A quest catalog window in the editor**, now or later? Planner's pick: later; quests are short to write by hand, and an editor window is two slices (its rules and catalog step, then the window).
- **Q11. Quest rewards**: a reward field on the quest (items, experience given on completion), or events give them with `giveItem`? Planner's pick: events, no reward field for now.
- **Q12. A saved quest whose stage no longer exists in content.** Planner's pick: it goes back to its quest's first stage, and the repair says so (as other repairs do); a quest no longer in content is forgotten.
- **Q13. The placeholder quest's shape** (N1). Planner's pick: one side quest, "placeholder_errand", two stages: a new NPC on the start map gives it, the innkeeper moves it to its second stage, the giver completes it; each step and every condition kind used once.

## ADRs needed
- [ ] **ADR needed: where quest conditions live** (event gate, per-step condition or branch step; Q1). Before QC.
- [ ] **ADR needed: quest progress as its own model** (a `QuestLog` of stage ids per quest) rather than general story flags or variables that quests would read. Before QL. If the developer prefers general flags, QL, QS and SV change shape: the plan is re-drafted, not patched.
