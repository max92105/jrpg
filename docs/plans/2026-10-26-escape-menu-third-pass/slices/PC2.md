# PC2. Two-column pages: Inventory and Equipment

Part of [the escape menu third pass plan](../README.md). Decisions 269, 290. Design: [design.md](../design.md), "Tunables and placeholders" (layout and columns: Inventory, Equipment). After PC1. Look at `docs/ui-reference/menu-inventory-description.webp` (rail, list column, map gap, sidebar panel) and `menu-equipment.webp` (two columns, a divider) first, and `docs/captures/2026-10-26-menu/menu-inventory.jpg`, `menu-equipment.jpg`, `menu-equipment-choice.jpg`.

- **PC2. Two-column pages: Inventory's rail, list and sidebar; Equipment's sheet and choice** (Game, about 160 lines)
  - **Goal:** Inventory reads as the reference's: a darker rail strip, the list on a page column, the map showing in a gap, the sidebar on its own darker column, all header to footer. Equipment's sheet has a fixed width; its choice is a second page column beside it, drawn only while choosing, so nothing narrows (decision 290).
  - **New types:** none.
  - **Changes (Game):**
    - `Menus/CategoryRailView`: its strip and two strokes become a `SideColumn` panel at full height (`MenuLayout.FullHeight`); the tabs stay where they are.
    - `Menus/InventoryColumnView`: the list on `PageColumn` from the rail's right edge (its left edge is the divider) to the column's right, full height, closed on the right.
    - `Menus/InventoryPage`: the gap before the sidebar grows from 24 to 96 px.
    - `Menus/PartySidebarView`: its panel is `SideColumn` at full height, closed on the right, instead of the `SidebarPanel` gradient.
    - `Menus/EquipmentPage`: the sheet's column is always the area minus `EquipmentChoiceView.PanelWidth`, on `PageColumn`; while choosing, the choice column on `PageColumn` beside it (its left edge the divider) closes the page; otherwise the sheet closes it.
    - `Menus/EquipmentChoiceView`: stops drawing its own `UiTheme.Surface` panel.
    - `Ui/UiTheme`: `SidebarPanel` and `CategoryRail` go.
    - `docs/plans/backlog.md`: the "Equipment page: opening a slot's choice hides the party sidebar" line goes.
  - **Tests:** none new (draw-only). Inventory and Equipment tests pass unchanged.
  - **Verify by hand:** F12 on Inventory (list, rail on, an empty category) and Equipment (browsing, choosing): columns from header to footer with side lines; the rail and the sidebar darker than the list; the map in the gap; Equipment's sheet keeps its width when the choice opens and closes. Z judges the side column's darkness.
