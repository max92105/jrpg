# M3a. Upgrade in Core

Part of [the forge plan](../README.md). Decisions 351 (with its 2026-10-29 clarification), 357, 369. After M2, and after the affix stats plan's A4a (`GearRoll.Roll(item, luckPermille)`). Was half of the draft's M3 (split on 2026-10-29). Design: [design.md](../design.md), "Core changes", "Key decisions", "Replacement mapping".

- **M3a. Upgrade in Core: one more affix from the shared `GearRoll`** (Core + Simulations, about 250 lines)
  - **Goal:** an owned Common or Magic piece upgrades for its next tier's cost: the materials are taken, the piece becomes Magic with one affix or Rare with a second, the new affix drawn from the pool for its kind, never one it has; it keeps its place in the list. Refused, with nothing changed and nothing drawn, at Rare (Unique too), when the pool has no affix left for its kind (decision 357), or when short of materials. Drops and the Forge share one `GearRoll`.
  - **New types:** none.
  - **Changes:**
    - `Crafting/CraftingRules.UpgradeCosts { get; init; }`: `IReadOnlyDictionary<Rarity, IReadOnlyList<InventoryEntry>>` by the tier reached (Magic, Rare), empty by default.
    - `Crafting/ForgeRefusal`: `HighestRarity`, `NoAffixLeft`, `ShortOfMaterials`.
    - `Loot/GearRoll`:
      - the affix loop leaves `Roll` for a private `DrawAffixes(EquipmentKind kind, int count, IReadOnlyList<RolledAffix> kept)` (the candidates are the kind's affixes not in `kept`); `Roll` passes none kept. Existing `GearRollTests` pass unchanged (the guard).
      - `bool HasAffixLeftFor(GearPiece piece)`: some affix for the piece's kind it doesn't have; draws nothing.
      - `GearPiece? AddAffix(GearPiece piece, Rarity raisedTo)`: the piece at `raisedTo` with one more affix, after its own; null when none is left. No luck parameter (luck is a drop's, A4a).
    - `Items/Inventory.Replace(GearPiece old, GearPiece replacement)`: puts the replacement where the first piece equal to `old` is; false, changing nothing, when none is.
    - `Crafting/Forge`: the constructor takes the `GearRoll` (four parameters, still internal); `ForgeQuote QuoteUpgrade(GearPiece piece)` (refusals in order: `HighestRarity` with no materials, `NoAffixLeft`, `ShortOfMaterials`, both with the cost); `bool TryUpgrade(GearPiece piece)` (checks first, then pays, adds the affix and replaces the piece).
    - `Session/GameSession`: `internal static GearRoll GearRollFor(ContentCatalog catalog, IRandom random)` (the pool in id order, the comment kept) replaces `DropRollFor`; the constructor builds one `GearRoll` and gives it to the `DropRoll` and the `Forge`.
    - `tests/Jrpg.Simulations/Loot/LootSimulation.cs:28`: builds its `DropRoll` from `GameSession.GearRollFor` (keeping whatever luck argument A4a gave it).
  - **Tests:**
    - `ForgeUpgradeTests: TryUpgrade_CommonWithMaterials_MagicWithOneAffixAndCostTaken` (pinned draws)
    - `ForgeUpgradeTests: TryUpgrade_Magic_RareWithASecondAffix` (decision 351: two affixes, the old one first)
    - `ForgeUpgradeTests: TryUpgrade_ShortOfEssence_ReturnsFalseAndKeepsAll` (a Magic piece, 5 Scrap and 0 Essence against the Rare cost of 5 Scrap and 1 Essence: the Scrap stays at 5)
    - `ForgeUpgradeTests: QuoteUpgrade_Rare_RefusedHighestRarity`
    - `ForgeUpgradeTests: TryUpgrade_NoAffixLeftForKind_ReturnsFalseAndCostsNothing` (decision 357; a custom pool with one affix for the kind, the piece already has it)
    - `GearRollCraftingTests: AddAffix_PieceHasOne_NeverRepeatsIt` (new file: `GearRollTests` is 134 plus A4a's test)
    - `GearRollCraftingTests: AddAffix_PoolExhausted_ReturnsNull`
    - `InventoryGearTests: Replace_OwnedPiece_KeepsItsPlace`
    - `InventoryGearTests: Replace_NotOwned_ReturnsFalseAndChangesNothing`
  - **Call sites:** `GameSession.DropRollFor` (2: the constructor, `LootSimulation`); `new GearRoll(` in `GearRollTests` (1) and `DropRollTests` (2) unchanged; `new Forge(` in `ForgeSalvageTests` (its fixture) gains the `GearRoll`.
  - **Watch:** `GameSession` about 245. `HasAffixLeftFor`'s two sides are both reached through `Forge` (the success tests and the 357 test); `InventoryGearTests` 74 (+25).
- **Done, with deviations:** `Inventory.Replace` is `TryReplace`, as `TryRemoveGear` (it returns false when no piece is equal). `TryUpgrade` checks the quote, then ownership (`Gear.Contains`), then draws, pays and replaces, so an unowned piece draws nothing. A missing cost row for the next tier refuses `HighestRarity` (the tiers a craft reaches are the rows content gives). Added `QuoteUpgrade_Common_IsTheMagicCost` (a refused quote still names the cost) and `TryUpgrade_NotOwned_ReturnsFalseAndDrawsNothing` (the excluded case, decision 369). `LootSimulation` builds its `DropRoll` from `GameSession.GearRollFor` through a local `DropRollFor`. Tests 2828 → 2841. The review added the other two ways to `HighestRarity` (Unique in a theory with Rare, and a missing Rare cost row) and the Forge to `GearRoll`'s summary.
