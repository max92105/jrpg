# Escape menu third pass
Status: approved, in progress

Fourth feature plan after the migration, following the UI second pass (`done/2026-10-25-ui-second-pass`, retro skipped).
- Decisions 266-272 and 282-290 are recorded below. Questions 273-281 are answered (decisions 282-290); none is open; the next free number is 291.
- No ADR is needed; the next ADR is still 0026.

Developer's request (2026-10-26): the walk-through of the game menu (slice Z of the second pass) found it "far from the reference". Nine findings, in the developer's order: the tabs show a line on their left edge; the party sidebar shows on pages that don't need it; every page's content floats on the map instead of sitting on dark columns; the Party page's leader notice is a red line; the selection look is too bright and too blue. This plan answers them.

The developer approved the draft on 2026-10-26, answering every question (decisions 282-290) and adding the tabs folding into an icon rail inside a page (decision 286, TB1a and TB1b). The devil's advocate's review gave the tabs their own colors instead of pinning the battle's header, drew the tabs 1 px off-screen instead of re-baking their shape, put the glow at a fixed width, moved `MenuLayout` and the single-column width ahead of the sidebar's move (ML1), made columns flat with left edges only, removed `ItemAim`, and fixed the sidebar's and picker's member order at its source.

## Goal
At the end of this plan:
1. **One subtler selection look everywhere** (decision 270): a deep navy band fading to black, a soft white glow by the pointer, faint hairlines. The tabs run off the screen's left edge with no outline there (decision 271), and fold smoothly into an icon rail while a page is open (decision 286).
2. **Every menu page sits on dark translucent columns** from the header to the footer, with hairline edges as dividers (decision 269): a darker side column for the member column, the inventory's category rail and the party sidebar. Single-column pages are about 1100 px wide, the map showing on the right (decision 285).
3. **The party sidebar is the Inventory's alone** (decision 266), and using an item aims at its cards: the pointer moves onto the sidebar, the cards that can take the item are lit, each shows its HP now and after (decisions 267, 287), and the pointer stays there while copies remain (decision 288). The "Use <item>" band is gone.
4. **The Party page's refusals open a titled framed dialog with one OK** (decisions 268, 282, 283), drawn with the yes/no prompt's frame and button.

## Order, and why
1. **Phase 1: the shared look (LK1, C1, ML1, SB1, PC1).** The selection and tab retune first: it changes every screen at once, so every later capture is taken with it. Then `MenuLayout` (ML1): the page area beside the tabs' right edge, which TB1a will move, and the single-column width, which must be in place before the sidebar's width is handed to the pages. Then the sidebar leaves every page but Inventory (C1 makes room in `InventoryPage` first). Then the column panels, with the single-column pages as their first users.
2. **Phase 2: tabs and pages (PC2, TB1a, TB1b, IT1, IT2, IT3, C2, PD1).** The two-column pages' columns; the tabs folding (motion, then look), once every page lays out from its area; the inventory's aiming on the sidebar, staying there, then its HP preview (the one Core query); the dialog button's split right before the Party notice dialog.
3. **Phase 3: walk-through (Z).**

## Guiding rules
- **One look, everywhere (decision 252, retuned by 270).** Every selected row is still `UiTextures.DrawSelectedRow` with `MenuPointer`'s diamond at its left; LK1 changes its colors, not its callers. The tabs get their own colors (`SelectedTab*`), so `Selection` and `SelectionStroke` keep serving the battle (command header, turn order) unchanged (decision 272).
- **One column look.** Every page draws its columns with `Ui/ColumnPanelView` in one of two flat styles, `UiTheme.PageColumn` (content) and `UiTheme.SideColumn` (rails, the member column, the sidebar), over `MenuLayout.FullHeight`. A column draws its left edge; the page closes its last one. No page draws its own panel color.
- **Layout follows the tabs.** Every page lays out from the area `GameMenuScene` gives it, and that area starts at `MenuLayout.PageArea(tabsRight)`; no page reads the tabs' width. TB1a then only changes the number passed in.
- **Look at the reference and the captures first.** Before each look slice, read the shots its slice file names (`docs/ui-reference/`, `.webp` opens with Read) and today's capture in `docs/captures/2026-10-26-menu/`. Take the vibe, not a copy (decision 253).
- **No game rules in the Game.** The HP an item would leave a member with is Core's (`FieldItemUse.HpAfterUse`, IT3); the Game only shows it.
- **Pure presenter + thin view** (`docs/map.md`). Input, state and motion are tested in `Jrpg.Game.Tests`; drawing is checked by hand against captures. A draw-only change gets before/after captures, not invented tests.
- **Slice sizes.** Tests count about 13 lines each, plus the existing tests a change breaks or removes; a slice adding a view type is estimated at 1.6x; every slice stays under about 250 lines.
- **Files near 300 lines** (verify fails a changed file crossing 300): `InventoryPage` (281) splits in C1 before it grows; `PartyPage` (258), `GameMenuSceneTests` (293), `PartyPageTests` (251) and `InventoryPageTests` (325, already over) take no new tests: new tests go in new classes.
- **Mechanical slices** (C1, C2) record the solution's test count before and after; existing tests pass unchanged.
- **No allocation per frame**: previews and dialog text measured when they change, columns drawn from the plain pixel, the page area a struct computed per frame.
- **No NuGet package, no save change.**

## Out of scope (follow-ups)
- **The battle screens' look** (decision 272): they follow later on the same theme. Their selected rows change with LK1 (accepted).
- **Other red notices** (Equipment's refusal, the save failure line) as dialogs (decision 284; a backlog line).
- **A list in Equipment's right column before a slot is chosen** (decision 290).
- **Menu sub-entries, tab icon art, the tip box's notch, a "remaining" box** (decision 289).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the shared look
- [x] **LK1. The selection retuned (navy to black, a fixed glow by the pointer); tab-only colors; the tabs 1 px off-screen** (Game, about 110 lines) → [slices/LK1.md](slices/LK1.md)
- [x] **C1. `InventoryPage` split: the list column's drawing moves to `InventoryColumnView`** (Game, mechanical, about 170 lines moved, 30 new) → [slices/C1.md](slices/C1.md)
- [x] **ML1. `MenuLayout`: the page area beside the tabs' right edge; single-column pages capped at about 1100 px** (Game, about 140 lines) → [slices/ML1.md](slices/ML1.md)
- [x] **SB1. The party sidebar only on Inventory: the page owns it, the menu forgets it** (Game, about 220 lines) → [slices/SB1.md](slices/SB1.md)
- [x] **PC1. Column panels: `ColumnPanelView`; Status, Party, Options, System and the member column on them** (Game, about 170 lines) → [slices/PC1.md](slices/PC1.md)

### Phase 2: tabs and pages
- [x] **PC2. Two-column pages: Inventory's rail, list and sidebar; Equipment's sheet and choice** (Game, about 160 lines) → [slices/PC2.md](slices/PC2.md)
- [x] **TB1a. The tabs fold into an icon rail inside a page: the motion, and the page following it** (Game, about 230 lines) → [slices/TB1a.md](slices/TB1a.md)
- [x] **TB1b. The icon rail's look: icons centered in the rail, labels gone by halfway, the selected tab on the rail** (Game, about 120 lines) → [slices/TB1b.md](slices/TB1b.md)
- [ ] **IT1. Using an item aims at the sidebar's cards; the "Use item" band goes** (Game, about 220 lines) → [slices/IT1.md](slices/IT1.md)
- [ ] **IT2. The pointer stays on the sidebar while the item has copies left** (Game, about 140 lines) → [slices/IT2.md](slices/IT2.md)
- [ ] **IT3. The aimed cards show HP now and after (`FieldItemUse.HpAfterUse`)** (Core + Game, about 210 lines) → [slices/IT3.md](slices/IT3.md)
- [ ] **C2. Dialog buttons: `ConfirmPromptView`'s button drawing moves to `Ui/DialogButtonView`** (Game, mechanical, about 80 lines) → [slices/C2.md](slices/C2.md)
- [ ] **PD1. The Party page's refusals open a titled framed dialog with one OK** (Game, about 230 lines) → [slices/PD1.md](slices/PD1.md)

### Phase 3: walk-through
- [ ] **Z. Menu walk-through against the reference and the 2026-10-26 captures** (manual) → [slices/Z.md](slices/Z.md)

The developer asked for the tab folding as one slice, TB1; at about 350 lines with its tests it is split up front into TB1a (behavior with a plain draw) and TB1b (the look). The pointer staying on the sidebar (decision 288) is its own slice, IT2, so IT1 stays under 250.

## Decisions
Questions and decisions share numbers.

Taken in the request (2026-10-26), from the developer's walk-through of the second pass's menu:
- **266. The party sidebar stays only on Inventory** (answers 265). Equipment, Status, Options and System lose it; Party never had it. The member column alone marks the shown member on member pages.
- **267. Using an item aims at the sidebar's cards.** The "Use <item>" band (`ItemTargetPickerView`) goes; the pointer moves onto the sidebar, the cards that can take the item are lit, the others faded, and the lit cards show the HP the item would leave, as in `menu-inventory-target.webp`. The picking rules (`ItemTargetPicker`, Core's target choice) stay.
- **268. The Party page's leader notice becomes a titled framed dialog with one OK** (`menu-skill-learned-dialog.webp`), built on the yes/no prompt's window and button, not a copy of them.
- **269. Every page's content sits on dark translucent columns** with hairline dividers and darker side strips (`menu-inventory-description.webp`): Inventory (rail, list, sidebar), Party, Equipment, Status, Options, System and Save.
- **270. The selection look is retuned** toward `menu-tabs.webp`: a deep navy band fading to black, a soft white glow near the pointer, subtle hairlines. It stays one look (decision 252), so the title screen and the battle lists change with the menu; accepted.
- **271. The tabs have no left edge**: they run off the screen like the reference's; the top and bottom hairlines stay.
- **272. The battle screens keep today's look** in this plan (they follow later on the same theme); only their selected rows change, through 270.

The developer's answers to the draft (2026-10-26):
- **282. All three Party refusals open the OK dialog** (answers 273): the leader's, "The party is full." and "<name> is the only one standing and can't leave the party."
- **283. The dialog's title is "Party"** (answers 274), the page's name (`MessageIds.MenuParty`).
- **284. Other red notices stay** (answers 275): Equipment's refusal and the save page's failure line keep their red lines; a backlog line records them.
- **285. Single-column pages are about 1100 px wide, the map showing on the right** (answers 276), as the reference's Status page leaves its art: Status, Options, System, Save and Load. The width is `MenuLayout.SingleColumnWidth`, a named placeholder; it also keeps Status's labels near their values once the sidebar's width is freed. Party keeps its whole area (two card columns).
- **286. The tabs fold into an icon rail inside a page** (answers 277), as `menu-inventory-description.webp` shows: the labels go, the icons stay on a narrow rail at the left, and the tabs unfold when the page closes. Smoothly, not a snap: an eased motion over a short time, in `Ui/Easing`'s terms as `MenuMotion` and `SelectionGlide` are; the page's content moves left with the tabs (TB1a, TB1b).
- **287. The HP preview shows on every aimed card** (answers 278).
- **288. After a use the pointer stays on the sidebar while the item has copies left** (answers 279), as Octopath does; Cancel returns to the list; when the stack runs out, or nobody can take the item any more, the pointer returns to the list (IT2).
- **289. No "remaining" box** beside the aimed card (answers 280): the list's quantity column shows it.
- **290. Equipment's choice column draws only while choosing** (answers 281), and the sheet's width is fixed, so nothing narrows or jumps.

Earlier decisions that apply: 146 (the sidebar's cards; its "marks the shown member" part ends with 266), 252 and 253 (one selection look, the diamond pointer), 262 (motion), 112 (the picker's layout: gone with 267).

## Open design questions (developer decides)
None open. Still open from earlier plans: 92, 112 (moot once IT1 removes the band), 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

## ADRs
- **None needed.** The sidebar moving into `InventoryPage`, the column panels, the tab folding, the dialog and the retune are Game look and structure, with named constants and placeholders; the one Core addition (`FieldItemUse.HpAfterUse`) is a query on an existing rule.
