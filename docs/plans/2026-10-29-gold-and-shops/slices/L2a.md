# L2a. The Shop's detail column

Part of [the gold and shops plan](../README.md). Decisions 335 (section bands), 362, 385 (a stock piece's affixes). References: `docs/ui-reference/menu-inventory-description.webp` (the description under a selected item), `menu-equipment.webp` (a detail column). After B3. Design: [design.md](../design.md), "Risks" (the detail column's height).

- **L2a. The detail column: name, description, owned count, a piece's affixes** (Game + text, raw 110, about 175 lines at 1.6x: adds a view type)
  - **Goal:** beside the list, the highlighted row's name (a piece in its rarity's color), its description, "Owned 3" (the stacks' count or the pieces of its base) and, for a rolled piece on either tab, its affix lines under an Affixes band (`Ui/AffixText`, as the Forge's detail column). Room under them for L2b's preview and T1's reason or notice line.
  - **New types:**
    - `Game/Trade/ShopDetailView`: draws the detail column: name, description wrapped once when the row changes, the owned line, the Affixes band and lines for a rolled piece, the reason or notice line (empty until T1).
  - **Changes:**
    - `Trade/StockRow` and `Trade/SaleRow`: the description and affix lines formatted once.
    - `Trade/ShopView`: places `ShopDetailView` in the detail column.
    - Text `shop_owned` ("Owned {count}"); `MessageIds`.
  - **Pixel arithmetic** (the detail column, x 1040 to 1880, 840 wide, 128 to 976: 848): name row 56; description up to 3 lines x 32 = 96 (wrapped at 840 when the row changes; the longest content description measured in the slice with the real font); owned line 40; Affixes band 46 + 3 lines x 40 = 166; reason or notice line 40; three 16 px gaps 48. Total 446, leaving 402 for L2b's preview (its arithmetic).
  - **Tests:**
    - `StockListTests: Rows_Item_CarriesItsDescription`
    - `SaleListTests: Rows_RolledPiece_CarriesItsAffixLines`
    - Draw-only: captures.
  - **Verify by hand:** at the shopkeeper: Down through the stock shows each description and owned count; the rolled sword shows its affix lines under the band; on Sell, a Magic piece shows its affix.
