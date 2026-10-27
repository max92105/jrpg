# G1c. The Equipment page lists nine slots

Part of [the stats and gear slots plan](../README.md). Decisions 252, 298. Design: "The pages" (Equipment page). After G1b. Look at `docs/ui-reference/menu-equipment.webp` (eleven slot rows, then the actions) and a capture of today's page first.

- **G1c. The Equipment page lists nine slots; the Armor tab lists every piece that isn't a weapon** (Game, about 150 lines with the 1.6x)
  - **Goal:** the Equipment page's sheet lists Weapon, Off-hand, Head, Chest, Hands, Feet, Amulet, Ring, Ring, then "Remove all", with the Changes section (E1) under them; all of it fits 848 px. Choosing a ring slot lists every owned ring and every ring other members wear. Plain look as today; E2 does the look.
  - **Changes:**
    - `Menus/EquipmentSheet.Slots`: the nine; `LabelIdOf` nine names (`slot_off_hand`, `slot_head`, `slot_hands`, `slot_feet`, `slot_amulet`, `slot_ring`; both ring slots read "Ring") in `ui.json` and `MessageIds`.
    - `Menus/EquipmentSheetView`: rows 48 px (nine rows 432 px; the Changes section's two columns of four lines below "Remove all" end near 800 px); the band's gap rule before "Remove all" unchanged.
    - `Menus/EquipmentPage`: `RemoveAll` walks the nine.
  - **Tests:**
    - `EquipmentSheetTests: Of_Member_ListsNineSlotsInOrder`
    - `EquipmentPageTests: Update_ConfirmOnSecondRing_ListsOwnedRings`
    - `EquipmentPageTests: Update_RemoveAll_EmptiesEverySlot`
  - **Existing tests that change:** the `EquipmentPageTests` that count rows to reach "Remove all" (pick the entry by what it is, `.claude/rules/tests.md`); count them.
  - **Watch:** `EquipmentPage.cs` (255): no growth expected beyond a line or two.
  - **Size:** raw about 70 (view 45, sheet and page 10, messages 15); the view part at 1.6x (45 → 72): about 100; 3 tests about 40; changed tests about 10: about 150.
  - **Verify by hand:** nine rows and "Remove all" fit with the Changes section; the selection glides through all ten; Esc closes a choice. F12.
- **Done, with deviations:**
  - **Rows 40 px, not 48:** with 48 px rows the Changes section's four lines, its "+N more" line and the notice ran to about 936 px. At 40 px rows, 40 px Changes lines and a 12 px notice gap (24 before), the page ends near 842 of 848; the arithmetic is in `EquipmentSheetView`'s layout comment. E2 redoes the look anyway.
  - **Tests pick rows by slot:** a new `EquipmentPageRows` test helper (`Of(slot)`, `RemoveAll`, `MoveDown(page, rows)`) replaced the fixed Down presses in `EquipmentPageTests` and `EquipmentPageComparisonTests`; three comparison tests had counted to the chest row, and one of them looped forever on the empty off-hand choice once the rows moved. `Update_RemoveAll_EmptiesBothSlots` became `Update_RemoveAll_EmptiesEverySlot` (a ring in the second ring slot too).
  - **The ring test** reads a catalog copy with a test ring, since content has none until G2.
  - **Tests:** `EquipmentSheetTests` 1, `EquipmentPageTests` 1 new and 1 widened. Count: 2,568 → 2,570.
  - **Hand check (yours):** nine rows and "Remove all" fit with the Changes section; the selection glides through all ten; Esc closes a choice. F12 a capture: the rows are tighter than before until E2.
  - **Follow-up (reviewer):** a hand-built `power_ring` item now lives in three test files (`EquipmentPageTests`, `FileSaveStoreTests`, `SaveGameRepairWornTests`); Core's `ItemDefinitionBuilder` can't be reached from the Game and Data test projects. G2 puts a real ring in content, so the three can read it from the catalog then.
