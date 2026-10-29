# R3a. The affix pool and the rarity table in content

Part of [the loot plan](../README.md). ADR 0030, decisions 339, 340. Design: "Tunables" (rarities, affixes), "Content shapes". After R2.

- **R3a. The affix pool and the rarity table in content** (Data + content, about 220 lines)
  - **Goal:** `affixes.json` and `rules.json` `loot` load into `ContentCatalog.Affixes` (empty since S1a) and `GameRules.Loot`, validated with the file and field named. Drops still give plain pieces until R3b.
  - **New types:**
    - `Data/Loot/AffixCatalogValidator`: checks the affix pool file: ids unique, `min` and `max` nonzero with `min ≤ max` and within the item bonus bound (100), at least one kind and no null kind, `weight` at least 1.
    - `Data/Loot/LootRulesValidator`: checks `rules.json` `loot`: Common, Magic and Rare present and no Unique row (decision 339), weights at least 0 with one above 0, `minAffixes ≤ maxAffixes`, Common's 0, and at most 5 (placeholder bound).
  - **Changes:**
    - `Content/GameRules` gains `Loot` (`LootRules`); `ContentCatalogBuilder` passes a Common-only table (weight 1, 0 affixes) so test drops stay plain.
    - `Data/Content/ContentCatalogLoader`: reads `affixes.json` after items; `GameRulesValidator` calls `LootRulesValidator`. `Rarity` and `EquipmentKind` read as camelCase names, `stat` through `StatKind`'s camelCase names (as `StatBonusesConverter`).
    - Content: `affixes.json` with design.md's rows; `rules.json` `loot.rarities` (design.md).
  - **Tests:**
    - `AffixCatalogValidatorTests: Validate_MinAboveMax_Throws`
    - `AffixCatalogValidatorTests: Validate_NoKinds_Throws`
    - `AffixCatalogValidatorTests: Validate_NullKind_Throws`
    - `AffixCatalogValidatorTests: Validate_WeightZero_Throws`
    - `AffixCatalogValidatorTests: Validate_UnknownStat_ThrowsNamingIt`
    - `LootRulesValidatorTests: Validate_MissingRarity_Throws`
    - `LootRulesValidatorTests: Validate_UniqueRow_Throws`
    - `LootRulesValidatorTests: Validate_EveryWeightZero_Throws`
    - `LootRulesValidatorTests: Validate_MinAffixesAboveMax_Throws`
    - `RepositoryLootTests: AffixPool_HasPowerForWeapons`
  - **Call sites:** `new GameRules(` in `ContentCatalogBuilder` (1).
- **Done, with deviations:**
  - An unknown stat name and a null kind are refused when the file is read, as any bad enum name is, so their tests are a `ContentFileReaderTests` theory with two fixtures (`affix-with-unknown-stat.json`, `affix-with-null-kind.json`) rather than `AffixCatalogValidatorTests`. The validator's own checks are one theory (min above max, min 0, max past 100, no kinds, weight 0).
  - `ItemCatalogValidator.MostBonus` became internal, so an affix's range has the item bonus's bound.
  - `LootRulesValidator` also refuses a null row (the dictionary null-entry rule). Its checks are one theory (negative weight, min above max, max above 5, Common with an affix, a Unique row) plus missing, null and all-zero tests.
  - The review added a range crossing 0 to the refusals (a roll there could add nothing) and `GameRulesValidatorTests.Validate_NegativeRarityWeight_NamesTheField` (the table is checked with the file). Tests 2675 → 2695.
