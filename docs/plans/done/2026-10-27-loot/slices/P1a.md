# P1a. The Inventory page shows rarity and affix lines

Part of [the loot plan](../README.md). Decisions 339, 342, 361; waits for question 362 (the colors). Design: "Pages" (Inventory), "Folder dependencies" (Game). After R3b. Reference: `docs/ui-reference/menu-inventory-description.webp` (the description under the selected row).

- **P1a. The Inventory page shows rarity and affix lines** (Game + text, raw 150, about 240 lines at 1.6x: new Ui types)
  - **Goal:** a piece's name is drawn in its rarity's color; under a selected rolled piece, the detail strip reads its rarity and affixes ("Rare · +4 Power · +2 % crit chance") instead of the description (decision 361), hidden with Info like a description (decision 333). A plain piece keeps its description.
  - **New types:**
    - `Ui/RarityNames`: each rarity's name id, Unique included (as `StatusNames`).
    - `Ui/AffixText`: one rolled affix as the player reads it ("+4 Power", "+2 % crit chance"), from `StatNames` and `StatText`'s units.
  - **Changes:**
    - `Ui/UiTheme`: `RarityColorOf(Rarity)`, the four colors question 362 settles (Common the body text color).
    - `Menus/InventoryRow`: `Rarity` (Common for stacks); `Menus/InventoryList` formats a rolled piece's detail line once (rarity name, then each `AffixText`, joined by " · ").
    - `Menus/InventoryListView`: tints the name; `Menus/ItemDetailLine`: a rolled piece's line instead of the description.
    - Text: `rarity_common`, `rarity_magic`, `rarity_rare`, `rarity_unique`; `MessageIds`.
  - **Tests:**
    - `RarityNamesTests: IdOf_EveryRarity_HasText`
    - `AffixTextTests: Of_Primary_ReadsPlusPoints`
    - `AffixTextTests: Of_PercentStat_ReadsPlusPercent`
    - `ItemDetailLineTests: TextFor_RolledPiece_ListsRarityThenAffixes`
    - `ItemDetailLineTests: TextFor_PlainPiece_ShowsDescription`
    - `InventoryPageGearTests: Open_RarePiece_RowIsRare` (a read accessor on the row, not the color)
  - **Verify by hand:** with a rolled piece owned (win until one drops), the Inventory page's Weapons tab shows it in its color; selecting it shows its affix line; Info hides it.
- **Note from G1c:** add `InventoryRow.Piece` (`GearPiece?`, null for a stack) here, set by `InventoryList`'s piece rows: G1c left it out because nothing read it before the rarity color and the affix lines.
- **Done, with deviations:**
  - **Look**, checked in the game on a capture content copy whose goblins drop gear rolled Magic or Rare: a rolled piece's name in its rarity's color (Magic blue `#6F9BFF`, Rare gold `#F0C850`, Unique orange `#F08C32`, decision 362), plain pieces still greyed as items the menu can't use; the selected Rare sword's strip reads "Rare · +4 Power · +3 % Crit chance · +15 % Crit damage". The colors live in `UiTheme.Loot.cs`.
  - The affix line is the row's `Description` for a rolled piece, formatted once by `InventoryList` (`AffixText.LineOf`), so `ItemDetailLine` didn't change and its two planned tests became `AffixTextTests.LineOf_RarePiece_ListsRarityThenAffixesInOrder` and `InventoryPageGearTests.Show_RolledAndPlainPieces_RolledRowIsRareWithItsAffixLine`. `InventoryRow` gained `Rarity` only: `Piece` still has no reader, so G1c's note doesn't apply.
  - Texts: `affix_line` ("{value} {stat}"), `affix_separator` (" · ") and the four rarity names. Added `AffixTextTests.Of_NegativeValue_KeepsItsMinusOnly` and a rarity color test.
  - **Follow-up:** the Weapons and Armor tabs still show the QTY column title, though each of their rows is a piece without a count.
  - Tests 2701 → 2711.
