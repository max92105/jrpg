# MO2b. Page lists glide and bob

Part of [the UI second pass plan](../README.md). Decision 262. Design: [design.md](../design.md), "Tunables and placeholders" (`SelectionGlide`, pointer bob). After MO2a and C5.

- **MO2b. Page lists glide and bob: items, equipment, options, save files** (Game, about 200 lines)
  - **Goal:** the menu pages' lists move like the tabs and the title: the selected band glides between rows, and the pointer bobs, through `IMenuPage.Advance` (TS). Motion is the same everywhere.
  - **Changes (Game):** for each list, the page owns a `SelectionGlide` that follows its cursor (`Select`, `JumpTo` on a wrap or a scroll that moves the whole view) and a clock for the bob, both advanced in `Advance`; its view draws `DrawSelectedRow` and `MenuPointer` at the glide's position:
    - `Menus/InventoryPage` with `InventoryListView` (the rail keeps jumping between categories: a category change rebuilds the list);
    - `Menus/EquipmentPage` with `EquipmentSheetView` and `EquipmentChoiceView`;
    - `Menus/OptionsPage`;
    - `Menus/SavePage` and `LoadPage` with `Saving/SaveSlotListView` (through `SaveFilesPanel`).
    - The party page's columns and the sidebar keep jumping (their cursor moves in two dimensions); say so in the Done note.
  - Each page grows by a few lines; check `EquipmentPage` (238) and `InventoryPage` after C5.
  - **Tests:** one per page that the glide follows the cursor, through a small internal read accessor (`SelectionPosition`):
    - `InventoryPageTests`: `Advance_AfterMovingDown_SelectionReachesTheNextRow`
    - `EquipmentPageTests`: `Advance_AfterMovingDown_SelectionReachesTheNextSlot`
    - `OptionsPageTests`: `Advance_AfterMovingDown_SelectionReachesTheNextOption`
    - `SavePageTests`: `Advance_AfterMovingDown_SelectionReachesTheNextFile`
  - **Verify by hand:** on each page, hold Down: the band glides, the diamond bobs, a scroll doesn't make the band fly across the list.
- **Done, with deviations:**
  - **`Ui/SelectionMotion`** holds a list's `SelectionGlide` and its bob clock, so the five lists don't each copy them. It follows a `ListCursor` by its row in the visible window (`ListCursor.VisibleRow`, `VisibleCount`, new), so a scroll that keeps that row in place doesn't glide and a wrap jumps. `MenuPointer.BobDistance` (4 px) replaces the title's own constant. Review: `SelectionMotion.RowIn`/`At` give a `GlidingRow` (the band's place and the bob) and `UiTextures.DrawGlidingSelection` draws it, so the five views don't copy the band and the bobbing pointer. 4 tests in `SelectionMotionTests`.
  - **Who owns it:** `InventoryPage`, `EquipmentPage` (the sheet) and `OptionsPage` own one; `EquipmentChoice` and `SaveSlotList` own theirs as `Selection`. The highlight jumps when the inventory's list is rebuilt (a category change, an item used) and when a page or a choice opens.
  - **The save files glide everywhere:** `SaveFilesPanel.Advance` is called by the Save and Load pages, and by the title's and the defeat screen's file lists too, which share the panel.
  - **Equipment:** the highlight crosses the gap above "Remove all" on its way there.
  - **Tests:** the inventory's test is in a new `InventoryPageSelectionTests`, since `InventoryPageTests` is over 300 lines. 2146 tests (8 new).
  - **Still jumping:** the party page's columns and the party sidebar (their cursor moves in two dimensions), and the item use's target picker (not in this slice).
