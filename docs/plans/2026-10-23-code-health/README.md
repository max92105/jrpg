# Code health before the new systems
Status: approved

Developer's request (2026-10-23): "ok next plan is going through code health before going into next feature."

The developer approved the reviewed plan on 2026-10-23 ("do it"). That review did three things:
- It cut the plan to the slices the next plans need.
- It moved the other code-health items back to `backlog.md`, under "When next touched".
- It added the developer's menu feedback (the inventory filter rail, the party sidebar) and the party of five.

## Goal
At the end of this plan:
1. **The known bugs are fixed**, each with a test that fails first (or a hand reproduction where the code is window code):
   - a re-cut no longer leaves the chosen cell outside the grid;
   - Space + drag pans the map whichever panel has focus;
   - the victory card at the highest level shows a full bar and "Max" like the Status page (decision 148);
   - a field sprite too small for the walk frames is refused at startup.
2. **The menu works as the developer expects:**
   - the Inventory page's category rail can be used from the keyboard, to filter the list, and shows where the cursor is;
   - the party sidebar lists the active and reserve members and marks the member a page is showing (decisions 145, 146).
3. **The battle screens fit the official party of five** (decision 149), and Data refuses a larger party.
4. **The code the next plans touch first is in shape:**
   - the exploration tests and the editor's `MainViewModel` (cutscenes and map events);
   - the shared `Ui/` pieces (a new screen).
5. **`docs/plans/backlog.md` says, for every item not done here, why it waits and for what.**

## Order, and why
1. **Bugs (F1, F2, F4, F5a).** Correctness comes first. Each is small, and none depends on another.
2. **The menu feedback (A1, A2, P1, P2).** The developer hits these today. Each screen gets its behavior with a plain draw first, then its look.
   - The inventory rail comes first. It is the smaller one, and it adds the "where is the cursor" pointer that the sidebar's look then reuses.
   - The sidebar comes next: behavior (who is listed, who is marked, scrolling), then look.
3. **The party of five (N1)** comes after the sidebar, which must hold the seven recruits whatever the active count.
4. **The code the cutscene plan touches (X1, E1a, E2, E3):**
   - first the exploration tests;
   - then the `MainViewModelTests` split, so the `MainViewModel` split moves tests between small classes;
   - then the undo routing, then the two duplicated endings.
5. **The shared UI pieces (U1, U2, U3, U6):**
   - numbers first, since the others then format through it;
   - then column titles before measured text, which removes three widths that U3 would otherwise convert;
   - then the footer.

## Guiding rules
- **A refactor slice changes no behavior.** "Existing tests pass unchanged" is the guard.
  - In a test split (X1, E1a), a moved test keeps its body except for a mechanical prefix on shared helpers (for example `_content.Open(...)`). The test count per project is recorded before and after, and must match.
  - In E2 the undo commands move, so `viewModel.UndoCommand` becomes `viewModel.History!.UndoCommand`. That is the only change to those bodies.
- **A bug slice starts with the failing test**, and records that it failed before the fix. A test of a new API fails to compile first; F4 says so. Window code (F2) is reproduced by hand before and after.
- **Behavior, then look, for screens** (A1/A2, P1/P2): the behavior slice draws plainly and is tested headless; the look slice is checked with screenshots.
- **Draw-only moves get before/after screenshots** (`.claude/scripts/play-game.ps1`), not invented tests.
- **Code moved into a new type meets `.claude/rules/code-quality.md` in its new home.** Comments changed or moved are rewrapped (`perl .claude/scripts/reflow-comments.pl <file>`).
- **A split never fixes a bug it finds.** It notes the bug in `backlog.md`.
- **No new NuGet package.** No content value changes except the new message ids, which A1 and P1 add to `ui.json`. `maxActiveMembers` stays as it is until question 150 is answered.
- **Each slice:**
  - removes or rewrites its lines in `docs/plans/backlog.md`;
  - updates `docs/map.md` when it adds a type others build on;
  - follows `.claude/rules/tests.md`, including the refresh/cache rule for F1 and P1.
- **One slice owns a file at a time.** When an earlier slice in this plan changes a file, a later cleanup that would also change it leaves it alone and says so in the backlog. U3 leaves `InventoryPage`, which A1 and A2 change.

## Out of scope (follow-ups)
- **The new systems**, each in its own plan: cutscenes, the map name banner, autosave, event battles, gold and drops, animated battle sprites, NPC movement, audio, the loading screen.
- **Everything in `backlog.md` under "When next touched"**, each with its reason. These are:
  - the `Directory.Build.props` merge (decision 144 keeps two files for now);
  - the remaining page splits (`InventoryPage` rows and list, `StatusPage`);
  - the equipment refusal in Core;
  - the easing copies;
  - the `MessageIds` split;
  - the second `MainViewModelTests` split;
  - the `MainWindow` split;
  - the tile browser split and its Cut tests;
  - the object-shape tests;
  - the New NPC dialog's sprite check;
  - the `MainViewModel` image-size cache and import summary.
- **`ContentWriter`'s cleanup hiding the first error:** dropped. Its first `File.Delete` lands on the same directory and throws the same exception type, so no test can fail first, and valid content can't reach the case. F5a documents the non-atomic moves.
- **More enemies per encounter** waits for question 151.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Backlog survey, design overview and risks: [design.md](design.md).

Sizes are hand-written lines, tests included; "moved" lines are cut and pasted (`git diff --color-moved`).

### Phase 1: bugs
- [x] **F1. A re-cut lets go of a chosen cell past the new grid** (Editor, about 60 lines) → [slices/F1.md](slices/F1.md)
- [x] **F2. Space + drag pans whichever panel has focus** (Editor, about 15 lines; hand reproduction) → [slices/F2.md](slices/F2.md)
- [x] **F4. The victory card at the highest level shows a full bar and "Max"** (Core + Game, about 130 lines) → [slices/F4.md](slices/F4.md)
- [x] **F5a. Field sprites must hold the walk frames; the Data tests share one PNG writer** (Data + Game + Editor, about 190 lines) → [slices/F5a.md](slices/F5a.md)

### Phase 2: the menu feedback
- [x] **A1. The inventory category rail takes the cursor: Left to the rail, Up/Down filter, Right back** (Game + content, about 220 lines) → [slices/A1.md](slices/A1.md)
- [x] **A2. The rail's look: pointer on the focused side, its own view** (Game, about 140 lines) → [slices/A2.md](slices/A2.md)
- [ ] **P1. The party sidebar lists active and reserve, marks the shown member, scrolls to it** (Game + content, about 250 lines) → [slices/P1.md](slices/P1.md)
- [ ] **P2. The party sidebar's look: shown member lit, reserve set apart, scroll cues** (Game + content, about 150 lines) → [slices/P2.md](slices/P2.md)

### Phase 3: the party of five
- [ ] **N1. Data refuses more than 5 active members; the battle screens are checked with 5** (Core + Data + Game, about 120 lines) → [slices/N1.md](slices/N1.md)

### Phase 4: the code cutscenes touch
- [ ] **X1. `ExplorationStateTests` by area: interaction target and touch events** (Core tests, about 50 lines plus about 200 moved) → [slices/X1.md](slices/X1.md)
- [ ] **E1a. `MainViewModelTests` by area: shared content copy, saving and undo classes** (Editor tests, about 80 lines plus about 330 moved) → [slices/E1a.md](slices/E1a.md)
- [ ] **E2. The undo routing leaves `MainViewModel`: `CombinedHistory`** (Editor, about 300 lines; split at the size check if over) → [slices/E2.md](slices/E2.md)
- [ ] **E3. `MainViewModel`: one discard question, one "add and choose the new object"** (Editor, about 80 lines) → [slices/E3.md](slices/E3.md)

### Phase 5: shared UI pieces
- [ ] **U1. Whole numbers formatted in one place** (Game, about 120 lines) → [slices/U1.md](slices/U1.md)
- [ ] **U2. `Ui/ColumnTitles`: column captions over a hairline, once** (Game, about 120 lines) → [slices/U2.md](slices/U2.md)
- [ ] **U3. `MeasuredText` wherever text is stored next to its width (not `InventoryPage`)** (Game, about 80 lines) → [slices/U3.md](slices/U3.md)
- [ ] **U6. `Ui/FooterBar` and the battle outcome screens' `OutcomeFrame`** (Game, about 130 lines) → [slices/U6.md](slices/U6.md)

The review dropped B1, F3, F5b, E1b, E4, E5a, E5b, E6, C1, U4, U5, U7 and U8; `backlog.md` has them under "When next touched".

## Decisions
Earlier decisions this plan builds on:
- 50: keyboard and controller, no mouse;
- 82: UI sizes are placeholders in each view;
- 92 (open): the tab buttons have no keyboard key (A1 makes the rail reachable without them);
- 117: one undo order, catalog undo refused while a record is in use (E2 moves it, unchanged);
- 128, 130 and ADR 0015: an NPC sprite has `player.png`'s layout, 48 px frames, 8 by 4 (F5a);
- 139: the party sidebar is ported;
- 144: `src/` and `tests/` keep their own `Directory.Build.props`, and a backlog line waits to merge them. The review corrected a backwards citation: the merge is not done now.

The developer's answers (2026-10-23):
- **145, 146. The party sidebar** (answered in substance): "In the UI examples, the other sidebar shows all the party members and reserve, and offers navigation feedback. It's the way of selecting the next character and knowing the one I'm editing."
  - The sidebar lists the active members, then the reserve.
  - On a page that shows one member (Status, Equipment), it marks that member, and it follows as the player switches member (P1, P2).
  - What stays unclear is in questions 153-155.
- **148. The victory card at the highest level** shows experience the way the Status page does, a full bar and "Max", and is never hidden (F4).
- **149. The official party size is 5 active members.** The battle screens must fit 5 party members, and encounters may hold more monsters than today.
  - Data refuses `maxActiveMembers` above 5, and N1 checks the battle layout, the HUD and the victory cards with 5.
  - Raising the content to 5 is question 150, and the enemy count is question 151.
- **`Directory.Build.props` merge:** not chosen now (decision 144 stands; backlog).

Proposed by the architect and approved with the plan:
- **A re-cut lets go of a choice it no longer holds** (F1), as when its sheet is gone. It doesn't clamp.
- **`ContentWriter`'s moves stay non-atomic across files** (F5a adds the comment). A crash between two moves leaves a mix, and the next save rewrites everything.

## Open design questions (developer decides)
Each slice uses the placeholder until answered. Questions and decisions share numbers; next free number: 156.
- **150. Raise `rules.json`'s `maxActiveMembers` to 5 now?**
  - Placeholder: content stays at 4. N1 makes the code fit 5 and checks it with a content copy.
  - Raising it also needs a new-game party and recruits planned to fill five places: a design call.
- **151. How many enemies an encounter may hold.** Today's largest is 3. Five enemy rows fit the battle layout as it is (checked in N1); six reach the turn order strip.
  - Placeholder: no limit change until answered.
  - Once there is a number, Data refuses more and the layout is checked with it.
- **152. The inventory rail's controls.** Placeholder (A1):
  - the page opens on the list;
  - Left goes to the rail;
  - on the rail, Up/Down change the category and the list follows at once;
  - Right or Confirm go back to the list's first row;
  - Cancel on the rail leaves the page;
  - the shoulder buttons still change the category from either side.

  Alternatives: open on the rail (Octopath opens its item menu on the category); Cancel on the rail returns to the list.
- **153. Where the party sidebar sits.**
  - Placeholder: the 460 px card column on the right, as today.
  - The examples have two sidebars. One is a card panel on the right (`Menu5`, `Menu9`). The other is a narrow column of small portraits between the menu tabs and the page, with a pointing hand at the member being edited (`Menu2`, `Menu5`, `Menu6`, `Menu11`). The second is what "the way of selecting the next character" shows.
  - Moving to the portrait column would also settle the Equipment page's 76 px narrowing (backlog).
- **154. How reserve members look in the sidebar, and whether Inventory shows the sidebar.**
  - Placeholder: the reserve sits under a "Reserve" caption and hairline, its cards at 60 % opacity.
  - Placeholder: Inventory keeps the sidebar, with no member marked.
- **155. Which buttons switch member.**
  - Placeholder: what Status and Equipment already use, Left/Right and the shoulder buttons. The member pages' footer hint names them.
  - Alternative: Up/Down on the sidebar itself, as a focus area like the inventory rail.

Still open from earlier plans: 92 (keyboard keys for the tab buttons), 112 (item target picker layout), 147 (prompt key names).

## ADRs needed
None. Every slice follows an existing pattern:
- pure presenter plus view (ADR 0008);
- one undo order (ADR 0014, unchanged);
- art checks in Data (ADR 0009);
- party rules validated in Data (ADR 0002).

The next ADR number stays 0016.
