# P1a. One pointer and one selection look on every list

Part of [the UI second pass plan](../README.md). Decisions 252, 253. Design: [design.md](../design.md), "Key decisions" (one selection look), "Replacement mapping". After SZ2. Look at `docs/ui-reference/menu-tabs.webp`, `menu-inventory-description.webp` and `battle-command-menu-1.webp` (one glow band, one pointer, wherever the list is) and S0's captures of the save list, the defeat menu and the command panel first.

- **P1a. One pointer and one selection look on every list** (Game, about 170 lines)
  - **Goal:** every list's selected row is drawn the same way: `UiTextures.DrawSelectedRow` (the blue glow band between hairlines) with `Ui/MenuPointer`'s diamond at its left. The lists that draw their own state today move onto it.
  - **Changes (Game):**
    - `Battle/DefeatScreenView`: the selected choice is `DrawSelectedRow` over its bar instead of `SelectedRewardBar`, and its hand-drawn diamond becomes `MenuPointer` (backlog). If `UiTextures.SelectedRewardBar` has no user left, it goes.
    - `Battle/CommandPanelView`: the selected row's own lit fill and hand-drawn diamond become `DrawSelectedRow` and `MenuPointer`. The target pointer above a battler stays (decision 56). It is at 282 lines; this should shrink it.
    - `Saving/SaveSlotListView`: `UiTheme.Highlight` becomes `DrawSelectedRow` plus `MenuPointer`; `UiTheme.Highlight` goes if unused.
    - Lists already on `DrawSelectedRow` without a pointer get `MenuPointer` where they have the cursor (check each: `OptionsPage`, `EquipmentSheetView`, `EquipmentChoiceView`, `PartyActionChoiceView`, `MemberCardView`); a list whose cursor is elsewhere (the dimmed side of a page) keeps its dimmed band and no pointer.
    - `UiTextures.DrawSelectedRow`'s summary says it is the one selected-row look (decision 252).
    - Remove the "row pointer diamonds drawn by hand" line from `docs/plans/backlog.md`.
  - The yes/no buttons move in W1b, the title in T3.
  - **Tests:** none new (draw-only; the selection's state is already tested per list). Existing tests pass unchanged.
  - **Verify by hand:** F12 the save list, the defeat menu, the command panel, Options, Equipment, the party page's action choice: every selected row has the same band and the same diamond at its left, and only the list with the cursor shows the pointer.
- **Done, with deviations:**
  - **Moved onto the shared look:**
    - the defeat menu: the band over its pointed bar, the pointer before the label;
    - the command panel: the plain fill under the band, the pointer before the icon, inside the frame;
    - the save list.
  - **The pointer added** to Options, the equipment choice and the party action choice.
  - **Not in the slice's list:**
    - the System page drew no band at all; it now has the band and pointer, dimmed while its yes/no is open;
    - the Equipment sheet and the Party page's cards show the pointer only while they have the cursor (a `HasCursor` on their `Place`), and the sheet dims its row while the choice panel is open, like the Inventory page.
  - **Removed:** `UiTextures.SelectedRewardBar`, `UiTheme.Highlight`, the hand-drawn diamonds' constants and the backlog line.
  - **Left as is:** the item target picker's chosen cards (several can be chosen at once).
  - **Review:** the sidebar drew a second pointer on the Equipment page. With the developer's answer, `IMemberPage.HasOwnPointer` (Equipment true, Status false) tells the sidebar to light the shown card without pointing (`PartySidebarView.Place`). The Party page's selected card keeps its full band while the action choice is open.
