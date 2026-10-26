# M2b. The member column drawn

Part of [the UI second pass plan](../README.md). Decisions 252, 255. Design: [design.md](../design.md), "Tunables and placeholders" (member column). After M2a. Look at `docs/ui-reference/menu-status.webp` and `menu-equipment.webp` first.

Question 265's placeholder: the party sidebar stays on the right; the page narrows by the column.

- **M2b. The member column drawn between the tabs and member pages** (Game, about 210 lines with the 1.6x for a new view)
  - **Goal:** on Status and Equipment, a narrow column of small diamond portraits sits between the tabs and the page; the shown member's portrait is in the selection look (a short band behind it and the diamond pointer at its left), and Left/Right move it as they change the member.
  - **New types:** Game `Menus/MemberColumnView` (internal sealed): draws the member column: small diamond portraits (`UiTextures.DiamondPortrait`, 56 px), the shown one in the selection look, scroll marks when members are out of view.
  - **Changes (Game):** `Menus/GameMenuScene`: a member page's area starts after the column (80 px); the column is drawn with the page's opacity and transition. Check `GameMenuScene`'s size first: if it would pass about 260, move the tab drawing to a `Menus/MenuTabsView` before this slice's changes (M1 also draws there).
    - BG1's warm-up includes the column's portrait size.
  - **Tests:** none new (draw-only; M2a tests the model). `GameMenuSceneTests` pass unchanged.
  - **Verify by hand:** Status and Equipment show the column; Left/Right move the marked portrait and the page together; with more members than fit, the column scrolls; other pages show no column; the page's content still fits after SZ1 (F12, compare with `menu-status.webp`).
- **Done, with deviations:**
  - **`Menus/MenuTabsView`** takes the tab drawing out of `GameMenuScene` first, unchanged, since the scene would have passed 260 lines (now 236). M1 draws its icons there.
  - **Column:** `MemberColumnView` draws the members in view as 56 px diamond portraits, 72 px apart. The shown one sits on a 64 px band in the shared selection look (`DrawSelectedRow` and the diamond pointer, which doesn't bob). Small diamonds above and below mark members out of view. It is drawn at the member page area's left (80 px), with the page's slide and fade, and the page narrows by it. The sidebar stays on the right (question 265's placeholder).
  - **Review:** on a page with its own pointer (Equipment), the column shows the band without a pointer (`Place.PointsAtShown`, as the sidebar does), so only one pointer shows (decision 252). The scroll marks match the sidebar's 10 px cues.
  - **No warm-up added:** BG1 found a diamond portrait bakes in under 20 ms, so it warmed none. The first Status opening bakes up to 8 small portraits once. Worth watching in the hand check.
  - **Tests:** none new; 2170 unchanged.
