# UI second pass and clean-up
Status: approved

Third feature plan after the migration, following the class mechanics plan.
- Decisions 231-235 and 249-264 are recorded below. Questions 236-248 are answered (decisions 249-263); question 265 is open; the next free number is 266.
- This plan writes ADR 0025 (font roles); the next ADR is 0026.

Developer's request (2026-10-25, translated): "a second pass on UI and clean-up". The larger direction, for later plans: an investor demo, "something I can show to people to get funding for an artist to come in and clean up": clean code, clean UI/UX, some animation, music, a playable Act 1 with no bugs.

The developer approved the draft on 2026-10-25, answering every question (decisions 249-263). The devil's advocate's review added the capture key (S0), dropped the reference ADR and the curated copy, moved the titled window to W1b, the detail line to M4 and the footer clean-up to the end, and renamed `MenuEntrance` to `MenuMotion`.

## Goal
At the end of this plan:
1. **The reference material is usable.** The screenshots live in `docs/ui-reference/` under one naming scheme, with a README saying what each shows, and a short index names the export's files per screen. A debug key captures our own screens at 1920 x 1080, so every look slice is specced and checked against real pictures.
2. **The first menu opening is smooth.** The stall that skips the menu's fade the first time is found and fixed.
3. **Every screen is closer to the reference**, and coherent with itself (decision 252):
   - text, numbers, rows and panels sized toward the reference's proportions;
   - one selection look everywhere (the blue glow band between hairlines, the diamond pointer);
   - a serif for big titles; a title screen that fades in with a clear selection;
   - framed windows (the battle's action name, yes/no dialogs, the dialogue box);
   - an inn that fades to black;
   - a menu that slides in and fades out, with tab icons and a member portrait column; selections that glide and pointers that bob, on the tabs, the title and every page list;
   - a Status page showing level, class, resources and skills, in spaced-caps labels beside large numbers; an inventory with column titles and the description under the selected row;
   - an encounter that breaks the field like glass into the battle.
4. **The code the look slices touch is clean first**: `UiTheme` in partial files; `EventPresenter`, `StatusPage` and `InventoryPage` split right before their look slices; the footer and time-share copies on the shared helpers last.

## Order, and why
1. **Phase 0: capture, bug, reference (S0, BG1, R1, R2).** The capture key comes first: every later slice reads the captures. The first-menu-open bug is next (decision 259). Then the folder rename and the index.
2. **Phase 1: sizing (SZ1, SZ2).** Early, so every look slice after it works at the new sizes (decision 251). Menus first, since "these kinds of games are mostly menuing" (decision 249).
3. **Phase 2: fonts, pointer, title (ADR 0025, T1, T2, P1a, T3).** P1a makes every list use one pointer and one selection look (decision 252); T3 builds on it, not on T1.
4. **Phase 3: windows and the map (C1, W1a, W1b, W2, C3, X1).** C1 right before W1a, the first slice adding theme colors; C3 right before X1.
5. **Phase 4: the menu (MO1, TS, MO2a, C5, MO2b, M4, C4, M3a, M3b, M2a, M2b, M1).** Motion first (it touches the frame every page shares), then pages get a time step so their lists glide too, then each page's split right before its first slice.
6. **Phase 5: the encounter (B1a, B1b, B1c).** Capture and flash, the frame breaking into shards, the shards flying apart.
7. **Phase 6: clean-up and walk-through (C2, Z).** C2 last: nothing after it adds timing or footers.

## Guiding rules
- **One look, everywhere (decision 252).** Every selected row is `UiTextures.DrawSelectedRow` (the blue glow band between hairlines) with `Ui/MenuPointer`'s diamond at its left; a tab is the same gradient in the tab shape. No screen draws its own selected state. Every look slice checks its screen against this, and `Z` checks all of them.
- **Look at the reference and our captures first.** Before each look slice, read the screenshots its slice file names (`docs/ui-reference/`, `.webp` opens with Read), the index's row for its screen, and the S0 captures of today's screen. Take the vibe, not a copy (decision 253). Open the export only one subfolder at a time: walking it whole times out.
- **Other games' assets are reference only** (decision 264, a line in `.claude/rules/game.md`): never committed, loaded or shipped. The game bakes its own shapes in `Ui/`.
- **No game rules change.** Game and docs, plus content a screen needs (a font, a few `ui.json` messages). No Core, Data or save change: the Status page reads what Core already exposes.
- **Pure presenter + thin view** (`docs/map.md`). Every timing or layout type is pure and tested in `Jrpg.Game.Tests`; drawing is checked by hand against captures. A draw-only change gets before/after captures, not invented tests.
- **Slice sizes.** Tests count about 13 lines each, plus the existing tests a change breaks. A slice that adds a view type is estimated at 1.6x (the class mechanics plan's LK1 and LK2); every slice here is estimated that way and stays under about 250 lines.
- **Files near 300 lines split right before the slice that grows them** (verify fails a changed file crossing 300). Known ones: design.md, "Clean-up selection".
- **Mechanical slices** (C1, C3, C4, C5) record the solution's test count before and after; existing tests pass unchanged.
- **No allocation per frame**: text measured when it changes, shapes baked once in `UiTextures`.
- **Ui depends on nothing but Rendering** (`.claude/rules/game.md`).
- **No NuGet package.** The serif is a TTF the developer downloads into `content/art/fonts/titles/` (ADR 0025).
- **Determinism.** The shatter's shards come from a seeded layout in the Game (decision 258); no wall clock outside the host (the capture file name).

## Out of scope (follow-ups)
- **Music and sound effects**, the inn's music included (decision 257: later).
- **Act 1's content**, the story, maps and bug hunting (later plans).
- **Character animation** (animated battlers, walk cycles). This plan's motion is UI motion only.
- **The map name banner** (its own plan, which shares the escaped notice's band).
- **New screens**: shop, world map, skill learning, a loading screen.
- **Menu sub-entries** beside a tab: no tab has entries yet.
- **Tab icon art**: M1's icons are placeholders we draw (decision 254).
- **Editor clean-up**: no slice here touches the editor; the backlog keeps it.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview, clean-up selection and risks: [design.md](design.md).

### Phase 0: capture, bug, reference
- [x] **S0. A debug key saves the 1920 x 1080 screen as a PNG outside the repo; the developer captures today's screens** (Game, about 110 lines) → [slices/S0.md](slices/S0.md)
- [x] **BG1. The first menu opening stalls and skips its fade: find the cause, warm it up, test it** (Game, about 150 lines) → [slices/BG1.md](slices/BG1.md)
- [x] **R1. `docs/ui-reference/`: the folder renamed, the screenshots renamed, a README; the reference-only rule** (docs + comments, about 90 lines) → [slices/R1.md](slices/R1.md)
- [x] **R2. The export index: 2-4 named files per screen and the idea to take** (docs, about 60 lines) → [slices/R2.md](slices/R2.md)

### Phase 1: sizing
- [x] **SZ1. Sizing pass, menus: text, numbers, rows, panels toward the reference** (Game, about 230 lines) → [slices/SZ1.md](slices/SZ1.md)
- [x] **SZ2. Sizing pass, title, dialogue, battle and outcomes** (Game, about 200 lines) → [slices/SZ2.md](slices/SZ2.md)

### Phase 2: fonts, pointer, title
- [x] **ADR 0025: Font roles, with Cormorant Garamond for titles** (before T1) → [slices/ADR-0025.md](slices/ADR-0025.md)
- [x] **T1. A title font; the game's name on the title screen in it** (Game + content font, about 160 lines) → [slices/T1.md](slices/T1.md)
- [x] **T2. The title font elsewhere: "Victory!", the defeat title, the leader's name** (Game, about 80 lines) → [slices/T2.md](slices/T2.md)
- [x] **P1a. One pointer and one selection look on every list** (Game, about 170 lines) → [slices/P1a.md](slices/P1a.md)
- [x] **T3. Title entrance and a clear selection: the screen fades in, entries in the shared selection look, a footer line** (Game, about 220 lines) → [slices/T3.md](slices/T3.md)

### Phase 3: windows and the map
- [x] **C1. `UiTheme` in partial files by screen** (Game, mechanical, about 300 lines moved, 30 new) → [slices/C1.md](slices/C1.md)
- [x] **W1a. The untitled framed window, first used by the battle's action name** (Game, about 200 lines) → [slices/W1a.md](slices/W1a.md)
- [x] **W1b. The titled framed window with its separator; yes/no prompts become a centered framed dialog** (Game, about 220 lines) → [slices/W1b.md](slices/W1b.md)
- [x] **W2. The dialogue box in the frame language** (Game, about 130 lines) → [slices/W2.md](slices/W2.md)
- [x] **C3. `EventPresenter` split: the run of stills moves out** (Game, mechanical, about 220 lines) → [slices/C3.md](slices/C3.md)
- [x] **X1. The inn fades to black, holds, then says the party rests** (Game, about 170 lines) → [slices/X1.md](slices/X1.md)

### Phase 4: the menu
- [x] **MO1. The menu slides in and fades out (`MenuMotion`)** (Game, about 240 lines) → [slices/MO1.md](slices/MO1.md)
- [x] **TS. Menu pages advance with the menu's time** (Game, about 130 lines) → [slices/TS.md](slices/TS.md)
- [x] **MO2a. The selection glides and the pointer bobs: the menu tabs and the title** (Game, about 210 lines) → [slices/MO2a.md](slices/MO2a.md)
- [x] **C5. `InventoryPage` split: `InventoryRow` and `InventoryListView`** (Game, mostly moved, about 200 lines) → [slices/C5.md](slices/C5.md)
- [x] **MO2b. Page lists glide and bob: items, equipment, options, save files** (Game, about 200 lines) → [slices/MO2b.md](slices/MO2b.md)
- [x] **M4. Inventory look: column titles, the description under the selected row** (Game, about 210 lines) → [slices/M4.md](slices/M4.md)
- [x] **C4. `StatusPage` split: `StatusSheet` and `StatusSheetView`** (Game, mostly moved: about 300 lines moved, 40 new) → [slices/C4.md](slices/C4.md)
- [x] **M3a. Status page content: level, class, speed, resources and skills** (Game, about 230 lines) → [slices/M3a.md](slices/M3a.md)
- [x] **M3b. Status page look: spaced-caps labels beside large numbers, hairline rows** (Game, about 170 lines) → [slices/M3b.md](slices/M3b.md)
- [ ] **M2a. The member column: which members a page goes through, which one is shown** (Game, about 130 lines) → [slices/M2a.md](slices/M2a.md)
- [ ] **M2b. The member column drawn between the tabs and member pages** (Game, about 210 lines) → [slices/M2b.md](slices/M2b.md)
- [ ] **M1. Tab icons: placeholder glyphs we draw** (Game, about 230 lines) → [slices/M1.md](slices/M1.md)

### Phase 5: the encounter
- [ ] **B1a. The encounter freezes the field's last frame and flashes before the fade** (Game, about 200 lines) → [slices/B1a.md](slices/B1a.md)
- [ ] **B1b. The frozen frame breaks into shards (`ShatterLayout`)** (Game, about 240 lines) → [slices/B1b.md](slices/B1b.md)
- [ ] **B1c. The shards fly apart over the fade into battle** (Game, about 180 lines) → [slices/B1c.md](slices/B1c.md)

### Phase 6: clean-up and walk-through
- [ ] **C2. Footer hints on `FooterBar.TextTop`, time shares on `Easing`** (Game, about 120 lines) → [slices/C2.md](slices/C2.md)
- [ ] **Z. Demo walk-through against the captures and the index** (manual) → [slices/Z.md](slices/Z.md)

## Decisions
Questions and decisions share numbers.

Taken in the planning session (2026-10-25):
- **231. The extracted UI is reference only.** The umodel export of Octopath Traveler II's UI (`C:\Users\max92\Downloads\umodel_win32\Export\Game\UserInterface\`, 2.4 GB, 6,818 PNGs) stays outside the repository: never committed, never loaded by the game, never shipped. Generalized by decision 264.
- **232. An index page, no copy** (amended at approval by the devil's advocate's review). The repository gets one index page in `docs/ui-reference/` naming 2-4 export files per screen and the idea to take from each. The curated copy outside the repository is dropped.
- **233. The game draws its own versions.** Diamonds, pips and marks are baked procedurally in `Ui/`; new pieces are too. The export and the screenshots give the vibe, proportions and colors.
- **234. The investor demo is the direction**, for later plans: clean code, clean UI/UX, some animation, music, a playable Act 1 with no bugs.
- **235. Screenshot names:** lowercase, words joined by hyphens, the screen first and then the detail, no "exemple" (`battle-turn-order.webp`). A number ends the name only when several shots show the same thing. The table is in design.md.

The developer's answers to the draft (2026-10-25):
- **249. Every existing screen matters equally** (answers 236): "these kinds of games are mostly menuing". The menu phase stays in this plan.
- **250. Fonts** (answers 237): "look at the reference and go close to that". One open-license serif for big titles, chosen by the plan: **Cormorant Garamond** (SIL Open Font License, Google Fonts), its SemiBold weight, which the developer downloads into `content/art/fonts/titles/` (ADR 0025). No bold header role. Damage numbers stay in the sans.
- **251. A sizing pass** (answers 237): "right now everything is too small, use more of the screen." Base text sizes, numbers, row heights and panel widths rise toward the reference's proportions on 1920 x 1080, specced from S0's captures, with before/after captures as the hand check (SZ1, SZ2).
- **252. The title screen near the reference, and one selection look everywhere** (answers 238): "right now the menu is just my art with the options and it's hard to know what is selected"; "the UI/UX should be coherent with itself everywhere." The selected entry reads clearly through the shared selection treatment (blue glow band and hairline), on the title as on every list (guiding rule, P1a, T3).
- **253. Keep the diamond pointer** (answers 239): "we don't have to be one-for-one with the example; see the vibe, what I like, what I want us to be close to." P1 becomes P1a: every list on `MenuPointer`, no hand.
- **254. Tab icons, as placeholders we draw ourselves** (answers 240): baked glyphs listed beside the menu's tabs (M1).
- **255. A member portrait column like the reference** (answers 241): M2a and M2b.
- **256. The dialogue box takes the new frame** (answers 242): W2.
- **257. Rest: fade to black, hold, "the party rests"** (answers 243): X1. Music comes later.
- **258. The encounter breaks like glass** (answers 244): a fade, "ideally a screen break, like glass shattering". The field's last frame is captured, broken into shards by a seeded, Core-free layout and flown apart over the fade into battle (B1a-B1c).
- **259. Fix the first menu opening** (from the answers): "when I press Esc for the first time to open the menu, the fade doesn't work, only the first time; it feels like a lag." BG1, first in Phase 0 after the capture key.
- **260. The Status page shows what the reference's does** (answers 245): level, class, resources, skills and the like (`menu-status.webp`). M3a (content), then M3b (look).
- **261. "HP" everywhere** (answers 246): the Equipment page's "Max HP" becomes "HP" (SZ1, which already edits its labels' sizes).
- **262. "Make the animation smooth"** (answers 247): menu motion as planned, the selection glide kept, and pages get a time step so page lists glide and pointers bob too; motion is the same on tabs, title and page lists (TS, MO2a, MO2b).
- **263. The screenshot folder becomes `docs/ui-reference/`** (answers 248): one rename in R1, updating CLAUDE.md, `.claude/rules/game.md`, this README and the code comments that name it.
- **264. Other games' assets are reference only** (devil's advocate, replaces the drafted ADR 0025 on reference material): never committed, loaded or shipped; one line in `.claude/rules/game.md` "UI look" (R1).

Earlier decisions that apply:
- 56: the target pointer bobs.
- 81, 82: the game menu's tabs, its opening fade and the placeholder layouts.
- 84: images no content file names are listed in `Ui/UiArtFiles`.
- 101: only damage has an animation; anything else shows a cue with the action's name.
- 146: the party sidebar marks the member a page shows.

## Open design questions (developer decides)
Each slice uses the placeholder until answered.
- **265. The party sidebar beside the member column** (reopens 153 for M2b). Placeholder: the sidebar stays on the right on member pages and the column sits between the tabs and the page, so the page narrows by the column's width. Alternatives: the column replaces the sidebar's marking on member pages; the sidebar shows only on item use.

Still open from earlier plans: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

## ADRs
Each is a checkbox step right before the first slice that needs it.
- **0025 Font roles** (before T1), written with Cormorant Garamond (decision 250).
  - **A. A subfolder per role (recommended).** `content/art/fonts/` stays the body font and its fallbacks; `content/art/fonts/titles/` holds the title face, loaded as its own font system that falls back to the body files. `FontLibrary.TitleFontAt(pixels)` beside `FontAt`. No new content file.
  - **B. A `fonts.json` naming a file per role**, with a schema and a validator in Data. More flexible, a new content file for two fonts.
  - **C. A file-name convention** (`Title-*.ttf`). Nothing tells a reader the prefix means something.
- **None needed for:** reference material (decision 264, a rule line); the framed window, the pointer, the motion timings, the shatter's drawing (Game look, named constants; the shatter's triangles are drawn with MonoGame's own primitives, no package); the splits.
