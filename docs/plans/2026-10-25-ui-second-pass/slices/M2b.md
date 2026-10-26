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
