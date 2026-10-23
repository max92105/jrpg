# Retiring the WPF demo: last gaps, the word "legacy", the `legacy/` folder
Status: approved

Developer's request (2026-10-22, lightly translated): "The next plan is to remove the term legacy everywhere. What we have is our base now. At the end of it, I want the term 'legacy' removed from everywhere and the legacy folder deleted altogether. So find all the big gaps left between the two versions, make steps to address and fix them, and after that we start with real new systems and features. The modernisation and new direction in code and tech is all good."

This plan has to use the word to talk about what it removes. Plans are history (decision 134), so it keeps the word. While it is open, the guard skips its folder by name; once it moves to `done/`, the `done/` rule covers it.

## Goal
At the end of this plan:
1. **Every test passes on `main`** before any other change (slice 0). Tests on repository content assert the entries they need, never totals (`.claude/rules/tests.md`).
2. **The gaps worth porting are ported.** The survey (design.md, "Gap survey") goes through the WPF demo feature by feature. Each feature is ported, ported differently, dropped by a recorded decision, or left as a new feature for a later plan. The gaps this plan ports:
   - the **interaction prompt**: the device's Confirm key and the `Prompt` every Interact event already has. Data refuses an Interact event without a prompt, but the game never shows it;
   - the **party sidebar** beside the menu pages (decision 139);
   - **taking equipment from another member**, one row per wearer (decision 137);
   - the menu's **footer hint line** and its **page transition** (review finds; the developer can strike them, see "Slices").
3. **The codebase no longer uses the word "legacy"** (case-insensitive), and none of the documents read as current guidance do. Covered:
   - code and data: `src/`, `tests/`, `content/`;
   - the agents' setup: `.claude/` (rules, skills, agents, scripts, agent memory);
   - living docs: `docs/` outside its history, `CLAUDE.md`, `README.md`, `AI-SETUP.md` and the root build files.

   Where a comment explained a value by "legacy did it", it now gives the real reason, a decision number, or says the value is a placeholder (decision 135). ADRs 0001-0015 and plans stay as written (decision 134).
4. **`legacy/` is deleted.** Nothing references it: no project, script, rule, doc or agent. An annotated tag marks the last commit that has it, and the deletion commit names the tag (decision 142).
5. **The word can't come back** unnoticed: `verify.ps1` fails on it outside the history files (decision 136).

Then the next plans are new systems (design.md, "After this plan"; cutscenes sketched in "Cutscenes later").

## Order, and why
1. **Slice 0: green.** Five tests fail on `main` because they pin content totals that the developer changed in the editor. Every later slice needs a green start.
2. **The prompt (I1, G1a, G1b)** while `legacy/` is still there to read. I1 gives the Game the current input device and button names, which the prompt shows (decision 138) and later hints can reuse.
3. **The menu (H1, T1, P1, P2, Q1, Q2).**
   - The footer hint comes first: the sidebar and the taking of equipment both put text there.
   - The page transition comes next. It is small, and it settles how a page enters before the sidebar arrives.
   - The sidebar follows: behavior first, then its look.
   - Taking equipment comes last: Core, then the screen.
4. **The wording, by area (W1-W6).** These slices are mechanical and checked by grep: Core and Data, then the Game in three folder groups, then the editor, then docs and `.claude`. They run after the gap slices, so new code is written without the word, and before the deletion, so a comment that names a demo file can still be checked there.
5. **Deletion (X1)**, once no comment or doc needs the folder: the tag, the folder, the unused F1 debug button, the unused art. **The guard (X2)** comes last, because it fails until X1 is done.
6. **Session check (Z)**, which takes in the play-through check of the 2026-10-18 plan (decision 143).

## Guiding rules
- **Rules stay in Core (ADR 0001).** What can be interacted with, and what can be taken from whom, are Core queries. The Game only shows them.
- **Port the mechanism, keep the values as placeholders.** Values taken over from the demo are listed in design.md's tunables, marked as placeholders.
- **Wording slices change words, never behavior.** "Existing tests pass unchanged" is the guard. A renamed test keeps its body. The one code rename allowed is the private `UiTheme.FromLegacy`. Each slice ends with a grep over its area that finds nothing.
- **The reason survives.** "Legacy X did Y" becomes the reason itself, a decision number, or "placeholder". It is never simply deleted when that would leave a magic number unexplained. Nobody invents a reason that isn't known.
- **Don't refactor what a slice doesn't need.** Backlog items stay in `backlog.md` unless a slice already changes that file for its own goal (design.md, "Backlog items near this plan").
- **Pure presenter + thin view** for the Game screens: a new screen part gets its behavior with a plain draw first, then its look.

## Out of scope (follow-ups)
- **New features that the demo never had**, each in its own plan:
  - cutscenes (decision 133; sketch in design.md, "Cutscenes later");
  - the map name banner (decision 33, question 53);
  - autosave (questions 44, 67);
  - battles started by events (question 30);
  - gold and item drops (decision 103);
  - animated battle sprites (decision 100);
  - NPC movement (question 127, with its own ADR);
  - audio (question 76);
  - the loading screen (question 66).
- **The rest of question 69** (per-tile and per-object-part Z overrides, tile names, tile collision polygons, tile animation): no content uses them. The placeholder stays: dropped.
- **Code health** in `backlog.md` (design.md, "Backlog items near this plan").
- **One `Directory.Build.props` at the root** (decision 144): a backlog line, added in X1.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, the gap survey, the design overview and risks: [design.md](design.md).

### Phase 0: green
- [x] **0. Content tests assert entries, not totals** (Data + Editor tests, about 50 lines) → [slices/0.md](slices/0.md)

### Phase 1: the interaction prompt
- [x] **I1. The current input device and button names** (Game, about 150 lines) → [slices/I1.md](slices/I1.md)
- [x] **G1a. The prompt over what the player faces: Core query and plain text** (Core + Game + content, about 250 lines) → [slices/G1a.md](slices/G1a.md)
- [x] **G1b. The prompt's look: key cap, panel and fade** (Game, about 170 lines) → [slices/G1b.md](slices/G1b.md)

### Phase 2: the menu
H1 and T1 come from the plan review, not from the developer's answers. Strike either line to drop it; the slices after them still work without it (without H1, Q2 has no footer hint; its rows already name the wearer).
- [x] **H1. The footer hint line** (Game + content, about 180 lines) → [slices/H1.md](slices/H1.md)
- [x] **T1. Page transition: slide and fade** (Game, about 120 lines) → [slices/T1.md](slices/T1.md)
- [x] **P1. The party sidebar: which pages, plain cards** (Game, about 220 lines) → [slices/P1.md](slices/P1.md)
- [x] **P2. The party sidebar's look** (Game, about 150 lines) → [slices/P2.md](slices/P2.md)
- [x] **Q1. Core: what can be taken from whom, and taking it** (Core, about 170 lines) → [slices/Q1.md](slices/Q1.md)
- [x] **Q2. The equipment list offers one row per wearer, and Confirm takes it** (Game + content, about 200 lines) → [slices/Q2.md](slices/Q2.md)

### Phase 3: the word, area by area
Counts are matching lines (`grep -ic`), counted on 2026-10-22; a line can hold the word more than once (`UiTheme`).
- [x] **W1. Core and Data** (Core + Data + their tests and the simulation, 23 lines in 17 files, about 35 lines changed) → [slices/W1.md](slices/W1.md)
- [x] **W2. Game `Ui/`** (Game + Game tests, 151 lines in 17 files, about 120 lines changed) → [slices/W2.md](slices/W2.md)
- [x] **W3. Game `Battle/`** (Game + Game tests, 50 lines in 33 files, about 60 lines changed) → [slices/W3.md](slices/W3.md)
- [x] **W4. Game menus, saving, title, exploration, dialogue, input, flow** (Game + Game tests, 78 lines in 38 files, about 85 lines changed) → [slices/W4.md](slices/W4.md)
- [x] **W5. Editor** (Editor + Editor tests, 58 lines in 37 files, about 65 lines changed) → [slices/W5.md](slices/W5.md)
- [x] **W6. Docs indexes, rules and agent memory; rename one done plan; close the 2026-10-18 plan** (docs + `.claude`, 22 lines in 10 files plus renames, about 70 lines changed) → [slices/W6.md](slices/W6.md)

### Phase 4: deletion and guard
- [x] **X1. Tag, then delete `legacy/` and every pointer to it; the F1 debug button and unused art go too** (repository + Game, about 45 hand-written lines, plus the folder's 209 files and 2 images deleted) → [slices/X1.md](slices/X1.md)
- [x] **X2. The guard: `verify.ps1` fails on the word** (script + docs, about 45 lines) → [slices/X2.md](slices/X2.md)
- [ ] **Z. Session check** (manual) → [slices/Z.md](slices/Z.md)

Dropped while planning:
- Q2a (the wearer choice) and Q2b (the "Equipped by" strip): Q2 replaces both, with each worn row naming its wearer (decision 137);
- W7 (ADR wording; decision 134).

Their files say so and are deleted by the first slice that touches this folder.

## Decisions
Earlier decisions this plan builds on:
- 36: the intro cutscene stays a test hook (now superseded by 133);
- 50: keyboard and controller, no mouse;
- 51: text-only menu tabs;
- 59: the menu pages; "take it from whoever wears it" and the party sidebar were named as follow-ups;
- 69 part: NPCs as objects (ADR 0015);
- 70: map ids fixed, so no Save As;
- 73: the demo moved under `legacy/` as reference only;
- 74: leftover demo assets deleted;
- 76: audio gets its own plan;
- 82: the UI's colors and sizes live in `UiTheme`;
- 127: NPCs stand still.

The developer's answers (2026-10-22):
- **133. Cutscenes:** the demo's F1 debug intro is not ported. `GameButton.DebugCutscene` and its F1 binding are removed (X1). How cutscenes will work is sketched for a later plan (design.md, "Cutscenes later"). This plan has no cutscene code.
- **134. History stays as written; the codebase is what matters.** ADRs 0001-0015 and every plan keep their text. Only `done/2026-10-17-legacy-core-migration.md` is renamed (W6). W7 is dropped.
- **136. The guard exists** (X2). It checks:
  - `src/`, `tests/`, `content/` and `.claude/`;
  - `docs/` outside its history;
  - the root files.

  The history it skips:
  - `docs/plans/done/`;
  - this plan's folder while it is open;
  - ADRs 0001-0015, listed by file name. ADRs from 0016 on are checked.

  The script builds its pattern in pieces, so it doesn't contain the word itself.
- **137. Taking equipment:** any member, active or reserve, can give up an item; the receiver's own item goes back to the bag. Shape:
  - one worn row per wearer ("Bronze Sword (Sarxian)");
  - Confirm calls `TryTakeFrom` directly;
  - no wearer choice, no "Equipped by" strip;
  - `TakeableFor` leaves out the item the member already wears in that slot.
- **138. The prompt shows the device's key:** the key or pad button bound to Confirm on the device in use (decision 50), then the prompt text. The Game's input layer tracks the device and names the buttons (I1).
- **139. The party sidebar is ported** ("I like it"): behavior, then look (P1, P2). Where it shows follows the demo (design.md, tunables). The undecided parts are questions 145 and 146.
- **140. The vector icons stay in git history** (the tag finds them). No export.
- **141. Unused art is removed:** `content/art/portraits/fighter.png` and `content/art/icons/TargetArrow.png`. Checked 2026-10-22: nothing in `content/`, `src/` or `tests/` references them. X1 checks again before deleting.
- **142. The deletion is allowed, and the departure from the demo is visible in git.** In X1, Claude creates an annotated tag `wpf-demo-final` on the last commit that still has `legacy/`, with a message naming it as the last state with the WPF demo. The deletion commit's message names the tag. Nothing is pushed. The developer gave permission on 2026-10-22, recorded here.
- **143. The 2026-10-18 plan:** its play-through check is folded into this plan's Z where relevant. Its retro is skipped, and W6 moves it to `done/`.
- **69 (rest):** not answered; the placeholder stays: dropped.

Decided by the architect (the developer delegated these, 2026-10-22):
- **135. What replaces the word.** Code comments give the real reason, a decision number, or "placeholder" for a value nobody designed. They never point to a demo file. Docs that must name the old program say "the WPF demo".
- **144. Build props:** `src/` and `tests/` keep their own `Directory.Build.props`. X1 rewrites their comments and adds a backlog line to merge them at the root.

## Open design questions (developer decides)
Each slice uses the placeholder until answered. Questions and decisions share numbers; next free number: 148.
- **145. Party sidebar placement.** Placeholder, the demo's layout:
  - a 460 px column on the right with a 24 px gap, on every page except Party (which already shows the whole party);
  - not on the main menu (no page open);
  - hidden while a page shows a side panel of its own (the equipment choice).

  The page area narrows to the left of it, so Status, Inventory and Equipment lay out in 484 px less width. The alternative is a narrower column (for example portraits and bars only), or showing it on fewer pages.
- **146. What a sidebar card shows.** Placeholder, the demo's `PartyMemberCard`: portrait, name, level, HP and MP bars with numbers, active members only, in party order. Show reserve members too, dimmed?
- **147. Key names in the prompt.** Placeholder: Confirm's first binding per device ("E" on the keyboard, "A" on a controller), drawn in a small key cap. The device in use starts as the keyboard and switches on the first press of the other device. Alternatives: start on the controller when one is connected, or use button glyph images later (art).

## ADRs needed
None. The prompt, the sidebar, the hints and taking equipment use existing patterns (Core queries, pure presenters; ADR 0001, 0003, 0008, 0010). The guard is tooling. Decision 134 is a documentation policy, recorded above. A cutscene plan will need its own ADR (design.md, "Cutscenes later"). The next ADR number stays 0016.
