# Code health cleanup: the Game
Status: approved, in progress

Developer's request (2026-11-11), verbatim: "it's time to do all the health check listed and do a clean up phase. dead code optimisation using clean code convention and microsoft convention. to be most human readable possible. using clear strucutre and segmentation and intention. everything work really well so no regression."

The developer chose two plans (decision 536): [2026-11-11-code-health-cleanup](../2026-11-11-code-health-cleanup/README.md) is the editor and Data phase; this one covers the backlog lines about `src/Jrpg.Game` and `tests/Jrpg.Game.Tests`, and follows it. The developer approved it on 2026-11-11 with the devil's advocate's changes (decisions 550-558).

## Goal
At the end of this plan:
1. **The Game's known bugs are fixed**, each with a test that fails first or a hand check where the change is a look:
   - the Inventory's Weapons and Armor tabs show no quantity column title over rows that have no count (loot plan, P1a);
   - a dialog a menu page opens dims the footer hint with the rest of the menu (escape menu third pass, PD1);
   - the UI art check compares exact file names, as the UI sound check does (music plan, P1).
2. **The Game files every next Game plan opens are in shape:** `Ui/MessageIds.cs` (767 lines) in partial files by feature; the three Game test files over 300 lines (`CommandMenuTests` 393, `GameFlowTests` 321, `EventPresenterTests` 306) split by behavior; one test builder for `EventPresenter` instead of 20 hand-built ones.
3. **The Game's dead members are listed with their fate**, as the first plan's X1 does for Data and the editor.
4. **The Options page, which the settings-kept-between-runs plan opens first, reads at one level**: its volume pips drawn by their own method, its tests reaching rows by what they are.
5. **`docs/plans/backlog.md` says, for every Game line this plan doesn't do, why it waits.**

## Order, and why
1. **Bugs (GB1-GB3).** Each before any other change to its file; none depends on another.
2. **Splits and test duplicates (GS1, GT1, GT2, GD1, GT3).** `MessageIds` first, since every later Game plan adds ids to it. Then the test files over 300: the flow and the battle menu (independent), then the `EventPresenter` builder (GD1) before the `EventPresenterTests` split (GT3), so the split moves tests that already use it.
3. **Dead code (GX1).** After the splits, so its listing reads the files as they stay.
4. **Readability (GR1).** The Options page, then the closing pass over the backlog.

Each phase ends green: every slice builds, passes every test, and passes `verify.ps1` (with the first plan's V1 check, if it has landed).

## Guiding rules
The first plan's rules hold (decision 551):
- **A refactor slice changes no behavior.** The guard is the test runner's total per project before and after, equal, with the moved test names listed in the Done note (decision 537); never a count of test methods. A moved test keeps its body except for a mechanical prefix on a shared helper.
- **A bug slice shows its test can fail** (decision 538); a look change is checked by hand by the developer. Agents take no captures: the look is the developer's.
- **A move is only a move** (decision 541): moved comments over 120 columns rewrapped, moved code lines over 130 wrapped; nothing else changes. A readability change touches only lines that do two things (decision 539).
- **No changed file crosses 300 lines; slices stay near 200 lines** of hand-written change, moved lines counted apart.
- **Each slice** removes or rewrites its `backlog.md` lines and updates `docs/map.md` when it adds a type or partial file others build on.
- **No NuGet package, nothing in `content/`, no Core change.**

## Out of scope
- **Every Game line the next Game work won't touch first**, which stays in `backlog.md` with its reason (design.md, "Backlog survey"). Among them:
  - the files at the limit with their own "split before the next growth" rule (`EventPresenter` 294, `PartyPage` 296, `ActionPlayback` 292, `BattleScene` 290, `ShopScene` 288, `CommandPanelView` 281, `VictoryScreenView` 277);
  - the shared `Ui` drawers waiting for a third copy (the tip strip, the scroll cues, the titled dialog frame, the rarity color, row text centering, the yes/no question);
  - the battle's look leftovers (unmirrored enemy strips, the skill hint's wrap, pointers drawn at the cursor's row), which follow the battle's own look plan (decision 272);
  - the music's joins and streaming (ADR 0035 follow-ups if heard).
- **The editor, Data and Core lines**: the first plan, or the backlog with their reasons.
- **Features named in the backlog:** a `--start` debug option, the inventory hint's `{button}`, a `TurnKept` event.

## Slices

One line per slice; each slice's goal, files, moves and checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Backlog survey, design overview and risks: [design.md](design.md).

Sizes are hand-written lines, tests included; "moved" lines are cut and pasted (`git diff --color-moved`) and counted apart.

### Phase 1: bugs
- [x] **GB1. The Weapons and Armor tabs show no quantity column** (Game, about 50 lines, hand check) → [slices/GB1.md](slices/GB1.md)
- [x] **GB2. A page's dialog dims the footer hint too** (Game, about 5 lines, hand check) → [slices/GB2.md](slices/GB2.md)
- [x] **GB3. The UI art check compares exact file names** (Game tests, about 15 lines) → [slices/GB3.md](slices/GB3.md)

### Phase 2: splits and test duplicates
- [x] **GS1. Moves only: `MessageIds` in partial files by feature** (Game, about 770 lines moved, about 30 new) → [slices/GS1.md](slices/GS1.md)
- [x] **GT1. `GameFlowTests` split: the new game's tests into `GameFlowNewGameTests`** (Game tests, about 110 moved) → [slices/GT1.md](slices/GT1.md)
- [x] **GT2. `CommandMenuTests` split by menu entry over a `CommandMenuDriver`** (Game tests, about 50 new, 170 moved) → [slices/GT2.md](slices/GT2.md)
- [ ] **GD1. One `EventPresenters.Over` builder for the 20 hand-built presenters** (Game tests, about 60 lines) → [slices/GD1.md](slices/GD1.md)
- [ ] **GT3. `EventPresenterTests` split: eight stills tests into `EventPresenterStillsTests`** (Game tests, about 100 moved) → [slices/GT3.md](slices/GT3.md)

### Phase 3: dead code
- [ ] **GX1. Game dead code with evidence** (Game, size set by the listing; about 40 lines expected) → [slices/GX1.md](slices/GX1.md)

### Phase 4: readability
- [ ] **GR1. The Options page's pips drawn apart; its tests reach rows by name; the backlog closed** (Game, tests, docs, about 60 lines, hand check) → [slices/GR1.md](slices/GR1.md)

## Decisions
- **550. Scope: the Game's backlog lines that the next Game work touches first**: `MessageIds` (every Game plan adds ids), the Options page (the settings-kept-between-runs plan, decision 415, is the next known Game plan), the Game test files over 300, and the known bugs. Every other Game line stays in `backlog.md` with its reason.
- **551. The first plan's rules hold here**: decisions 537 (the guard), 538 (bugs show their test failing), 539 (the line rule), 541 (a move wraps what it moves). A look change is checked by the developer by hand; agents take no captures.
- **552. `MessageIds` stays one static class in partial files by feature**, so no call site and no test changes: `MessageIds.cs` (startup, title, leader choice, cutscenes, the menu's frame, System and the shared Yes, No and OK), `.Field` (found items, recruits, rests, the inn, quest notices), `.Stats` (stat, damage type, affix, rarity and slot names, which several screens share), `.StatusPage`, `.Menus` (Inventory, Equipment, Party, Options, Save, Quests and the menu hints), `.Battle`, `.Crafting` (the Forge), `.Trade` (the Shop). A new id goes in the file of the screen that shows it; one several screens show goes in `.Stats` or `MessageIds.cs`.
- **553. The Weapons and Armor tabs show no quantity column title**, since every row there is a gear piece with no count (loot plan, P1a): `InventoryFilter.ShowsCounts(category)` says whether a category's rows carry a count, false for Weapons and Armor (every row a piece), and the column view draws its titles with or without Qty by it.
- **554. A page's dialog dims the footer hint**: `GameMenuScene.Draw` draws the hint before the page, as it draws the header, so a dialog's scrim covers it like the rest of the menu.
- **555. A repository file check compares exact names from the folder's listing**, never `File.Exists` (which ignores case on Windows): `UiArtFilesTests` as `UiSoundFilesTests` does.
- **556. A Game test file over 300 splits into sibling classes by behavior** (the repository's way since the quests plan: `EventPresenterQuestTests`, `ShopSceneBuyTests`), not partial classes; a helper both sides use is read from an existing test helper (`StartMapScenarios.TestWorld`) or a new static driver (`CommandMenuDriver`, as `ShopDriver`), and its callers change by a mechanical prefix. A sibling of a class in `[Collection(RepositoryScreenText.CollectionName)]` carries the same attribute (FontStashSharp's font cache is not thread-safe, and the totals guard can't see a lost attribute).
- **557. `ScreenFade`'s long-frame skip stays as it is**: `JrpgGame` runs a fixed step (`IsFixedTimeStep = true`, `TargetElapsedTime` 1/60 s), and MonoGame's `Game.Tick` (develop branch) passes `ElapsedGameTime = TargetElapsedTime` to every catch-up `DoUpdate`, its backlog capped at 500 ms, so no update is longer than a fade's hold; the two backlog lines merge into one that says so, to reopen if the step is ever made variable.
- **558. The Game's dead members follow the first plan's X1 rules**: listed by the same method, deleted when no `src/` caller remains and their tests move with a mechanical edit, a backlog line otherwise.

## ADRs needed
None.
