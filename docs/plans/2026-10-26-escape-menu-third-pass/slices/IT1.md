# IT1. Using an item aims at the sidebar's cards

Part of [the escape menu third pass plan](../README.md). Decision 267. Design: [design.md](../design.md), "Tunables and placeholders" (aiming and preview), "Key decisions" (one member order, `PointedIndex`). After TB1b. Look at `docs/ui-reference/menu-inventory-target.webp` first, and `docs/captures/2026-10-26-menu/menu-inventory-use.jpg` (the band this slice removes).

- **IT1. Using an item aims at the sidebar's cards; the "Use item" band goes** (Game, about 220 lines, with the removed view and its tests)
  - **Goal:** Confirm on a usable item moves the pointer onto the sidebar: the cards the item can go to are lit at full opacity (reserve members too), the others faded; Up and Down move between the cards (scrolling the sidebar), Confirm uses the item, Cancel goes back to the list. The list keeps its row selected, dimmed. The footer says who to choose for which item. After a use the cursor returns to the list (IT2 keeps it on the sidebar while copies remain).
  - **New types:** none (`ItemUse` is a rename).
  - **Changes (Game):**
    - `Menus/ItemUseDialog` → `Menus/ItemUse` (file renamed): using a consumable from the inventory: who it can go to, Core's use, the result line. It no longer draws: `ItemTargetPickerView` and its `Show`/`Draw` calls go. It exposes `ItemTargetPicker? Picker` (null while not aiming). Its picker lists `MemberOrder.ActiveThenReserve(party)`.
    - `Menus/PartySidebar.RebuildAll`: builds its cards from `MemberOrder.ActiveThenReserve` (the reserve flag from the active count), so the sidebar and the picker read one order from one place.
    - `Menus/ItemTargetPicker`: Up and Down only (the cards are one column; Left and Right do nothing, so Left never reaches the rail); new `PartyMember? Pointed` (the member the pointer is on; null when the item takes everyone).
    - `Menus/PartySidebarView`: `Place(Rectangle Column, ItemUse Use)` replaces `PointsAtShown`. While `Use.Picker` is set: the cards it selects in the selection look (`MemberCardView.Place.IsSelected`), candidates at full opacity whether or not they are in the reserve, non-candidates at 0.4 (`UnavailableOpacity`, moved from the deleted view), the pointer at the pointed card's left. Otherwise: plain cards (the reserve dimmed as today), no pointer.
    - `Menus/InventoryPage`: refreshes the sidebar with `Picker?.Pointed`; passes `ListHasCursor: false` to the column view while aiming; `Hint` is the target hint while aiming, formatted once when aiming starts; `IsChoosingTarget` stays.
    - Deleted: `Menus/ItemTargetPickerView`, `UiTheme.RaisedSurface`, `MessageIds.ItemUseTitle` and `ui.json` `item_use_title`. New: `MessageIds.MenuHintInventoryTarget`, `ui.json` `menu_hint_inventory_target` "Choose who gets the {item}.".
    - `docs/plans/backlog.md`: the "Item target picker layout" line goes (question 112: no more band).
  - **Tests:**
    - Removed: `ItemTargetPickerViewTests` (the view is gone).
    - `ItemTargetPickerTests`: `Update_Right_MovesLikeDown` and `Update_Left_MovesLikeUp` are replaced by `Update_Right_KeepsTheTarget` and `Update_Left_KeepsTheTarget` (the cards are one column now); new `Pointed_TakesAllChoice_IsNull`, `Pointed_AfterDown_IsTheNextCandidate`.
    - New class `InventoryPageAimTests`:
      - `Update_ConfirmPotion_PointsTheSidebarAtTheFirstCandidate`;
      - `Update_DownWhileAiming_PointsTheSidebarAtTheNextCandidate`;
      - `Update_DownToTheLastReserveMember_ScrollsTheSidebarToIt` (the start has more recruits than cards in view);
      - `Hint_WhileAiming_NamesTheItem`;
      - `Hint_AfterCancel_IsTheListHint`.
    - `PartySidebarTests` pass unchanged (the order is the same); `InventoryPageTests.Update_LeftWhileChoosingTarget_LeavesTheRailAlone` and the cancel and use tests pass unchanged.
  - **Verify by hand:** a potion with a hurt member: the pointer jumps to the sidebar, candidates lit (a reserve candidate as bright as the active ones), a fallen member faded; Up/Down move and scroll; Confirm heals and the cursor is back on the list with the result under the row; Cancel backs out with nothing used; a revive aims only at the fallen; an all-party item lights every candidate with no pointer; Esc while aiming backs out, it doesn't leave the page. F12 and compare with `menu-inventory-target.webp`.
