# P4a. Gear bonuses in content; `StatSheet.ValueOf`

Part of [the stats and gear slots plan](../README.md). Decisions 293, 295. Design: "The stat sheet", "Content shapes", "Tunables" (gear bonuses). After P3b. P4 was split in two on 2026-10-27 (the developer's OK): about 430 lines with tests once the bonuses' JSON reading was counted. P4b makes what is worn count.

- **P4a. Equipment carries bonuses by stat; any of the twenty stats reads through `StatSheet.ValueOf`** (Core + Data + content, about 220 lines)
  - **Goal:** equipment in content carries `bonuses` by stat kind, read and validated by Data; a screen reads any of the twenty stats through one `StatSheet.ValueOf(kind)`. Members' numbers don't change yet (P4b).
  - **New types:**
    - Core `Stats/StatKind` (public enum): the twenty stats a sheet shows and gear can add: the five primaries, MaxHp, Speed, Accuracy, CritChance, CritDamage, Dodge, CrowdControlResistance, and one resistance per damage type; `Stats/StatKinds.ResistanceTo(DamageType)` maps a type to its kind.
    - Core `Stats/StatBonuses` (public sealed record): what an item adds, by stat kind, whole points or whole percents; `Of(kind)`, `None`; value equality over its entries.
    - Data `Items/StatBonusesConverter`: reads `"bonuses": { "power": 5, "fireResistance": 10 }` (camelCase stat names) and refuses an unknown name with it named. Core carries no JSON attributes (ADR 0002: Data adapts a file's format).
  - **Changes:**
    - `Items/EquipmentStats`: gains `Bonuses` (`StatBonuses`, init, default `None`); `AttackBonus` and `DefenseBonus` stay until M1 and M2b.
    - `Stats/StatSheet.ValueOf(StatKind)`: the value a screen shows for a kind (whole points, or permille for percent kinds); the one switch over the twenty.
    - `Data/Items/ItemCatalogValidator`: each bonus nonzero and within −100..100, the stat named.
    - `content/items.json`: the design's bonuses on the three armors and the magic staff.
    - `ItemDefinitionBuilder.WithBonuses(attack, defense)` becomes `WithAttackAndDefense` until M2b deletes it (3 sites); `WithBonuses(StatBonuses)` is new.
  - **Tests:** `StatSheetTests: ValueOf_EveryKind_ReadsItsField` (a theory over the twenty kinds against a sheet with distinct values); `StatKindsTests: ResistanceTo_Fire_IsFireResistance`; `StatBonusesTests` (`Of`, equality); `ItemCatalogValidatorTests: Validate_ZeroBonus_NamesTheStat`, `Validate_BonusAbove100_NamesTheStat`; `StatBonusesConverterTests` (reads camelCase, refuses an unknown name); `ContentCatalogLoaderTests: Load_RepositoryChainMail_HasConstitution`.
  - **Verify by hand:** none (P4b makes the bonuses count; S1 and E1 show them).
- **Done, with deviations:** `StatSheet.ValueOf` reads resistances through `StatKinds.TypeResistedBy` and primaries through a new `PrimaryStats.ValueOf`, so no switch passes the build's complexity limit of 10. `ItemDefinitionBuilder.WithBonuses(StatBonuses)` waits for P4b's gear tests (its first user); the renamed `WithAttackAndDefense` had 1 site, not 3. The validator also has `Validate_NegativeBonusWithinBounds_Passes` (a cursed item may lower a stat). Extra tests: `StatKindsTests.TypeResistedBy_*` (every type both ways, and null for a non-resistance), `StatBonusesTests` (equality in any order, a copied dictionary), `StatBonusesConverterTests` (an object, unknown and repeated names), `PrimaryStatsTests`. From the review: `StatBonuses`' summary counts accuracy among the percents (as on the sheet, so P4b multiplies its bonus by 10), its entries are a frozen dictionary, and the converter's `Write` has a round-trip test. Members' numbers are unchanged until P4b. Tests: 2311 → 2357.
