# ML1. `MenuLayout`: the page area beside the tabs; single-column pages capped

Part of [the escape menu third pass plan](../README.md). Decisions 285, 286. Design: [design.md](../design.md), "Tunables and placeholders" (layout and columns), "Key decisions" (`MenuLayout.PageArea(tabsRight)`). After C1, before SB1: the cap must be in place before the sidebar's width goes to the pages. Look at `docs/ui-reference/menu-status.webp` (a column about 1100 px wide at 1920, the art beside it).

- **ML1. `MenuLayout`: the page area beside the tabs' right edge; single-column pages capped at about 1100 px** (Game, about 140 lines)
  - **Goal:** the menu's regions have one home, with the page area's left edge taken from the tabs' right edge (so TB1a can move it) and a single-column width for the pages that show one column. Nothing changes on screen yet: beside the sidebar, those pages are narrower than the cap.
  - **New types:** Game `Menus/MenuLayout` (internal static): the game menu's regions on the virtual screen.
    - `FooterHeight`, `BodyTop` (moved from `GameMenuScene`).
    - `Rectangle PageArea(int tabsRight)`: from `tabsRight` plus the 24 px gap to 40 px from the screen's right; from `BodyTop` to 32 px over the footer.
    - `SingleColumnWidth` = 1100 (decision 285, placeholder) and `Rectangle SingleColumn(Rectangle area)`: the same left, top and height, at most that wide.
  - **Changes (Game):**
    - `Menus/GameMenuScene`: `FooterHeight`, `BodyTop`, `PageLeft` and `_pageArea` go; it asks `MenuLayout.PageArea(NavTabShape.Width)` (TB1a will pass the folding tabs' edge). The sidebar's narrowing stays here until SB1.
    - `Menus/StatusPage`, `OptionsPage`, `SystemPage` (and with it the save and load files it shows): `Draw` narrows its area with `MenuLayout.SingleColumn` first.
  - **Tests:** new class `MenuLayoutTests`:
    - `PageArea_NarrowerTabs_StartsFurtherLeft`;
    - `PageArea_NarrowerTabs_KeepsItsRightEdge`;
    - `SingleColumn_WiderArea_IsCappedAtTheColumnWidth`;
    - `SingleColumn_NarrowerArea_IsUnchanged`.
    - `GameMenuSceneTests` and every page's tests pass unchanged.
  - **Verify by hand:** F12 on every page: identical to before (the cap isn't reached beside the sidebar).
- **Done, with deviations:** as planned. `MenuLayout` holds `FooterHeight`, `BodyTop`, `SingleColumnWidth` (1100), `PageArea(tabsRight)` and `SingleColumn(area)`; the gap, right margin and footer gap are private constants. `GameMenuScene` keeps one static `_pageArea` from `MenuLayout.PageArea(NavTabShape.Width)` until TB1a makes it move. Status, Options and System (with its save and load files) draw in `MenuLayout.SingleColumn(area)`. Nothing moves on screen yet: beside the sidebar the page area is 902 px wide, under the cap. Tests: 5 in `MenuLayoutTests`, the fifth (`PageArea_BesideTheTabs_IsBetweenTheMargins`, from the review) pinning today's page area so a changed margin fails; 2199 in the solution.
