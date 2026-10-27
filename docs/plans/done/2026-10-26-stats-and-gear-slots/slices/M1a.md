# M1a. Power scales attacks and skills

Part of [the stats and gear slots plan](../README.md). Decisions 296 (Power is magnitude), 318, 321 (consumables don't scale), 330 (one unarmed rule). Design: "Formulas" (order on a skill, healing), "Replacement mapping". After D3. Split from M1 on 2026-10-27: the pages' "Weapon damage" line (decision 337) is M1b.

- **M1a. Power scales attacks and skills; weapons deal damage; attack leaves classes and items** (Core + Data + content + tests, about 270 lines)
  - **Goal:** a member's basic attack deals their weapon's damage (or the unarmed damage from `rules.json`), raised by Power's percent; every skill's raw value (damage and healing) is raised by its user's Power too. Consumables land their own numbers (decision 321). A class no longer has an attack, nor a weapon an attack bonus.
  - **Pages:** unchanged; they build as they are, and their Attack line shows the raw weapon or unarmed damage until M1b (decision 337).
  - **New type:** Core `Battle/EffectScaling` (internal static): an effect as its user lands it: damage, healing and revive power times the user's damage permille, in a `long`, rounded down once (I1 adds the grant under Intelligence). An enemy's is 1000.
  - **Changes:**
    - `Stats/StatRules.UnarmedDamage`; `rules.json` 18; `GameRulesValidator` (at least 1).
    - `Items/EquipmentStats`: `Damage` (init) replaces `AttackBonus`; `ItemCatalogValidator`: a weapon's damage at least 1, any other kind 0; `content/items.json` weapons 24, 30, 20.
    - `Party/ClassStats`: loses `Attack` (`ClassGrowth`, `ClassCatalogValidator`, `content/classes.json`).
    - `Stats/CombatStats`: `Attack` now means the raw attack (weapon or unarmed; the enemy's `attack` until M2b renames it); gains `DamagePermille` (init, default 1000), a member's from the sheet through `MemberBattleStats`.
    - `Battle/AttackEffect.Of`: through `EffectScaling`. `BattleState.UseSkill`: `EffectScaling` first, then `SkillEmpowerment` (which now takes the scaled effect: `EffectOf(actor, skill, effect)`), so a dominant force raises the scaled value. `UseItem` unchanged.
    - Test support (review edit): `PartyMemberBuilder.WithStats(attack, defense)` **equips a test weapon** with `Damage = attack` (built by `ItemDefinitionBuilder.AsWeapon(damage)`), so battle tests keep their numbers under the one shared `TestStatRules.Neutral` (no per-member rules). Tests about what is worn (`PartyEquipmentTests`, `PartyEquipmentTakeTests`, `PartyMemberGearTests`) build members without `WithStats`, or say why the test weapon doesn't matter; count the ones that change.
  - **Tests** (new ones in `EffectScalingTests`, `BattlePowerTests`):
    - `EffectScalingTests: ScaledFor_TwentyPercentMore_RaisesDamage`
    - `EffectScalingTests: ScaledFor_Heal_RaisesHealing`
    - `EffectScalingTests: ScaledFor_GrantStatus_LeavesItAlone`
    - `BattlePowerTests: Execute_SkillByStrongMember_DealsScaledDamage`
    - `BattlePowerTests: Execute_AttackWithSword_UsesSwordDamageNotUnarmed`
    - `BattlePowerTests: Execute_ItemByStrongMember_LandsItsOwnPower`
    - `BattlePowerTests: Execute_DominantForceOnScaledSkill_RaisesTheScaledValue`
    - `ItemCatalogValidatorTests: Validate_WeaponWithoutDamage_NamesTheField`
  - **Call sites:** `new ClassStats(` 18 (the `Attack` argument goes); `AttackBonus` 24 occurrences in 8 files (builder, `ItemCatalogValidatorTests` 6, `EquipmentChoiceTests` 3, the deleted previews' leftovers); expectations that summed a weapon's bonus onto attack (`CombatantTests.FromPartyMember_EquippedMember_UsesTotalStatsOnPartySide` and the like: count them); equipment tests that now see the builder's test weapon.
  - **Size:** about 70 Core, 30 Data and content, 8 tests (about 105), call sites about 20 beyond the one-token `ClassStats` edits: about 225.
  - **Watch (from D1's review):** only `Battle/MemberBattleStats.Of` sets the attack type; `PartyMember.TotalStats`,
    `StatsWith` and `StatsWithout` always carry the default Blunt. A page that shows the weapon's type reads it from
    there, or the type moves onto the sheet.
  - **Verify by hand:** the fighter unarmed hits a goblin for about 18 × 1.15 (minus 2 defense); with the bronze sword more; the cleric's Heal restores 15 × its Power percent. The goblin trio simulation: every class still wins every seed; record the table (the leader fights unarmed).
- **Done, with deviations:**
  - **Pages:** the pages' Weapon damage line is M1b (the split).
  - **Neutral rules:** `TestStatRules.Neutral` hits unarmed for 10, the old default test class attack, so members built without `WithStats` keep their numbers.
  - **`BaseStats`:** holds the unarmed attack and the class's defense. Five growth assertions now expect attack 10 at every level, each with a comment: four in `PartyMemberTests`, one in `GameSessionSaveTests`.
  - **Tests about what is worn:** these build members without `WithStats`, so the builder's test weapon doesn't apply. Their swords deal 15 and 18 against the unarmed 10, so their expected numbers are unchanged (`PartyMemberStatTests`, `CombatantTests`, the `BattleResistanceTests` sword).
  - **Validation:** `ItemDefinitionBuilder.WithAttackAndDefense` became `WithDefenseBonus`. `ItemCatalogValidator` also refuses damage on anything but a weapon (`Validate_ArmorWithDamage_NamesTheField`). `Validate_NegativeAttackBonus` became `Validate_WeaponWithoutDamage`. `GameRulesValidator` gains `Validate_ZeroUnarmedDamage_NamesTheField`.
  - **From the review:** `BattlePowerTests.Execute_AttackByStrongMember_DealsScaledDamage` (the sword's 30 raised by Power to 36). `EffectScaling` now multiplies in a `long`.
  - **Goblin trio simulation (1000 seeds, attack only):** every class wins 100 % in 16.5 turns. Average HP left:

    | Class | HP left |
    |---|---|
    | Fighter | 176.6 |
    | Mage | 43.6 |
    | Cleric | 114.1 |
    | Rogue | 78.1 |
    | Barbarian | 195.6 |
    | Paladin | 176.1 |
    | Monk | 114.6 |
    | Warlock | 53.6 |
    | Druid | 94.6 |

    The turns match because unarmed 18, raised by each class's Power, kills a goblin in two hits for every class.
