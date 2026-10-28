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
- **Done, with deviations:**
  - **Look**, checked in the game on the capture content copy whose goblins drop Magic or Rare gear: the victory Items step draws each rolled piece in its color with its rarity's name right-aligned where a stack's "×n" goes; the Equipment page's weapon choice draws plain swords in the body text, Magic ones blue and the Rare one gold. A worn rolled piece's slot row takes its color the same way (tested, not captured: the capture's leader wore nothing).
  - `VictoryDropRow.Quantity` became `Tag`: "×n" for a stack, the rarity's name for a piece above Common. Another member's piece in the choice stays dimmed unless it rolled.
  - `EquipmentSheet` needed no change: its worn rows already hold the pieces, so `EquipmentSheetTests.Of_RarePieceWorn_RowIsRare` reads the piece's rarity there. The choice's colors are built in `Show` and read through `EquipmentChoiceView.OwnedColors`.
  - **Tooling:** `reflow-comments.pl` (through `verify.ps1 -Fix`) dropped a slash from a `///` line twice in this plan (S1a's `SaveGameFixtures`, this slice's `VictoryDropRow`), turning it into `//`; noted in the backlog.
  - The review added `EquipmentChoiceViewTests.Show_RareTakeablePiece_TakesItsColorPlainStaysDimmed` (`WornColors`). Tests 2711 → 2715.
