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
  - **The bob's clock lives in `SelectionGlide`** (4 px), so each list keeps one object, not a glide and a clock. After review, the glide also places the band (`BandOver`) and the pointer (`PointerAt`), so the five views don't copy that math; it ticks for the tabs and the title too, where nothing reads it (the title keeps its own clock).
  - **Scrolling lists follow the cursor's row in the visible window** (`SelectionGlide.FollowShown(ListCursor)`, with the new `ListCursor.WindowSize`), so a scroll leaves the band still and a wrap jumps; 2 more `SelectionGlideTests`.
  - **Who owns each glide:** the Inventory, Options and Equipment pages own theirs; `SaveSlotList` and `EquipmentChoice` own theirs (`Selection`), since they own the cursor. The save list's time comes through `ISaveFilesPage.Advance`, called by `SystemPage.Advance`. A new category or a used item puts the inventory's band in place at once.
  - **Equipment's "Remove all"** sits a gap below the slots, and its band crosses the gap too (`EquipmentSheetView.BandTop`).
  - **Unchanged:** the party page's columns and the sidebar keep jumping, since their cursor moves in two dimensions; so do the category rail and the System page's entries.
  - **Tests:** 2144 (from 2138): the four planned, plus the two `FollowShown` tests.
