# P1b. Rarity on the Equipment page and the victory drops

Part of [the loot plan](../README.md). Design: "Pages" (Equipment, Victory). After P1a (`RarityColorOf`, `RarityNames`).

- **P1b. Rarity on the Equipment page and the victory drops** (Game, raw 100, about 160 lines at 1.6x)
  - **Goal:** every piece name on the Equipment page (the worn rows and the choice's rows) and on the victory screen's Items step is drawn in its rarity's color; the Items step names a non-Common piece's rarity beside it.
  - **Changes:**
    - `Menus/EquipmentSheet`: the worn pieces' rarities; `EquipmentSheetView` tints each worn name.
    - `Menus/EquipmentChoiceView`: tints each piece's name when the choice is shown (formatted in `Show`, not in `Draw`).
    - `Battle/VictoryDropRow`: `Rarity`; `VictoryDropsView` tints the name and writes the rarity name after it for a piece above Common.
  - **Tests:**
    - `EquipmentSheetTests: Of_RarePieceWorn_RowIsRare`
    - `EquipmentChoiceViewTests: Show_RarePiece_RowIsRare` (a read accessor, as the stats plan's `ShownStats`)
    - `VictoryDropsViewTests: RowsOf_RarePiece_IsRareAndNamed`
  - **Verify by hand:** a rolled piece shows in its color in its slot row and in the choice list; a victory that drops one shows it colored with "Rare" beside it.
