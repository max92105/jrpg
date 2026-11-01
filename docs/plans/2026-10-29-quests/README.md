# Quests and the quest log
Status: approved, in progress

Drafted on 2026-10-29, before Act 1's content is written: the game needs quest progression and a menu page that tracks it. The developer writes the real quests, story and maps; this plan builds the mechanism, an editor window for quests, and one placeholder quest to test it all by hand.

- Numbers: decisions 386-398, ADRs 0032 and 0033. Runs after [the gold and shops plan](../done/2026-10-29-gold-and-shops/README.md) (its decision 374): that plan's save is format 6, this plan's format 7; that plan leaves `ExplorationState` at 299 lines and this plan splits it, once (R1).
- Approved on 2026-10-29 with the developer's answers (386-398) and the devil's advocate's edits: a "reached stage" condition with its own kind enum; the placeholder quest written from the first slice and wired on the map before the save, so the page shows a real quest; R1 unconditional, before QC; the Quest giver preset deferred, replaced by a content rule refusing an event that shadows the later ones on its object; CA1502 and null-entry budgets named; a quest editor window in this plan (decision 394); two categories on the page (decision 390); the Quests tab first (decision 393).

## Goal
1. **Quest definitions in content**: `quests.json` (id, title, description, main or side, ordered stages each with an id and a text), validated by Data.
2. **Progression through event steps**: `startQuest`, `advanceQuest` (to a named stage) and `completeQuest`; Core decides what each does in every state (decision 388), and a step that changed the log shows a line (decision 387).
3. **Events gated on a quest** (decisions 386, 389): an event runs only while a quest is not started, active, at a stage, at or past a stage, or completed, so an NPC's lines change as the quest advances.
4. **Saved**: quest progress lives in `WorldState`, kept by saves in format 7; format 6 is refused (ADR 0022).
5. **A Quests page**, the menu's first tab (decision 393): Active and Completed categories switched with Left and Right, main before side in each (decision 390), the chosen quest's description and current stage (decision 392), in the menu's look.
6. **Editor support**: the three steps and the condition in the event dialogs, ids picked from lists; a Quests window to create quests and their stages with undo (decision 394); the map save refuses an unknown quest or stage.
7. **One placeholder quest** (decision 397) on the start map, so every slice from N1 on is checked by hand.

## Order, and why
1. **Phase 1: quests in Core and content (QD, QL, QS, R1, QC, QV).** The definitions with the placeholder quest, the log, the steps; then `ExplorationState` split once (299 lines after the gold plan) before the condition touches it; then the condition and its content checks.
2. **Phase 2: authoring, the placeholder and the save (E1, E2, E3, N1, SV).** The editor opens and adds the steps and the condition before content uses them (the editor throws on a step it can't open); N1 wires the placeholder on the start map, so a hand check exists from here; SV saves it right after (between N1 and SV a save forgets quest progress, design.md "Risks").
3. **Phase 3: the page (J1, G1, G2, G3, L1, L2, L3).** Core's journal, a bare tab listing the placeholder quest, the chosen quest's detail, the two categories; then the look in three slices (the list column, the category tabs, the detail column).
4. **Phase 4: notices (G4).** A step that changed the log shows a line (decision 387).
5. **Phase 5: the Quests window (QE1, QE2, QE3, QE4).** Quests in the editor's catalog draft (one undo step, saved with the catalogs), one quest's edit, the list and applying it, then the window.

## Guiding rules
- **Every quest rule is Core's** (`.claude/rules/game.md`): what a step does in each state (`Quests/QuestLog`), whether an event may start (`Quests/QuestCondition`, through one gate `ActiveMap.CanStart`), and the order the page lists quests in (`Quests/QuestJournal`). The page and the editor only format.
- **Ids, never indexes** (`.claude/rules/content.md`): stages are referenced by id, in steps, conditions and saves. Only "reached" reads a stage's position, from the quest's current order.
- **Quest text is content**: titles, descriptions and stage texts live in `quests.json`, as item names live in `items.json`; only the page's labels and the notices are `ui.json` messages (ADR 0010).
- **Steps follow ADR 0006**: a quest step applies when it becomes current; it waits for its line only when it changed the log (G4; before G4 it never waits). Quest steps are not presentation steps: skipping a cutscene applies them (ADR 0016).
- **Pure presenter + thin view** (`docs/map.md`); one selection look (decision 252), every list through `SelectionMotion`; shared pieces from `Ui/` (a third copy moves there: the list layout numbers, the pointed tabs).
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. A Game slice that adds a view type gives its raw estimate and the 1.6x; Editor slices aim at about 200 lines.
- **Files near 300 lines:** new tests go in new files beside `EventRunTests` (269), `GameSessionSaveTests` (270), `ContentCatalogLoaderTests` (272), `EventStepViewModelTests` (260), `EventListViewModelTests` (258), `EventPresenterTests` (305). Watched: `Core/Exploration/ExplorationState` (299 after the gold plan's I1: R1 splits it before anything else touches it), `Exploration/EventRun` (`ApplyAndCheckWait` at the CA1502 limit: QS's budget), `Game/Exploration/EventPresenter`, `Editor/Events/EventStepViewModel` (about 230 after the gold plan's E2 and I3; E1 adds about 30), `Session/GameSession` (about 255 after the gold plan), `Ui/MessageIds` (constants; the backlog says it can wait). `Editor/Shell/MainViewModel` (489) and `MainWindow.xaml.cs` (547) are already over: QE3 adds one property to the first, QE4 its handler in a new partial file.
- **Save format 7**, format 6 refused, no migration (ADR 0022). **No NuGet package.**
- **Music plan:** if its menu sounds (its S3, `IMenuPage.Sounds`) land first, the Quests page asks its sounds too (a Watch line in G1, G2, G3).

## Out of scope (follow-ups)
- **The Quest giver preset** (deferred by the review): the developer builds a giver's events with the dialogs; QV's shadowing rule refuses the one order that can't work.
- **Gating a cell's touch event on a quest to change what it does**: a cell holds one event (`ActiveMap.CellEventAt` returns the first touch event on it, and the cell dialog edits one), so a quest gate on a cell only switches it on or off, and a cell gated off rolls a random encounter like any other cell. Workaround: put the changing events on an object (a sign, a stone, any object the player walks into: an object holds several Touch events and the first that may run wins), or keep the cell off maps with random encounters.
- **Removing a quest, or a stage already saved, in the Quests window**: the window removes only what it added in the same session; the rest is a follow-up (with `CatalogUsage` refusing a removal an event or a save would miss).
- **Quest rewards** (decision 395): a completion event gives items with `giveItem`; no reward field.
- **General story flags** (decision 398): story state that isn't a quest is a main-quest stage for now; flags get their own plan.
- **Map markers, a quest tracker on the field, failed or abandoned quests, a journal of past stages, per-step conditions or branch steps.**

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: quests in Core and content
- [x] **QD. Quest definitions in `quests.json` with the placeholder errand, validated and in the catalog** (Core + Data + content, about 230 lines) → [slices/QD.md](slices/QD.md)
- [x] **ADR 0032: quest progress is its own log** (stage ids per quest in `WorldState`, rather than general flags; story state that isn't a quest is a main-quest stage for now): write it from decisions 388 and 398 and design.md "Key decisions" (nothing left to ask), before QL
- [x] **QL. The quest log in `WorldState`: start, advance, complete** (Core, about 230 lines) → [slices/QL.md](slices/QL.md)
- [x] **QS. The `startQuest`, `advanceQuest` and `completeQuest` steps** (Core + Data, about 220 lines) → [slices/QS.md](slices/QS.md)
- [x] **R1. `ExplorationState` split: starting map events moves out** (Core refactor, about 160 lines moved, tests unchanged) → [slices/R1.md](slices/R1.md)
- [x] **ADR 0033: quest conditions gate whole events** (`onlyWhenQuest` beside the leader and object gates; the first event of an object that may start wins): write it from decisions 386 and 389 (nothing left to ask), before QC
- [x] **QC. Events gated on a quest, through one start gate** (Core, about 230 lines) → [slices/QC.md](slices/QC.md)
- [ ] **QV. The condition in content, and no event shadowing the later ones on its object** (Data, about 210 lines) → [slices/QV.md](slices/QV.md)

### Phase 2: authoring, the placeholder and the save
- [ ] **E1. The event dialogs show and add the three quest steps** (Editor, about 210 lines) → [slices/E1.md](slices/E1.md)
- [ ] **E2. The event's quest condition in the dialogs' model** (Editor, about 180 lines) → [slices/E2.md](slices/E2.md)
- [ ] **E3. The quest, condition and stage boxes in both event dialogs** (Editor, about 110 lines) → [slices/E3.md](slices/E3.md)
- [ ] **N1. The placeholder errand on the start map** (content + tests, about 100 lines) → [slices/N1.md](slices/N1.md)
- [ ] **SV. Quests in saves, format 7, repaired against content** (Core + Data, about 240 lines) → [slices/SV.md](slices/SV.md)

### Phase 3: the page
- [ ] **J1. The quest journal: the active quests in Core's order** (Core, about 100 lines) → [slices/J1.md](slices/J1.md)
- [ ] **G1. A Quests tab, first in the menu, lists the active quests** (Game + text, raw 125, about 200 lines at 1.6x) → [slices/G1.md](slices/G1.md)
- [ ] **G2. Choosing a quest shows its detail** (Game + text, about 190 lines) → [slices/G2.md](slices/G2.md)
- [ ] **G3. The Active and Completed categories, switched with Left and Right** (Core + Game + text, about 210 lines) → [slices/G3.md](slices/G3.md)
- [ ] **L1. The list column's look: rows, kind marks, one list layout in `Ui/`** (Game, raw 120, about 190 lines at 1.6x) → [slices/L1.md](slices/L1.md)
- [ ] **L2. The category tabs' look: `Ui/PointedTabsView`** (Game, raw 80, about 130 lines at 1.6x) → [slices/L2.md](slices/L2.md)
- [ ] **L3. The detail column's look: serif title, wrapped texts, the Objective band** (Game, raw 125, about 200 lines at 1.6x) → [slices/L3.md](slices/L3.md)

### Phase 4: notices
- [ ] **G4. A quest step that changed the log shows a line** (Core + Game + text, about 210 lines) → [slices/G4.md](slices/G4.md)

### Phase 5: the Quests window
- [ ] **QE1. Quests in the editor's catalog draft: one undo step, saved with the catalogs, an undo removing a used quest refused** (Data + Editor, about 210 lines) → [slices/QE1.md](slices/QE1.md)
- [ ] **QE2. The Quests window's rules: one quest, its stages, their ids and problems** (Editor, about 210 lines) → [slices/QE2.md](slices/QE2.md)
- [ ] **QE3. The Quests window's list: new quests, unique ids, applied to the draft as one undo step** (Editor, about 170 lines) → [slices/QE3.md](slices/QE3.md)
- [ ] **QE4. The Quests window** (Editor, about 160 lines) → [slices/QE4.md](slices/QE4.md)

## Decisions
From earlier plans and ADRs: 107 (the leader is fixed), 252 (one selection look), 253 (take the vibe), 254 (placeholder tab icons), 262 and 336 (lists glide), 355 (the victory's pointed tabs), 374 (the gold and shops plan runs first); ADR 0006 (event steps), 0010 (text), 0011 (content), 0014 (editing shared catalogs), 0016 (skipping), 0022 (saves).

The developer's answers (2026-10-29):
- **386. Quest conditions live on the whole event** (question Q1): `onlyWhenQuest`, beside `onlyWhenObjectId` and `onlyWhenLeaderId`; an NPC holds one event per quest situation, and the first event of an object that may start wins (as today). ADR 0033.
- **387. A quest step shows a line when it changed something** (Q2): "Quest started: X", "Quest updated: X", "Quest completed: X" in the dialogue box, like "Obtained Potion"; a step that changed nothing shows nothing and doesn't wait (G4).
- **388. Steps out of order** (Q3): start on a started or completed quest does nothing; advance on a quest not started starts it at that stage; advance may move back to an earlier stage; advance on a completed quest does nothing; complete on a quest not started completes it at its last stage; complete on a completed quest does nothing.
- **389. Five conditions, one per event** (Q4): not started; active (any stage); at a stage; reached a stage (at it or past it, or completed); completed. `QuestCondition` has its own kind enum; a stage is named only by the two stage kinds. *Interpretation flagged:* "past" is read from the quest's current stage and the stage order in content, so an advance back (388) un-reaches the later stages.
- **390. The page shows two categories, Active and Completed, switched with Left and Right** (Q5); main quests before side quests in each.
- **391. Active quests newest first, completed ones oldest first** (Q6), within main and within side.
- **392. A quest's detail** (Q7, Q8): its description and its current stage's text; a completed quest adds a "Completed" note; past stages' texts aren't shown.
- **393. The Quests tab is the menu's first** (Q9), before Inventory: the menu opens with it highlighted.
- **394. A Quests window in the editor, in this plan** (Q10): create quests, their stages and texts, ids checked, undo through `CatalogEdit` as the other catalogs, saved with them by Ctrl+S (ADR 0014); the event dialogs read the draft, so a new quest is offered without a restart. *Interpretations flagged:* a modal window opened from the menu bar, as the event dialogs are, rather than a docked panel; four Editor slices rather than the two estimated (the catalog step, one quest's edit, the list and applying it, the window: each near the 200-line Editor size, tests counted), and removing only what the window added in the same session (removing saved quests or stages is a follow-up).
- **395. Rewards are given by events** (Q11), with `giveItem`; no reward field on a quest.
- **396. A saved quest at a stage content no longer has goes back to its first stage**, and the repair report says so (Q12); a saved quest content no longer has is forgotten, reported too.
- **397. The placeholder is one side quest with two stages** (Q13), `placeholder_errand`, using each step and each condition kind once (N1).
- **398. Story state that isn't a quest is a main-quest stage for now** (Q14: a boss beaten for good, a door opened by the plot); general flags get their own plan later. ADR 0032 says so.

Planner's picks, each marked in its slice (unnumbered):
- **The log keeps each started quest's definition, stage id and completion**, in start order (SV); conditions and the journal read definitions from it, so `WorldState` needs no catalog.
- **One start gate**: `ActiveMap.CanStart(mapEvent, leaderId)` (leader, "once" done, quest condition) replaces the three copies of the leader and "once" checks; `MapEvent.CanRunOn` (the object's look) stays apart.
- **No event shadows its object's later ones** (the review): an object event with no object, leader or quest gate that isn't "once" may not come before another event of the same activation on the same object; the repository's maps pass (the door's two events differ in activation and are both gated; the intro's are cell events).
- **The editor's condition is three boxes**: the quest ("(none)" for always), the kind, and the stage (shown for the two stage kinds, listing the chosen quest's stages in order).
- **The Quests window removes only what it added in the session**; ids are typed for new quests and stages and fixed after.
- **The category tabs are a shared `Ui/PointedTabsView`** (the third copy, after the victory's Items step and the Shop's Buy/Sell tabs, which go to the backlog); the list layout numbers are one `Ui/PageListLayout` (the third copy; the Forge's, the Inventory's and the others' go to the backlog).

## Open design questions (developer decides)
None open. The flagged interpretations in decisions 389 (reached is positional) and 394 (a modal window; four slices; removal only of what the session added) are for the developer to confirm.

## ADRs
- **ADR 0032: quest progress is its own log.** A `QuestLog` of stage ids per quest in `WorldState` rather than general story flags quests would read; story state that isn't a quest is a main-quest stage for now (decision 398). A checkbox step before QL, written from decisions 388 and 398 (nothing left to ask).
- **ADR 0033: quest conditions gate whole events.** `onlyWhenQuest` on `MapEvent` rather than a per-step condition or a branch step; five kinds (decision 389); the first event of an object that may start wins, and none may shadow its later ones. A checkbox step before QC, written from decisions 386 and 389.
- The save format change follows ADR 0022; the steps follow ADR 0006; the Quests window follows ADR 0014 (QE1 adds a dated note there and edits its README line: `quests.json` joins the catalogs Ctrl+S writes).
