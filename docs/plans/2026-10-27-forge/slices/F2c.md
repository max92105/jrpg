# F2c. Upgrade and reroll at the Forge

Part of [the forge plan](../README.md). Decisions 351 (a crafted Rare has two affixes), 352, 357. After F2b. Was the crafting part of the draft's F2. Design: [design.md](../design.md), "Key decisions" (the piece replaced in place; the cursor on the changed piece).

- **F2c. Upgrade and reroll carried out; the cursor stays on the changed piece** (Game + text, about 150 lines; adds no view type)
  - **Goal:** Confirm on an available Upgrade or Reroll carries it out through `Forge.TryUpgrade` or `TryReroll`, without a prompt (nothing is destroyed): the list, the detail column and the materials show the new piece and counts, the highlight stays on the piece that changed, and a notice says what changed ("Bronze Sword is now Magic.", "Bronze Sword's affixes were rolled again.") until the cursor moves. The actions stay open on the new piece, with its new quotes. Confirm on a greyed Upgrade or Reroll does nothing.
  - **New types:** none.
  - **Changes:**
    - `Crafting/ForgeScene`: reads `Inventory.Gear.IndexOf(piece)` before the action (with two equal pieces, `Inventory.Replace` changes the first, so that is the row that changed), carries it out, then `ForgeList.Show(index)`, `MaterialStock.Refresh()`, a new `ForgeActionChoice` on the new piece, and the notice.
    - Text: `forge_upgraded` ("{name} is now {rarity}."), `forge_rerolled` ("{name}'s affixes were rolled again."); `MessageIds`.
  - **Tests** (`ForgeSceneCraftTests`, new file; seed-independent: they assert the rarity and the count of lines, not which affixes):
    - `Confirm_UpgradeAffordable_SameRowShowsMagicWithOneAffix`
    - `Confirm_UpgradeOnSecondOfTwoEqualPieces_HighlightsTheUpgradedPiece` (two equal plain pieces, the second highlighted: the highlight moves to the first row, which holds the Magic piece)
    - `Confirm_RerollAffordable_CostTakenAndLinesRebuilt` (a Magic piece; the Scrap and Essence counts fall by the content's cost, the row's lines are the new piece's)
    - `Confirm_GreyedUpgrade_DoesNothing` (the guard: a Rare piece; nothing changes, no draw)
  - **Call sites:** none.
  - **Verify by hand:** with Scrap and Essence: upgrade a plain cap to Magic: it turns blue with one affix, Scrap falls by 3; upgrade it to Rare: gold, two affixes, then Upgrade greyed "Highest rarity"; reroll it: both lines change (or not, a fresh draw), Scrap and Essence fall by the shown cost.
