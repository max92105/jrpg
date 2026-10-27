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
