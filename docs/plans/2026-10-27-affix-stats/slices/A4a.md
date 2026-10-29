# A4a. Loot rolls take a luck: `LootFormula`

Part of [the affix stats plan](../README.md). Decisions 349, 367; ADR 0026, ADR 0030. After A3. Builds on design.md's "Key decisions" (drop luck).

- **A4a. Loot rolls take a luck: `LootFormula`** (Core + Simulations, about 140 lines: about 50 of code, 90 of tests)
  - **Goal:** the drop rolls take a drop luck in permille and apply it: every drop entry's chance and every rarity weight above Common rise by its percent (decision 349), with no cap on the luck (decision 367) and a chance capped at 100 %. Every caller passes 0 until A4b gives luck a source, so nothing changes in play yet; the split keeps A4b small (review).
  - **New type:**
    - `Loot/LootFormula` (internal static): drop chances and rarity weights under drop luck, one place. `ChancePermille(int chancePercent, int luckPermille)`: the chance in permille × (1000 + luck) ÷ 1000, rounded down, capped at 1000. `RarityWeight(int weight, Rarity rarity, int luckPermille)`: Common unchanged, the others × (1000 + luck) ÷ 1000, rounded down. Comment: luck makes drops and better tiers likelier by the same percent and grows the total rather than removing Common (decision 349); a `long` product so any luck is safe (decision 367: no cap).
  - **Changes:**
    - `Loot/GearRoll.Roll(ItemDefinition item, int luckPermille)`: `RollRarity` weighs each row through `LootFormula.RarityWeight`.
    - `Loot/DropRoll.Roll(IReadOnlyList<DropTable> tables, int luckPermille)`: each entry's chance through `LootFormula.ChancePermille` (its private `PermillePerPercent` goes, the formula converts); passes the luck to `GearRoll.Roll`. The summary says luck raises the chances and the tiers.
    - `Session/GameSession.SpoilsOf`: `_dropRoll.Roll(tables, luckPermille: 0)` with `// TODO(plan 2026-10-27-affix-stats, slice A4b): the active members' drop luck.`
    - `tests/Jrpg.Simulations/Loot/LootSimulation.cs:28`: `.Roll(_tables, luckPermille: 0)` (A4b adds the lucky run).
  - **Tests:**
    - `LootFormulaTests: ChancePermille_ThirtyPercentWithFiftyLuck_Is450`
    - `LootFormulaTests: ChancePermille_NoLuck_IsTheChanceInPermille` (30 is 300)
    - `LootFormulaTests: ChancePermille_HighChanceWithLuck_CappedAtWhole` (80 % with +50 % is 1000)
    - `LootFormulaTests: RarityWeight_Common_Unchanged` (650 with +50 % stays 650)
    - `LootFormulaTests: RarityWeight_RareWithFiftyPercentLuck_IsHalfAgainAsMuch` (70 is 105)
    - `GearRollTests: Roll_DrawPastUnluckyTotal_IsRareWithLuck` (650/280/70 at +50 % is 650/420/105, total 1175: draw 1100 lands on Rare; without luck the total is 1000 and the scripted 1100 would throw)
    - `GearRollTests: Roll_DrawJustPastCommon_IsMagicWithLuck` (draw 650 at +50 % is Magic: Common's 650 is unchanged, so it would be Common if luck raised it too)
    - `DropRollTests: Roll_DrawJustAboveChance_DropsWithLuck` (a 30 % entry, draw 300: no drop without luck, a drop at +50 %)
    - The share maths stay in `LootFormulaTests`; `GearRollTests` and `DropRollTests` pin one draw each on the boundary.
  - **Call sites:** `GearRollTests`' `Roll` helper (line 127) passes 0 and gains a lucky overload or parameter; `DropRollTests`' two call sites (lines 82 and 91) pass 0; `DropRoll`'s own call to `GearRoll.Roll`; `GameSession.SpoilsOf`; `LootSimulation.cs:28`. `GameSession.DropRollFor` builds the roll and is unchanged.
  - **Verify by hand:** none in play (luck is 0 everywhere); the loot simulation's table is unchanged from before the slice.
- **Done, with deviations:** the lucky `GearRollTests` roll through a rarity-only table (`_rarityOnly`: the same weights with no affixes) so each draws its rarity only. Added `LootFormulaTests.RarityWeight_Magic_RaisedAndRoundedDown` (a theory: 280 at +25 % is 350, 3 is 3). `RarityWeight` holds a huge luck's weight at `int.MaxValue`, as `WeightedPick` sums ints. Tests 2778 → 2788. The review caught a comment detached by inserting a test above `Roll_NotEquipment_Throws`'s `[Fact]` (fixed). Negative luck, which would make a negative weight, can't happen: A1's `AffixCatalogValidator` refuses a negative range for an affix-only stat.
