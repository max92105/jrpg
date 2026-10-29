# L2b. The Shop's detail column

Part of [the gold and shops plan](../README.md). Decisions 335 (section bands), 362. References: `docs/ui-reference/menu-inventory-description.webp` (the description under a selected item), `menu-equipment.webp` (a detail column). After L2a. The party's stat preview (the reference's right column) is Q9, out of scope. Design: [design.md](../design.md), "Extension points".

- **L2b. The Shop's detail column: the highlighted item's description, owned count, a piece's affixes** (Game, raw 95, about 150 lines at 1.6x: adds a view type)
  - **Goal:** beside the list, the highlighted row's name (a piece in its rarity's color), its description, "Owned 3" (the stacks' count or the pieces of its base) and, for a rolled piece on the Sell tab, its affix lines (`Ui/AffixText`); room under them for T1's reason or notice line.
  - **New types:**
    - `Game/Trade/ShopDetailView`: draws the detail column: name, description wrapped once when the row changes, the owned line, the Affixes band and lines for a rolled piece, the reason or notice line.
  - **Changes:**
    - `Trade/ShopView`: places `ShopDetailView`; `StockRow` and `SaleRow` carry the description and affix lines formatted once.
    - Text `shop_owned` ("Owned {count}"); `MessageIds`.
  - **Pixel arithmetic** (detail column, x 1040 to 1880, 840 wide, from 128 to 976: 848):
    - name row 56; description up to 3 lines x 32 = 96 (wrapped at 840 when the row changes; the longest content description measured in the slice with the real font); gap 24; owned line 40; gap 24; Affixes band 46 + 3 rows x 40 = 166; gap 24; reason or notice line 40. Total 470 ≤ 848.
  - **Tests:**
    - `SaleListTests: Rows_RolledPiece_CarriesItsAffixLines`
    - `StockListTests: Rows_Item_CarriesItsDescription`
    - Draw-only: captures.
  - **Verify by hand:** at the shopkeeper: Down through the stock shows each description and owned count; on Sell, a Magic piece shows its affix under the band.
