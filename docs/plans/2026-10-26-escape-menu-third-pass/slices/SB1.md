# SB1. The party sidebar only on Inventory

Part of [the escape menu third pass plan](../README.md). Decisions 266 (answers 265), 285. Design: [design.md](../design.md), "Key decisions" (the sidebar moves into `InventoryPage`; `PointedIndex`) and "Replacement mapping". After ML1, whose single-column cap now takes effect on Status, Options and System.

- **SB1. The party sidebar only on Inventory: the page owns it, the menu forgets it** (Game, about 220 lines, about a third of it removed code and tests)
  - **Goal:** the sidebar shows on Inventory only. Equipment uses the width it took; Status, Options and System widen up to `MenuLayout.SingleColumnWidth`, the map showing on the right. The member column alone marks the shown member on member pages.
  - **New types:** none.
  - **Changes (Game):**
    - `Menus/IMenuPage`: `ShowsPartySidebar` goes; so do its six implementations (Inventory, Party, Equipment, Status, Options, System) and `CountingMenuPage`'s.
    - `Menus/IMemberPage`: its summary says the member column marks the shown member (not the sidebar); `HasOwnPointer`'s says it decides the column's pointer.
    - `Menus/GameMenuScene`: the sidebar's fields, `SidebarWidth`, `SidebarGap`, `_pageAreaBesideSidebar`, `_sidebarColumn`, `ShowsSidebar` and its drawing go; `RefreshMemberViews` refreshes the member column only. The constructor becomes `GameMenuScene(ScreenText text, UiTextures ui, MenuHeader header, ImmutableArray<IMenuPage> pages)`.
    - `Menus/MenuFrame`: deleted. `Flow/SceneFactory`: `CreateMenuFrame` goes; the menu gets `new MenuHeader(_text, session)`.
    - `Menus/InventoryPage`: builds and owns its `PartySidebar` and `PartySidebarView` (from the session's party and a `MemberCardView` of its text, textures and catalog: the constructor keeps its four parameters). It refreshes the sidebar in `Open` (the party may have changed on another page) and at the end of each `Update`, with nothing pointed yet; it draws its list column in its area minus the sidebar and the 24 px gap (the constants move here from `GameMenuScene`), and the sidebar at its right. `internal PartySidebar Sidebar` for tests.
    - `Menus/PartySidebar`: `ShownIndex` → `PointedIndex`, `Refresh(PartyMember? shown)` → `Refresh(PartyMember? pointed)`; its summary: it marks the card an item's pointer is on (IT1) and keeps it in view. `PartySidebarView`'s `PointsAtShown` stays until IT1 replaces it.
    - `Menus/EquipmentPage`: the sheet narrows only by the choice panel (PC2 fixes its width).
  - **Tests:**
    - Removed, as the behavior they pin is gone (decision 266): `GameMenuSceneTests.ShowsSidebar_PageThatWantsIt_IsTrue`, `ShowsSidebar_PageThatShowsThePartyItself_IsFalse`, `ShowsSidebar_MainMenu_IsFalse`, `Update_RightOnStatus_SidebarMarksTheNextMember`, `Update_InventoryOpen_SidebarMarksNoMember`; `EquipmentPageTests.ShowsPartySidebar_WhileChoiceOpen_IsFalse`. `GameMenuSceneTests`' helpers build a header instead of a frame (`Update_RightOnStatus_MemberColumnShowsTheNextMember` stays).
    - `PartySidebarTests`: the four `Refresh_ShownMember_MarksItsCard`, `Refresh_NoShownMember_MarksNone`, `Refresh_ShownMemberBelowTheWindow_ScrollsToShowIt`, `Refresh_ShownMemberAlreadyVisible_KeepsTheWindow` renamed to `Pointed` and read `PointedIndex`; assertions unchanged.
    - New class `InventoryPageSidebarTests`:
      - `Update_AfterUsingAPotionOnTheLeader_SidebarCardShowsTheNewHp` (the refresh moved from the menu into the page; the leader starts hurt so the card's HP changes);
      - `Open_AfterPartyReordered_SidebarListsTheNewOrder` (reorder the party between two openings; the cards follow without an update).
  - **Verify by hand:** Inventory shows the sidebar as before; Equipment shows none and uses the width; Status, Options and System are about 1100 px wide with the map on their right, Status's labels still near their values; Status and Equipment keep the member column; System's save list fits its column (F12 each, compare with `docs/captures/2026-10-26-menu/`).
