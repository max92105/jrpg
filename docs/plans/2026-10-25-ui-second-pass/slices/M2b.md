# M2b. The member column drawn

Part of [the UI second pass plan](../README.md). Decisions 252, 255. Design: [design.md](../design.md), "Tunables and placeholders" (member column). After M2a. Look at `docs/ui-reference/menu-status.webp` and `menu-equipment.webp` first.

Decision 266 (answers 265): on member pages the column replaces the party sidebar. After M2b0, which moves the tab drawing out of `GameMenuScene`; M2c then removes the sidebar's shown-member marking.

- **M2b. The member column drawn between the tabs and member pages** (Game, about 210 lines with the 1.6x for a new view)
  - **Goal:** on Status and Equipment, a narrow column of small diamond portraits sits between the tabs and the page; the shown member's portrait is in the selection look (a short band behind it and the diamond pointer at its left), and Left/Right move it as they change the member.
  - **New types:** Game `Menus/MemberColumnView` (internal sealed): draws the member column: small diamond portraits (`UiTextures.DiamondPortrait`, 56 px), the shown one in the selection look, scroll marks when members are out of view.
  - **Changes (Game):** `Menus/GameMenuScene`: a member page's area starts after the column (80 px) and takes the sidebar's width (decision 266: `StatusPage` and `EquipmentPage` no longer show the sidebar); the column is drawn with the page's opacity and transition.
    - BG1's warm-up includes the column's portrait size.
  - **Tests:** none new for the column (draw-only; M2a tests the model). The tests that a member page shows the sidebar change to say it doesn't; record which.
  - **Verify by hand:** Status and Equipment show the column and no sidebar; Left/Right move the marked portrait and the page together; with more members than fit, the column scrolls; other pages show no column; the page's content still fits after SZ1 (F12, compare with `menu-status.webp`).
