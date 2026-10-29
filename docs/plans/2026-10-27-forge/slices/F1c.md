# F1c. The detail column

Part of [the forge plan](../README.md). Decisions 361 (a rolled piece's lines), 370 (every material listed, 0 included). After F1b. Was the detail half of the draft's F1. Reference: `docs/ui-reference/menu-equipment.webp`, `menu-status.png` (values beside spaced-caps labels). Design: [design.md](../design.md), "Structure and folder dependencies" (`SheetRowLook` to `Ui/`), "Replacement mapping".

- **F1c. The detail column: the highlighted piece's affixes and every material owned** (Game + text, about 150 lines; extends `ForgeView`, adds no view type)
  - **Goal:** beside the list, the highlighted piece's name, rarity and affix lines, and under them every material in the catalog with how many the party owns, 0 included (decision 370: the costs name them). Formatted when the items or the highlight change, never in `Draw`.
  - **New types:**
    - `Game/Crafting/ForgeRow(GearPiece Piece, IReadOnlyList<string> AffixLines)`: one owned piece as the Forge lists it, its affixes formatted once (`Ui/AffixText.Of`, one line per affix so the detail column draws them as rows). Replaces `ForgeList.Pieces` with `Rows`.
    - `Game/Crafting/MaterialCount(string Name, MeasuredText Count)`: one material and how many are owned, measured for its row.
    - `Game/Crafting/MaterialStock`: the materials panel's rows, rebuilt when the items change. `MaterialStock(Inventory inventory, IReadOnlyDictionary<string, ItemDefinition> items, SheetRowLook rows)` (measures with the row look, so drawing formats nothing); `IReadOnlyList<MaterialCount> Rows` (every `IsMaterial` item, in id order: the catalog keeps no file order); `Refresh()`.
  - **Changes:**
    - First, a mechanical move: `Menus/SheetRowLook` to `Ui/SheetRowLook`, owning its label size, value size and label spacing as public constants that `StatusSheetView` then reads (Ui must not depend on Menus; game.md). The Status and Equipment tests pass unchanged (the guard).
    - `Crafting/ForgeList`: takes the messages; builds `ForgeRow`s.
    - `Crafting/ForgeScene`: builds the `MaterialStock` (the catalog's items, from `SceneFactory`).
    - `Crafting/ForgeView`: the detail column: name (rarity color) and rarity name, the affix lines, then the materials as `SheetRowLook` rows (name, count). Plain placement.
    - Text `forge_materials` ("Materials"), `forge_no_affixes` ("No affixes"); `MessageIds`.
  - **Tests:**
    - `ForgeListTests: Show_RolledPiece_RowHasItsAffixLines`
    - `MaterialStockTests: Rows_NoneOwned_ListsEveryMaterialAtZero` (new file)
    - `MaterialStockTests: Refresh_AfterScrapAdded_ShowsTheNewCount` (starts from a count that differs from the expected one, tests rule)
  - **Call sites:** `ForgeList.Pieces` readers (`ForgeView`, `ForgeListTests`); `SheetRowLook`'s `using` in `EquipmentChangesView`, `EquipmentSheetView`, `StatusParametersView`.
  - **Verify by hand:** at the forge: the highlighted piece's affixes beside the list, changing as the highlight moves; Scrap and Essence listed, 0 included; win Scrap from goblins, come back: its count is up.
