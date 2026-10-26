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
