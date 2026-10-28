# G1c. The Inventory page lists each piece

Part of [the loot plan](../README.md). ADR 0029, decision 345. Design: "Pages" (Inventory). After S1a; closes G1b-1's Inventory page gap. Builds on C1's `InventoryList`.

- **G1c. The Inventory page lists each piece** (Game, about 130 lines; no new view type)
  - **Goal:** every tab lists stacks, then pieces, each in pickup order; a piece's row shows no quantity. Confirm on a piece does nothing (gear isn't used from the menu).
  - **Changes:**
    - `Menus/InventoryFilter`: `Select(category, inventory, items)` returns the rows' sources: the kept stacks, then the kept pieces (Weapons: weapon pieces; Armor: every other piece; All: everything; Consumables: stacks with an effect).
    - `Menus/InventoryRow`: `Quantity` becomes `MeasuredText?` (null for a piece); `Piece` (`GearPiece?`) beside `ItemId`.
    - `Menus/InventoryList`: builds piece rows (name and icon from the base, its description).
    - `Menus/InventoryListView`: skips the quantity when null.
  - **Tests** (new file `InventoryPageGearTests`; `InventoryPageTests` was split in C1):
    - `InventoryPageGearTests: Open_WeaponsTab_ListsEachSwordPiece`
    - `InventoryPageGearTests: Open_AllTab_ListsStacksThenPieces`
    - `InventoryPageGearTests: Confirm_OnPiece_DoesNothing`
    - `InventoryFilterTests: Select_Armor_KeepsEveryPieceButWeapons`
  - **Call sites:** `InventoryFilter.Select` (the list and its tests), `InventoryRow` constructions (2).
  - **Verify by hand:** a new game's Inventory page: Weapons shows two Bronze Sword, one Iron Sword and three Magic Staff rows without numbers; potions keep their counts. The All tab's order changes: every stack first, then every piece (no longer one pickup order across both).
