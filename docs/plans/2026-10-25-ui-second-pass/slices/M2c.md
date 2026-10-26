# M2c. The party sidebar's unused shown-member marking removed

Part of [the UI second pass plan](../README.md). Decision 266. Split from M2b on 2026-10-26. After M2b.

- **M2c. The party sidebar's unused shown-member marking removed** (Game, about 100 lines removed)
  - **Goal:** since M2b no page that shows the sidebar shows one member (decision 266), so the sidebar's marking of the shown member is dead: remove it and what only it used.
    - `IMemberPage.HasOwnPointer` and its two implementations; `PartySidebarView.Place.PointsAtShown` and the card's pointer and band for the shown member; `PartySidebar.ShownIndex`, `Refresh`'s shown member and its scroll to it (the sidebar then keeps its scroll at the top, or scrolls by its own rule if one is needed: check `PartySidebarView`'s scroll marks).
    - The sidebar's summary and decision 146's mention in this plan's README ("146: the party sidebar marks the member a page shows") updated.
  - **Tests:** remove the tests on the removed behavior (`PartySidebarTests` on `ShownIndex`, `GameMenuSceneTests` on the sidebar's shown index); record which and the count before and after.
  - **Verify by hand:** the Items, Options and System pages show the sidebar as before; Status and Equipment show the member column and no sidebar.
  - From M2b's review: the scene picks the column layout from `IMemberPage` but the sidebar from `ShowsPartySidebar`; once the marking is gone, derive the sidebar from the page kind (`IMemberPage` never shows it) or drop the member pages' `ShowsPartySidebar`, so the two can't disagree.
