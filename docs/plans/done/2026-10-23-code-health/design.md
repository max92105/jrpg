# Code health: design

Part of [the code health plan](README.md).

## Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| Field sprite frame | `Jrpg.Data.Art.FieldSpriteLayout.FramePixels` (F5a) | 48 | One square frame of a field sprite sheet, in image pixels. It was `Game/Exploration/FieldSpriteFrame.Pixels`, with a copy in `Editor/Tiles/TileImages`. |
| Field sprite walk frames | `FieldSpriteLayout.WalkFrames` (F5a) | 8 | Columns the walk animation reads (was `WalkAnimation.FrameCount`). |
| Field sprite rows | `FieldSpriteLayout.Rows` (F5a) | 4 | One row per facing. |
| Largest active party | `Core.Party.PartyRules.MostActiveMembers` (N1) | 5 | Decision 149: the official party size, which the battle screens are laid out for. |
| Inventory rail controls | `Menus/InventoryPage`, `CategoryRail` (A1) | Left to the rail; Up/Down choose; Right or Confirm back; Cancel leaves | Question 152 placeholder. |
| Dimmed selection | `UiTheme` (A2) | 0.5 | The selection on the side without the cursor (rail or list). |
| Menu pointer | `Ui/MenuPointer` (A2) | small diamond, 12 px side, 8 px left of the line | The "you are here" mark; P2 reuses it. |
| Sidebar window | `PartySidebar.VisibleCards` (P1) | 6 | Cards shown at once: 6 × (120 + 12) px fits the 864 px page height, and seven recruits scroll. |
| Reserve look | `UiTheme` (P2) | caption "Reserve" over a hairline; cards at 0.6 opacity | Question 154 placeholder. |
| Member switch buttons | `StatusPage`, `EquipmentPage` (unchanged) | Left/Right and the shoulder buttons | Question 155 placeholder. |
| Sidebar placement | `GameMenuScene` (unchanged) | 460 px column at the right | Question 153 placeholder. |
| Column titles | `Ui/ColumnTitles` (U2) | 20 px to the hairline; rows 28 px below the titles | This was copied three times. |
| Outcome screen frame | `Battle/OutcomeFrame` (U6) | title at (104, 112), its hairline at 96, 226; 100 px footer with a hairline 170 px above the bottom | This was copied in the victory and defeat views. |

## Backlog survey

Every line of `docs/plans/backlog.md` as of 2026-10-23, checked against the code that day ("Holds" means the code still has it), and where the reviewed plan puts it. "When next touched" means the line moves to that section of `backlog.md`, with the reason given here.

### From the menu cleanup plan

| Item | Holds? | Where |
|---|---|---|
| `InventoryPage` 331 and `StatusPage` 294, not split | Holds | A1/A2 take the rail out of `InventoryPage`. **When next touched:** its rows and list view, and all of `StatusPage`. Nothing planned next needs them. |
| The private `Number(int)` copied about six times | Holds (5 copies plus `FoundItemText.Count`) | U1 |
| Text stored next to its width by hand | Holds; also `ItemTargetPickerView` and `SavePage` | U3, except `InventoryPage`, which goes **when next touched**, with its split |
| Column titles over a hairline, copied three times | Holds | U2 |
| `StatusPage`'s own slot labels | Holds | **When next touched**, with the `StatusPage` split |
| Why Core refused an equipment change | Holds | **When next touched**: it works today, and nothing planned changes equipment |
| Item target picker layout (q112) | Holds | Stays: a design question |

### From the battle plan

| Item | Holds? | Where |
|---|---|---|
| The clamped time share copied 6 times, the ease-out twice | Holds | **When next touched**: small, and no planned work touches those files |
| The footer bar drawn 3 times; the outcome screens' shared frame | Holds | U6 |
| `MessageIds` is 338 lines | Holds | **When next touched**: a table of constants, with no call site gain |
| More inline invariant formatting | Holds | U1 |
| The victory screen at the highest level shows "EXP 0/0" | Holds (no steps at the highest level; "0/(old need)" when reached in battle) | F4, decision 148 |
| Victory cards have no row limit | Holds in the code | N1: Data caps the party at 5, and five cards fit |
| Enemy attack strips are drawn unmirrored | Holds | Stays: a design question |
| "Load the latest save" and the save overlay, written twice | Holds (two copies) | Stays; to be shared in the **autosave plan**, the likely third copy |
| The banner band, drawn twice | Holds (two copies) | Stays; to be shared in the **map name banner plan**, the third copy |
| `ContentWriter.WriteAll`: moves not atomic; cleanup hides the first error | Holds | The hiding half is dropped: the cleanup's first `File.Delete` throws the same exception type, and valid content can't reach the case. F5a comments the non-atomic moves, and the line goes. |
| Space + drag pans only when the map had focus | Holds | F2 |
| `MainViewModel`: the unsaved-change questions | Holds | E3 |
| `MainViewModel`: the undo routing | Holds | E2 |
| `TileBrowserViewModelTests`: the Cut tests | Holds (410 lines) | **When next touched** |
| `MainWindow.xaml.cs` 547 | Holds | **When next touched** (F2 needs only about 15 lines) |
| Data tests: the PNG header writer copied twice | Holds | F5a |
| `TileBrowserViewModel`: the hit-testing | Holds | **When next touched** (`SheetGrid`). F1 extracts one private grid check. |
| A re-cut leaves the selection past the grid | Holds | F1 |
| `TileImages.FieldSpriteFrameOf` unvalidated; `Forget` keeps sprite frames | Holds | F5a validates at startup. The New NPC dialog check goes **when next touched**. `Forget` stays: there is no sprite import yet. |

### From the editor plan (retro)

| Item | Holds? | Where |
|---|---|---|
| `MainViewModelTests` 770 | Holds (777) | E1a does saving and undo. **When next touched:** new content and settings. |
| `ObjectShapesViewModelTests` 347 | Holds | **When next touched** |
| `CreateNpc` and `CreateObject` share three lines | Holds | E3 |

### From the demo retirement plan

| Item | Holds? | Where |
|---|---|---|
| Merge the `Directory.Build.props` files | Holds | **When next touched** (decision 144 keeps two). When it happens, use the standard form: the gates go in a root file, and `tests/Directory.Build.props` keeps its two test-only settings and imports the root through `GetPathOfFileAbove`. No path condition. |
| `ExplorationStateTests` 398 | Holds | X1 |
| The Equipment sheet narrows by 76 px | Holds | Stays; waits for question 153 (placement). |

No backlog line was stale. The code reviewer's last open warning (`verify.ps1` walking `.claude/worktrees/`) is already fixed.

## Design overview

### What exists
- **Editor:**
  - `MainViewModel` (596 lines) holds the content, the map list, the open map, saving, the import, and the undo journal and its commands.
  - `MainWindow` (547 lines) holds the view concerns.
  - `TileBrowserViewModel` handles choosing a sheet and its cells, the hit-testing, and the Cut panel.
- **Data:** `ArtReferenceCheck` reads every art file's size, but it never checks a field sprite's layout.
- **Game menus:**
  - `InventoryPage` filters through `InventoryFilter`. The category changes only through `PreviousTab`/`NextTab`, and only the controller's shoulder buttons are bound to those (question 92).
  - `StatusPage` and `EquipmentPage` each go through `MemberOrder.ActiveThenReserve` with their own cursor (Left/Right and the shoulders) and expose `Shown`.
  - The party sidebar (`PartySidebar`, `SidebarCard`, `PartySidebarView`) lists active members only, and marks none. `MemberCardView.Place` already has `IsSelected`.
- **Battle screens:**
  - `BattleLayout` is pure and places a row per party member and per enemy.
  - `VictoryScreenView` places its cards inline in 2 columns.
  - `rules.json` has `maxActiveMembers` 4, and nothing caps it.

### Core changes
Two, each in the slice that first uses it, and neither adds a namespace:
- **F4:** `ExperienceGain` gains `IsAtMaxLevel`, set by `BattleRewards` from the experience table.
  - The battle scene has no catalog, and "is this the highest level" is a Core rule (`ExperienceTable.IsMaxLevel`).
  - One test builds an `ExperienceGain` by hand.
- **N1:** `PartyRules.MostActiveMembers = 5` (decision 149). Data's validator reads it.

### The inventory rail (A1, A2)
- **Diagnosis:** the rail works only with a controller's shoulder buttons. The keyboard has no way to reach it, Left/Right do nothing on the page, and nothing shows that the rail can be chosen. The examples (`Menu7`, `Menu9`) show the rail as a place the cursor goes to, with a pointing hand on whichever side has it.
- **Behavior (A1):** a pure `Menus/CategoryRail` holds the rail's tabs, the chosen category and whether the cursor is on the rail. `InventoryPage` routes input to the rail or the list. The page doesn't grow much, because the tabs and their record move to the rail.
- **Look (A2):** `Menus/CategoryRailView` draws the rail (moved out of the page), and `Ui/MenuPointer` marks the side that has the cursor. The other side's selection is dimmed.

### The party sidebar (P1, P2)
- **The developer's answer** (145, 146): the sidebar lists all members, active and reserve, and is how the player knows and changes who they are editing.
- **Behavior (P1):**
  - The member pages implement `Menus/IMemberPage` (`Shown`), and `GameMenuScene` hands that member to `PartySidebar.Refresh`.
  - The sidebar marks the shown card and keeps a window of six cards that scrolls to it.
  - Switching member stays on the pages' existing buttons (question 155).
- **Look (P2):** the pointer, the lit card, the reserve caption and opacity, the scroll cues.
- **Placement** stays the right column until question 153 is answered. The examples' narrow portrait column would mean a new view on the same `PartySidebar`, not new behavior.

### The party of five (N1)
- **Rows fit.** The battle layout stacks a 136 px row per member: five rows end at 320 px from the top, clear of the top shade. Five enemy rows fit the same way, and six reach the turn order strip (question 151).
- **Victory cards fit.** They sit in 2 columns, 150 px apart from 420 px down, so the third row ends near 710 px, above the footer line at 910 px.
- **Data enforces the size.** It refuses a `maxActiveMembers` above 5.
- **What is tested and what is checked by hand:**
  - tests where the layout is pure: `BattleLayout`, and a new static `VictoryScreenView.CardTopLeft`;
  - a hand check with a five-member content copy for the HUD, the turn order, the Party page and the save list.

### Structure and folder dependencies

New types, each with one responsibility:

| Type | Project/folder | Responsibility | Used by | Depends on |
|---|---|---|---|---|
| `FieldSpriteLayout` | Data `Art/` | The frame grid every field sprite sheet follows, and whether an image holds it. | Data `ArtReferenceCheck`; Game `Exploration/FieldSpriteFrame`, `WalkAnimation`; Editor `Tiles/TileImages` | nothing |
| `CombinedHistory` | Editor `Shell/` | One Undo/Redo over the open map's and the catalogs' histories, in recorded order. It refuses a catalog undo whose removed record is still in use. | `Shell/MainViewModel`, `MainWindow.xaml` | `Catalogs/`, `Maps/MapViewModel`, `Shell/UndoJournal` |
| `CategoryRail` | Game `Menus/` | Which inventory category is chosen, and whether the cursor is on the rail. | `Menus/InventoryPage`, `CategoryRailView` | `Menus/CategoryTab`, `Ui/ListCursor` |
| `CategoryTab` | Game `Menus/` | One category on the rail: the category, its heading, its icon (was private in `InventoryPage`). | `Menus/CategoryRail` | nothing |
| `CategoryRailView` | Game `Menus/` | Draws the category rail, the selected tab lit, with the pointer when the rail has the cursor. | `Menus/InventoryPage` | `Ui/` |
| `MenuPointer` | Game `Ui/` | The menu's "you are here" mark, drawn left of a chosen line. | `Menus/CategoryRailView`, `InventoryPage`, `PartySidebarView` | `Ui/Diamond`, `UiTheme` |
| `IMemberPage` | Game `Menus/` | A menu page that shows one party member at a time. | `Menus/GameMenuScene`; implemented by `StatusPage`, `EquipmentPage` | Core `Party/` |
| `NumberText` | Game `Ui/` | Whole numbers written the same on every machine (invariant culture). | Menus, Saving, Battle, Dialogue, Ui | nothing |
| `ColumnTitles`, `ColumnTitle` | Game `Ui/` | A list's column captions over a hairline, measured once; and one caption and where it sits. | `Menus/InventoryPage`, `EquipmentChoiceView`, `Saving/SaveSlotListView` | `Ui/MeasuredText`, `UiTextures` |
| `FooterBar` | Game `Ui/` | The bottom bar with its top hairline that screens write their hint on. | `Menus/GameMenuScene`, `Battle/OutcomeFrame` | `Ui/UiTheme` |
| `OutcomeFrame` | Game `Battle/` | The victory and defeat screens' shared frame: dimmed battle, title hairline, footer with the hairline above. | `Battle/VictoryScreenView`, `DefeatScreenView` | `Ui/FooterBar`, `UiTextures` |
| `EditorTestContent`, `TestExplorationMap` | tests | Shared test setup: a content copy and the question log; the small exploration map. | the split test classes | |

The edges all point the allowed way:
- Game and Editor → Data → Core.
- `Ui/` reads no feature folder: `MenuPointer` uses `Ui/Diamond`.
- `Menus/` → `Ui/` and `Battle/` → `Ui/` are the existing directions.

No type is named after its folder (CA1724).

### Key decisions
1. **Bugs and the developer's feedback come before structure** (README, "Order").
2. **The fix is no bigger than the bug.** F2 is about 15 lines in the window. F1 extracts the grid check that already exists. New types come only where the next plans use them.
3. **Test splits are mechanical.** A moved body keeps its statements, apart from a prefix on shared helper names (`_content.Open`, `TestExplorationMap.Start`). The test count per project, before and after, is the guard (risk below).
4. **Behavior, then look, for each screen change** (A1/A2, P1/P2), with a pure presenter tested headless.
5. **`CombinedHistory` changes test bodies by one member path** (`viewModel.History!.UndoCommand`), rather than keeping forwarding commands on `MainViewModel`.
6. **`FieldSpriteLayout` lives in Data `Art/`.** It is a fact about art files, which Data's check needs and which Game and Editor both reference. The check matches what the Game reads: at least 8 × 4 frames of 48 px.
7. **`IMemberPage` instead of a new member on `IMenuPage`.** Only two pages show one member, and an interface with two implementers has a reason. Five other pages would otherwise return null.
8. **The sidebar is behavior plus a view,** so question 153 (placement) is answered by changing only the view.

### Extension points
- `CategoryRail` + `CategoryRailView` are what a shop page (`Menu7`) reuses for its own categories.
- `MenuPointer` is the pointer any new menu list uses.
- `IMemberPage` is what a future per-member page (skills, abilities) implements to get the sidebar's mark.
- `CombinedHistory` is where a new editor document (cutscene scripts) joins the one undo order.
- `TestExplorationMap` is where cutscene tests on `ExplorationState` start.
- `NumberText` and `MessageArgument.Number` are how gold and counts get written.

### Replacement mapping

| New | Replaces |
|---|---|
| `FieldSpriteLayout` | `FieldSpriteFrame.Pixels`, `WalkAnimation.FrameCount`, `TileImages.FieldSpriteFramePixels` and its comment |
| `CombinedHistory` | in `MainViewModel`: `_journal`, `Undo`, `Redo`, `RefusalOfCatalogUndo`, `CanUndo`, `CanRedo`, `RecordStep`, `NotifyUndoRedoChanged`, `OnMapStepRecorded`, `UndoRefusal`, and the `StepRecorded`/`RedoDropped` wiring |
| `CategoryRail`, `CategoryTab` | `InventoryPage._tabs`, `_categoryCursor`, the private `CategoryTab` |
| `CategoryRailView` | `InventoryPage.DrawRail`, `DrawRailTab` and their constants |
| `IMemberPage` | nothing: new, for the sidebar |
| `NumberText.Of`, `MessageArgument.Number` | five private `Number` copies, `FoundItemText.Count`, and about 15 inline `ToString(CultureInfo.InvariantCulture)` |
| `ColumnTitles` | `InventoryPage.DrawColumnTitles`, `EquipmentChoiceView`'s title lines, `SaveSlotListView.DrawColumnTitles` |
| `FooterBar`, `OutcomeFrame` | the footer rectangles in `GameMenuScene`, `VictoryScreenView`, `DefeatScreenView`, and the outcome screens' shared constants |

## Risks
- **A moved test silently stops running** (X1, E1a). Each split records the test count per project before and after.
- **The sidebar's `Refresh` runs every update.** It must compare the active and reserve lists in place, with no `MemberOrder.ActiveThenReserve` allocation per frame (the no-allocation rule).
- **P1 changes an existing expectation:** `Cards_ReserveMember_IsNotListed` becomes `..._IsListedAfterTheActive`. That follows decision 146, and the commit says so. It is not a weakened assertion.
- **A1 changes Left on the Inventory page.** The item target picker opened from it uses Left/Right too, but it answers input first while open (`_itemUse.IsOpen`), so they can't both act. A1 tests the order.
- **F5a may refuse today's content** if a field sprite is short. It measures them first and asks.
- **E2 touches 7 XAML bindings**, and a wrong path fails silently in WPF. The hand check presses every Undo/Redo entry point.
- **N1 may find screens that don't fit 5.** Small fixes are made in the slice; anything larger is recorded in `backlog.md` with a screenshot. Content stays at 4 (question 150), so the game as shipped can't reach a broken layout.
- **No save-format or determinism impact.** Content changes are limited to `ui.json` text (A1, P1, P2).
