# M2a. The member column: which members, which one is shown

Part of [the UI second pass plan](../README.md). Decision 255. Look at `docs/ui-reference/menu-status.webp`, `menu-equipment.webp` and `menu-field-skills.webp` (the narrow column of small portraits between the tabs and the page) first.

Question 265's placeholder: the party sidebar stays on the right.

- **M2a. The member column: which members a page goes through, which one is shown** (Game, about 130 lines)
  - **Goal:** a pure model of the column: on a member page, the members it goes through (the active party in order, then the reserve, as `MemberOrder.ActiveThenReserve`) and which one the page shows, with a scroll so the shown one stays in view.
  - **New types:** Game `Menus/MemberColumn` (internal sealed): the members a member page goes through, in order, and which one is shown.
    - `void Refresh(IReadOnlyList<PartyMember> members, PartyMember shown)` (rebuilds only when the order changed), `IReadOnlyList<PartyMember> Members`, `int ShownIndex`, `int FirstVisible`; `public const int VisibleCount` (placeholder 8).
  - **Changes (Game):** `Menus/IMemberPage` gains `IReadOnlyList<PartyMember> Members` if not every member page has it (`StatusPage` does; check `EquipmentPage`); `GameMenuScene` refreshes the column each update a member page is open, as it does the sidebar. Nothing is drawn yet: M2b draws it (the column has its caller in the menu's update now, and its view next).
  - **Tests** (`MemberColumnTests`):
    - `Refresh_StatusPage_ListsActiveThenReserve`
    - `Refresh_ShownMember_IsMarked`
    - `Refresh_ShownBelowTheView_ScrollsToIt`
    - `Refresh_SameOrder_KeepsTheScroll`
  - **Verify by hand:** nothing visible yet.
