# Escape menu third pass: design

Part of [the escape menu third pass plan](README.md).

### Tunables and placeholders

Every value is a placeholder, measured on the reference shots scaled to 1920 px (the menu shots are 1200 px: x1.6) and on `docs/captures/2026-10-26-menu/`. Screen values are named constants in the type that uses them; colors in `UiTheme` (`#AARRGGBB`, straight alpha).

**Selection and tabs** (LK1; decisions 270, 271)

| Name | Where | Today | Placeholder | Meaning |
|---|---|---|---|---|
| `SelectedRow` | `UiTheme` | blue `#E64A53A8` to `#1A6E78C8` | `0 #D90E1430`, `0.3 #C0141A3C`, `0.75 #700A0D20`, `1 #000A0D20` | Every selected row's band, navy to black |
| `SelectionGlow` (new) | `UiTheme` | | `0 #00FFFFFF`, `0.35 #40FFFFFF`, `1 #00FFFFFF` over `SelectionGlowWidth` = 160 px from the row's left (narrower rows: the row's width) | The soft glow by the pointer, the same size on a short button and a long row |
| `SelectionHairline` (new) | `UiTheme` | `HighlightLine` | `0 #00FFFFFF`, `0.08 #80E6EAFF`, `0.5 #33E6EAFF`, `1 #00FFFFFF` | The selected row's top and bottom lines; `HighlightLine` stays for the tip strips and the battle's lines |
| `SelectedTab` (new) | `UiTheme` | `Selection` | `0 #F00A0D1E`, `0.55 #F0141A3C`, `0.88 #E03A4472`, `1 #B08C96C8` (the glow at the point; the tab's width is fixed) | The selected tab only |
| `SelectedTabStroke` (new) | `UiTheme` | `SelectionStroke` | `#59D6DCFF` | The selected tab's outline only |
| `SelectedTabLine` | `UiTheme` | `SelectionLine`, peaks `#FFFFFFFF` | renamed, peaks `#B3FFFFFF` | The line under the selected tab |
| `Selection`, `SelectionStroke` | `UiTheme` | | unchanged | The battle's command header, the turn order's current diamond and extra-turn mark, Party's moving outline |
| `OffScreenEdge` | `Menus/MenuTabsView` | | 1 px: every tab drawn 1 px left of the screen, so its outlined first column is off-screen | Decision 271; `NavTabShape` unchanged |
| Readability fallback | LK1's hand check | | if a selected row isn't told apart at a glance: `SelectionHairline`'s peak `#80` → `#B3`, then the glow's `#40` → `#66` | |

**Layout and columns** (ML1, PC1, PC2; decisions 269, 285, 290)

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| `PageArea(tabsRight)` | `Menus/MenuLayout` | from `tabsRight` + 24 to 40 px from the screen's right; from 40 px under the header to 32 px over the footer | Today's area, with its left edge following the tabs |
| `SingleColumnWidth` | `Menus/MenuLayout` | 1100 px (`SingleColumn(area)` caps the width, keeps the left) | Status, Options, System, Save, Load (decision 285) |
| `FullHeight(area)` | `Menus/MenuLayout` | the same left and width, from the header's bottom (88) to the footer's top (1008) | Every column |
| `PageColumn` | `UiTheme` | fill `#B40A0C12`, edge `#33FFFFFF` (flat; a gradient only if Z asks) | A page's content column |
| `SideColumn` | `UiTheme` | fill `#D8060709`, edge `#40FFFFFF` | The member column, the category rail, the party sidebar |
| Party | `Menus/PartyColumnsView` | its whole area on `PageColumn`; a 1 px divider in the edge color between the active and reserve columns, from the titles down | |
| Inventory | `InventoryPage`, `InventoryColumnView` | rail 72 px on `SideColumn`; list on `PageColumn`; gap 96 px (was 24) showing the map; sidebar 520 px on `SideColumn` | `menu-inventory-description.webp` |
| Equipment | `Menus/EquipmentPage` | sheet on `PageColumn`, always the area minus 600 px; the 600 px choice column on `PageColumn` only while choosing | Decision 290 |
| Member column | `Menus/GameMenuScene` | its 80 px on `SideColumn` at full height | `menu-status.webp` |

**Tab folding** (TB1a, TB1b; decision 286)

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| `RailWidth` | `Menus/TabFold` | 120 px of each tab left on screen when folded (the point and the icon) | The reference's collapsed tabs, about 75 px at 1200 |
| Fold time | `Menus/TabFold` | 0.2 s each way, `Easing.CubicOut`; a change of direction starts from where the tabs are | As the tab slide-in and `PageTransition` |
| Label fade | `Menus/TabFold.LabelOpacity` | from 1 to 0 over the first half of the fold, back over the second half of the unfold | TB1b |
| Folded icon | `Menus/MenuTabsView` | centered between the screen's edge and the tab's shoulder; the selected icon's 14 px slide shrinks to 0 as it folds | TB1b |

**Aiming and preview** (IT1, IT2, IT3; decisions 267, 287, 288)

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Aimed cards | `Menus/PartySidebarView` | cards the item can go to in the selection look at full opacity (reserve or not); others at 0.4 (from `ItemTargetPickerView`); the pointer at the pointed card's left | |
| Keys while aiming | `Menus/ItemTargetPicker` | Up and Down; Left and Right do nothing (one column of cards) | Was: Left and Right moved too |
| Target hint | `ui.json` `menu_hint_inventory_target` | "Choose who gets the {item}." | Replaces `item_use_title` |
| After a use | `Menus/ItemUse` | copies left and someone to aim at: aim again, on the same member if still a candidate, else the first; otherwise back to the list | Decision 288 |
| HP preview | `Ui/MemberCardView` | the HP row's value column (140 px, unchanged, so the bar keeps its length) reads "now ◆ after": a 6 px diamond (Equipment's preview mark), the after value in `UiTheme.PreviewColorOf`'s color | On every aimed card (decision 287) |

**Notice dialog** (C2, PD1; decisions 268, 282, 283)

| Name | Where | Placeholder |
|---|---|---|
| Window | `Ui/NoticeDialogView` | the yes/no prompt's: 720 px wide (wider for a long message), titled, centered, the screen dimmed with `UiTheme.Scrim` |
| Title | `Menus/PartyPage` | "Party" (`MessageIds.MenuParty`; decision 283) |
| Message | `Ui/NoticeDialogView` | 24 px, centered, one line; 24 px above the button |
| OK button | `Ui/DialogButtonView` | 260 x 50, selected look with the pointer; "OK" (`ui.json` `notice_ok`) |

## Design overview

### What exists

From `docs/map.md` (Menus, Ui) and the code read for this plan:
- **The sidebar** is the menu's: `GameMenuScene` owns `PartySidebar` and `PartySidebarView` through `MenuFrame`, draws them when `IMenuPage.ShowsPartySidebar`, refreshes them with an `IMemberPage`'s shown member, and narrows the page by 544 px. Its cards are built active-then-reserve in `PartySidebar.RebuildAll`.
- **Item use** is `InventoryPage` → `ItemUseDialog` → `ItemTargetPicker` (input, tested) and `ItemTargetPickerView` (a full-width raised band). The picker lists `PartyState.Members` (recruitment order), not the sidebar's order.
- **Panels**: `Ui/PanelView` is a rounded 9-slice (the interaction prompt's pill), so it can't draw a square column; the sidebar draws the `SidebarPanel` gradient, the rail its own strip, the equipment choice a flat `Surface`. Nothing else has a panel.
- **Dialogs**: `ConfirmPrompt`/`ConfirmPromptView` draw a titled `FramedWindowLayout`/`FramedWindowView` with two buttons (`DrawButton`, private).
- **Selection**: `UiTextures.DrawSelectedRow` (`SelectedRow` between two `HighlightLine` hairlines); the selected tab (`SelectedNavTab` in `Selection` with `SelectionStroke`, `SelectionLine`), which share colors with the battle's `CommandHeader` and `TurnOrderView`.
- **Tabs**: `NavTabShape` outlines every pixel on the image's border, so column 0 is the line the developer saw. `MenuTabsView` places each tab at `TabLeft` (the slide-in); the page area starts 24 px after `NavTabShape.Width`, a constant in `GameMenuScene`.
- **Motion**: `MenuMotion`, `PageTransition` and `SelectionGlide` take the elapsed time and ease with `Easing.ShareOf` and `CubicOut`.
- **Sizes**: `InventoryPage` 281 lines, `PartyPage` 258, `GameMenuScene` 235, `UiTextures` 245, `MemberCardView` 153.

### Core change

One query, in IT3: `FieldItemUse.HpAfterUse(ItemDefinition item, PartyMember target)` returns the HP the target would have after the item, without using it. `Use` and `HpAfterUse` share one private helper that lands the effect (`EffectResolver.Apply` with the target's defense) on given vitals: the real ones for `Use`, a copy for `HpAfterUse`. How much a potion heals is a rule, so the Game must not compute it. No save, content or Data change.

### Structure and folder dependencies

Every new edge checked: `Ui` still uses only `Rendering`, `Input` (as `ConfirmPrompt` and `ListCursor` already do) and Core; `Menus` uses `Ui`.

| Folder | Uses | New or changed types |
|---|---|---|
| `Ui` | `Rendering`, `Input`, Core | `ColumnPanelStyle`, `ColumnPanelView`, `HpPreview`, `DialogButtonView`, `NoticeDialog`, `NoticeDialogView`; `UiTheme.PreviewColorOf` |
| `Menus` | `Ui`, `Rendering`, `Saving`, `Scenes`, `Input`, Core | `MenuLayout`, `TabFold`, `InventoryColumnView`, `ItemUse` (was `ItemUseDialog`); `MenuFrame`, `ItemTargetPickerView` go |
| `Flow` | `Menus` | `SceneFactory` builds the menu with its header only |
| Core `Party` | `Effects`, `Stats` | `FieldItemUse.HpAfterUse` |

### New types

| Type | Folder | Responsibility (one line) | Slice |
|---|---|---|---|
| `InventoryColumnView` | Menus | Draws the Inventory's list column: the category rail, the category's name, the column titles and the list. | C1 |
| `MenuLayout` | Menus | The game menu's regions on the virtual screen: the page area beside the tabs, a single column's width, a column's full height (static). | ML1 |
| `ColumnPanelStyle` | Ui | A column panel's look: its flat fill and its edge hairline (record). | PC1 |
| `ColumnPanelView` | Ui | Draws a column panel: its fill and its left edge; the page closes its last column's right edge. | PC1 |
| `TabFold` | Menus | How far the tabs have folded into their icon rail, easing toward folded or unfolded from wherever they are. | TB1a |
| `ItemUse` | Menus | Using a consumable from the inventory: who it can go to, Core's use, the result line (renamed from `ItemUseDialog`, its view gone). | IT1 |
| `HpPreview` | Ui | A card's HP after an item, measured once: the text and its color (record). | IT3 |
| `DialogButtonView` | Ui | Draws one dialog button: selected in the shared look with the pointer, or faintly outlined; its label centered. | C2 |
| `NoticeDialog` | Ui | A notice the player closes with OK: its title and message, and which presses close it (record). | PD1 |
| `NoticeDialogView` | Ui | Draws a `NoticeDialog`: the screen dimmed, a titled framed window, the message, the OK button. | PD1 |

### Key decisions (implementation, no ADR)

- **Tab-only colors** (LK1): the selected tab gets `SelectedTab`, `SelectedTabStroke` and `SelectedTabLine`; `Selection` and `SelectionStroke` stay as they are for the battle and Party's moving outline, so nothing has to be pinned. The category rail's selected tab becomes `DrawSelectedRow` (it is a selected entry), dropping its own gradient and `SelectionStroke` outline.
- **The glow is a fixed-width piece** at the row's left, not a gradient stop: a 260 px button and a 1100 px row get the same glow.
- **The tabs' left edge goes off-screen** (`MenuTabsView.OffScreenEdge`), not out of the shape: no bake or shape test changes.
- **`MenuLayout.PageArea(tabsRight)`** (ML1) is the one place the page area's left edge comes from; PC1 and PC2 lay out from the area they're given, so TB1a only passes a moving `tabsRight`.
- **The sidebar moves into `InventoryPage`** (SB1): the only page that shows it is the only one that aims at it. `IMenuPage.ShowsPartySidebar`, `MenuFrame` and the menu's sidebar code go. The page refreshes it in `Open` and each `Update`.
- **Pages draw their own columns**, because widths differ; `MenuLayout.FullHeight` gives them the same top and bottom. A column draws its fill and left edge; adjacent columns share that edge as their divider, and the page closes the last one (`ColumnPanelView.DrawRightEdge`).
- **One member order** (IT1): `PartySidebar.RebuildAll` and `ItemUse`'s picker both use `MemberOrder.ActiveThenReserve`, so they can't disagree.
- **`PartySidebar.ShownIndex` becomes `PointedIndex`** (SB1): with 266 no page shows a member on the sidebar; IT1 points it at the item's target.
- **`ItemUse` exposes the picker and an HP-after lookup** (IT3), built when aiming starts and after each use; `PartySidebarView` formats the previews when the picker changes (by reference), as it formats cards when they change.
- **`TabFold`** keeps a raw progress moving toward its target and eases it; a change of direction re-derives the progress from the current eased share, so the tabs never jump.
- **The notice dialog holds no state**: `NoticeDialog.IsClosedBy(input)` is a query; the page drops its dialog when it returns true.

### Extension points

- `NoticeDialog`: Equipment's refusal and the save failure line (backlog), a skill-learned notice.
- `ColumnPanelView`: the battle's later pass, a shop.
- `MenuLayout.SingleColumn`: any later single-column page.

### Replacement mapping

| New | Replaces |
|---|---|
| `SelectedTab`, `SelectedTabStroke`, `SelectedTabLine` | `Selection`, `SelectionStroke`, `SelectionLine` in `UiTextures.SelectedNavTab` and `SelectionLine` |
| `SelectionGlow`, `SelectionHairline` | `HighlightLine` inside `DrawSelectedRow` |
| `DrawSelectedRow` on the rail | `CategoryRailView`'s `SelectedRow` gradient and `SelectionStroke` outline |
| `MenuTabsView.OffScreenEdge` | the visible outlined first column of each tab |
| `InventoryColumnView` | `InventoryPage.DrawList`, its fonts, `ColumnTitles`, category names and layout constants |
| `MenuLayout` | `GameMenuScene`'s `FooterHeight`, `BodyTop`, `PageLeft`, `_pageArea` |
| `InventoryPage`'s sidebar | `GameMenuScene`'s sidebar fields, constants, `ShowsSidebar` and drawing; `MenuFrame`; `IMenuPage.ShowsPartySidebar` |
| `ColumnPanelView` (`SideColumn`) | `UiTheme.SidebarPanel`; `UiTheme.CategoryRail` and the rail's two strokes |
| `ColumnPanelView` (`PageColumn`) | `EquipmentChoiceView`'s `UiTheme.Surface` panel; the bare map behind every page |
| `TabFold` | the tabs staying full while a page is open |
| `PartySidebarView`'s aimed cards, the target hint | `ItemTargetPickerView` (and its tests), `UiTheme.RaisedSurface`, `item_use_title` |
| `ItemUse` | `ItemUseDialog` |
| `PartySidebar.PointedIndex`, `Refresh(pointed)` | `ShownIndex`, `Refresh(shown)` |
| `UiTheme.PreviewColorOf` | `StatPreviewText.ColorOf` |
| `DialogButtonView` | `ConfirmPromptView.DrawButton` and `PromptButton` |
| `NoticeDialog`, `NoticeDialogView` | `PartyColumns.Notice`, `PartyColumnsView`'s red notice line |

Backlog lines this plan closes: "Item target picker layout" (IT1) and "Equipment page: opening a slot's choice hides the party sidebar" (PC2). It adds one: the two red notices decision 284 keeps.

## Risks

- **The retune reaches every screen.** Title, leader choice, defeat menu, yes/no buttons, command rows and the inventory rail change in LK1; a navy band on a dark column can lose contrast. LK1's hand check has a pass/fail on the battle's command rows and the title, with the fallback in the tunables; Z checks again.
- **A heavy menu.** Dark columns over most of the page hide the map; the single-column cap (decision 285) and Inventory's gap leave it showing. The side column's darkness is on Z's checklist.
- **Two motions at once.** Entering a page, the page slides in from the right (`PageTransition`) while its left edge moves left with the folding tabs; TB1b checks they read as one movement and shortens the page's slide if they fight.
- **The 300-line check.** `InventoryPage` after C1 about 236, then SB1 (+20), IT1 (+15), IT2 (+5), IT3 (+2): about 278. If a slice would take it past 285, the hint and sidebar wiring move into `ItemUse` first. `PartyPage` 258 → about 268 in PD1. New tests go in new classes (`InventoryPageSidebarTests`, `InventoryPageAimTests`, `PartyPageNoticeTests`, `GameMenuSceneTabsTests`, `FieldItemUseHpAfterTests`).
- **Removed tests.** SB1 removes five `GameMenuSceneTests` and one `EquipmentPageTests` test about the sidebar on other pages; IT1 removes `ItemTargetPickerViewTests` and replaces the two Left/Right picker tests; PD1 replaces one assertion and one test whose behavior decision 268 changes. Each slice lists them with the reason; none is weakened to pass.
- **Determinism and saves.** One pure Core query; no randomness, no save change.
