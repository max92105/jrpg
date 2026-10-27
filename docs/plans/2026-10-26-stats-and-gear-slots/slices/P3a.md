# P3a. The member's sheet

Part of [the stats and gear slots plan](../README.md). Decisions 295, 297, 300, 302. Design: "The stat sheet", "Formulas" (units and rounding), "Tunables". After P2.

- **P3a. The member's sheet: accuracy, crit chance and damage, dodge, crowd control resistance, Power's percent, in tenths** (Core + Data + content, about 200 lines)
  - **Goal:** a member has a `Sheet` holding the numbers this plan shows or fights with (resistances and Intelligence's parts come in P3b, gear in P4), recomputed at each level-up. Derived percents are held in permille (tenths of a percent, ADR 0026). The battle doesn't read it yet; the Status page (S1) and the battle slices (D1 on) do.
  - **New type:** Core `Stats/StatSheet` (public sealed record): a member's resulting numbers at their level with what they wear: `Primaries`, `MaxHp`, `Speed`, `AccuracyPermille`, `CritChancePermille`, `CritDamagePermille`, `DodgePermille`, `CrowdControlResistancePermille`, `DamagePermille`. Built by `StatSheet.Of(ClassStats atLevel, StatRules rules)` (P4 adds the worn bonuses), so `Stats` never sees `Party` (design.md, "Folder dependencies").
  - **Changes:**
    - `Stats/StatRules`: `BaseAccuracy`, `BaseCritChance`, `BaseDodge`, `BaseCrowdControlResistance`, `DamagePercentPer100Power`, `CritDamagePercentPer100Power`, `CritChancePer100Agility`, `DodgePer100Agility`; `content/rules.json` the design's placeholders; `GameRulesValidator`: rates and bases at least 0.
    - `Stats/StatFormula`: `Accuracy`, `CritChance`, `CritDamage` (`BaseCritDamagePercent` = 150, decision 300), `Dodge` (`DodgeCap` = 50 %, decision 295), `CrowdControlResistance`, `DamagePercent`; each in permille, rounded down once, its comment saying what the conversion is for.
    - `Party/PartyMember`: `Sheet` (rebuilt in `LevelUp`); `Speed` and `Primaries` move into it, and their readers (`Battle/ResourceStarts`, `Menus/StatusSheet`) read `Sheet.Speed`.
    - `TestStatRules.Neutral`: base accuracy 100, base crit 0, base dodge 0, every rate 0 (certain hits and no crits for the battle slices, ADR 0026).
  - **Tests** (`StatFormulaTests`, `PartyMemberStatTests`):
    - `CritChance_TwentyAgility_AddsFivePercent`
    - `CritChance_ThreeAgility_AddsSevenTenths` (a small bonus moves the permille: the reason for tenths)
    - `Dodge_BelowCap_FollowsAgility`
    - `Dodge_HighAgility_StopsAtCap`
    - `CritDamage_ZeroPower_IsOneHundredFifty`
    - `CritDamage_FortyPower_AddsTwenty`
    - `DamagePercent_TwentyPower_IsOneHundredTwenty`
    - `PartyMemberStatTests: GainExperience_LevelUp_RaisesCritChanceWithAgility`
  - **Call sites:** `PartyMember.Speed` 3 (`ResourceStarts`, `StatusSheet`, `CombatantTests` through the rogue).
  - **Size:** about 90 Core, 20 Data and content, 8 tests (about 105): about 200.
  - **Watch:** `PartyMember.cs`: if past 280, move the stat previews (`StatsWith`, `StatsWithout`, `StatsWearing`) to `Party/EquipmentPreview` first (E1 replaces them anyway).
  - **Verify by hand:** none (no screen reads the sheet yet; S1 does).
- **Done, with deviations:**
