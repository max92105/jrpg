# M3b. Reroll in Core

Part of [the forge plan](../README.md). Decisions 352, 369. After M3a. Was half of the draft's M3 (split on 2026-10-29). Design: [design.md](../design.md), "Core changes", "Key decisions".

- **M3b. Reroll in Core: every affix drawn again** (Core, about 120 lines)
  - **Goal:** an owned piece with affixes rerolls for the reroll cost: every affix drawn again from every affix for its kind (the same count, the same rarity, never one twice; decision 352), the piece keeps its place. Refused, with nothing changed and nothing drawn, without affixes or when short of materials.
  - **New types:** none.
  - **Changes:**
    - `Crafting/CraftingRules.RerollCost { get; init; }`: `IReadOnlyList<InventoryEntry>`, empty by default.
    - `Crafting/ForgeRefusal.NoAffixes`.
    - `Loot/GearRoll.Reroll(GearPiece piece)`: the piece with as many affixes as it had, drawn through M3a's `DrawAffixes` with none kept. No luck parameter.
    - `Crafting/Forge`: `ForgeQuote QuoteReroll(GearPiece piece)` (`NoAffixes`, then `ShortOfMaterials`, both with the cost); `bool TryReroll(GearPiece piece)` (checks, pays, `Inventory.Replace`).
  - **Tests:**
    - `GearRollCraftingTests: Reroll_PinnedDraws_GivesThoseAffixesNotTheOldOnes` (the pinned draws pick affixes and values that differ from the original's)
    - `GearRollCraftingTests: Reroll_RarePiece_KeepsRarityAndCount`
    - `ForgeRerollTests: TryReroll_WithMaterials_NewAffixesAndCostTaken` (pinned draws differing from the original lines)
    - `ForgeRerollTests: QuoteReroll_PlainPiece_RefusedNoAffixes`
    - `ForgeRerollTests: TryReroll_ShortOfMaterials_ReturnsFalseAndKeepsAll`
  - **Call sites:** none.
  - **Watch:** a piece whose saved affixes no longer roll on its kind (content changed since) rerolls into as many as the kind offers; `GearRepair` keeps such affixes today, so this is the rare case, not an error.
- **Done, with deviations:** `TryReroll` checks the quote, then ownership, before drawing (as `TryUpgrade`); added `ForgeRerollTests.TryReroll_NotOwned_ReturnsFalseAndDrawsNothing` (the excluded case, decision 369). The refused quote names the cost (`QuoteReroll_PlainPiece_RefusedNoAffixes` asserts both). The Rare reroll's draws give the crit chance first, so the old order isn't kept either. Tests 2841 → 2847. Follow-up from the review: a Unique piece with affixes could be rerolled (salvage and upgrade refuse it); no Unique exists yet, so the Uniques plan decides.
