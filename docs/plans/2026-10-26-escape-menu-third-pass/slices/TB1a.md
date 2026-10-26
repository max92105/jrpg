# TB1a. The tabs fold into an icon rail inside a page: the motion

Part of [the escape menu third pass plan](../README.md). Decision 286. Design: [design.md](../design.md), "Tunables and placeholders" (tab folding), "Key decisions" (`MenuLayout.PageArea(tabsRight)`, `TabFold`). After PC2: every page and column lays out from the area it is given, so only the area's left edge moves here. Look at `docs/ui-reference/menu-inventory-description.webp`, `menu-equipment.webp` and `menu-status.webp` (the folded tabs at the left) and `menu-tabs.webp` (unfolded) first.

The developer asked for one slice, TB1; it is split up front: this slice is the motion with a plain draw, TB1b the rail's look.

- **TB1a. The tabs fold into an icon rail inside a page: the motion, and the page following it** (Game, about 230 lines)
  - **Goal:** entering a page, the tabs slide left over 0.2 s until only a 120 px rail of each shows, their labels fading out; the page's area (its columns and content) moves left with them. Leaving the page, they slide back. A change of direction mid-way starts from where the tabs are.
  - **New types:** Game `Menus/TabFold` (internal sealed): how far the tabs have folded into their icon rail, easing toward folded or unfolded from wherever they are.
    - `Fold()`, `Unfold()`: set the target; calling the one already under way changes nothing.
    - `Advance(TimeSpan delta)`: moves the raw progress toward the target at 1 per `Duration` (0.2 s).
    - `float Share`: 0 unfolded, 1 folded, eased with `Easing.CubicOut` in the direction of travel; a change of direction re-derives the raw progress from the current share, so `Share` never jumps.
    - `int HiddenWidth`: how much of each tab is pushed off-screen, `Share` × (`NavTabShape.Width` − `RailWidth`); `RailWidth` = 120 (placeholder).
  - **Changes (Game):**
    - `Menus/GameMenuScene`: owns a `TabFold`; Confirm into a page folds, leaving a page unfolds; advanced each update with the menu's other motions. The page area is `MenuLayout.PageArea(NavTabShape.Width − HiddenWidth)`, worked out per frame (a struct, no allocation); the member column follows it. `internal Rectangle PageArea` for tests.
    - `Menus/MenuTabsView`: `Place` gains the `TabFold`; each tab, its glow and its line draw `HiddenWidth` further left; labels at opacity × (1 − `Share`); each icon moves linearly from its place to the rail's middle as the share rises, so it never leaves the screen (TB1b refines this).
  - **Tests:**
    - New class `TabFoldTests`:
      - `Share_New_IsUnfolded`;
      - `Share_HalfwayThroughFolding_IsEasedPastHalf` (0.875);
      - `Share_AfterFolding_IsFolded`;
      - `Share_HalfwayThroughUnfolding_IsEasedBackPastHalf` (0.125);
      - `Share_AfterUnfolding_IsUnfolded`;
      - `Unfold_MidFold_KeepsTheShareItHad` (no jump at the turn, then it falls);
      - `HiddenWidth_Folded_LeavesTheRailOnScreen`.
    - New class `GameMenuSceneTabsTests` (`GameMenuSceneTests` stays under 300):
      - `Update_HalfwayIntoAPage_PageAreaLeftIsBetweenTabsAndRail`;
      - `Update_LongAfterEnteringAPage_PageAreaStartsBesideTheRail`;
      - `Update_LongAfterLeavingAPage_PageAreaStartsBesideTheFullTabs`.
    - `GameMenuSceneTests` and every page's tests pass unchanged.
  - **Verify by hand:** enter and leave each page: the tabs fold and unfold smoothly, never snapping; the labels fade; the icons stay on screen; the page's columns and content move with the tabs, nothing overlaps the rail; pressing Cancel mid-fold reverses from where it is; Inventory's list, Equipment's sheet and the single columns use the freed width. F12 folded, compare with the reference shots above.
