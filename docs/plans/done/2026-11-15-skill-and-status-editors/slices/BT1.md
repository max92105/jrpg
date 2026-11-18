# BT1. Core's readings for the balance table

Core, about 150 lines (about 100 of them tests). Decisions 993, 997. Design: design.md, section 2.2. **Phase 4,
after row 12:** builds on row 6 (`CombatStats.Power`, `DamageModifierPermille`, `DamageFormula.TieredAmount`) and row
12's `Party/ClassSheet` (decision 948). Split at its start (estimate past 320 lines): the table's class skill rows,
the Editor half, are [BT1b](BT1b.md).

## Goal
Two Core readings make the battle's internal rules readable for the balance table, headless: a member of a class at a
level wearing nothing, as the battle builds its stats, and what an effect deals or heals in one hit before its target,
as the battle scales it. Nothing in the battle changes.

## New types
- `Core/Battle/ClassBattleStats` (public static): a fresh member's battle stats for a class at a level, nothing worn.
  `At(ClassDefinition characterClass, int level, StatRules rules)`: `ClassSheet.At(...)` mapped by
  `MemberBattleStats.Of(sheet, rules.UnarmedDamageType)`; throws `ArgumentOutOfRangeException` below level 1.
- `Core/Battle/EffectAmounts` (public static): what an effect deals or heals in one hit before its target, as the
  battle scales it (ADR 0045's balance table). `PerHit(Effect effect, CombatStats user)`: `EffectScaling.ScaledFor
  (effect, user).Power` for damage, restoreHp and revive; throws `ArgumentException` for a kind that changes no HP
  (the table never asks).

## Changes
- `Core/Battle/MemberBattleStats`: `Of(PartyMember member)` maps through a new internal `Of(StatSheet sheet,
  DamageType attackType)`, which `ClassBattleStats` shares; no behavior change.
- `docs/map.md`: the Core Battle line names `ClassBattleStats` and `EffectAmounts`.

## Tests
- `Core.Tests/Battle/EffectAmountsTests` (new; a member's stats and effects built in the test):
  - `PerHit_TieredDamage_PowerTimesPerPower` (Power 15, `perPower` 2: 30).
  - `PerHit_TieredDamageWithMinusTwentyFivePercent_ScaledByTheSum` (22).
  - `PerHit_TieredHealWithAModifier_ModifierIgnored`.
  - `PerHit_FlatDamage_RaisedByDamagePercent`.
  - `PerHit_FlatReviveWithAModifier_RaisedByDamagePercentOnly`.
  - `PerHit_KindChangingNoHp_Throws` (grantStatus, readyExtraTurn, rest).
- `Core.Tests/Battle/ClassBattleStatsTests` (new; a class built in the test):
  - `At_LevelOne_PowerIsTheBase`; `At_LevelTen_PowerGrewNineTimes`; `At_LevelZero_Throws`.
  - `At_SameClassAndLevel_EqualsARecruitedMembersStats` (a member recruited and levelled in the test, through
    `Combatant.FromPartyMember(...).Stats`, under rules whose damage percent and unarmed type differ from
    `CombatStats`' defaults).

## Verify by hand
None: the page comes in BT3.
- **Note from FX2:** Core's `TieredEffect.AmountAt(power)` (decision 1001) already reads a tier's one hit at a
  Power, before modifiers; the tier sheet (BT2) builds on it.
- **Note from CL5:** BT1 hadn't landed when CL5 built Core's `Party/ClassSheet.At(characterClass, level, rules)`, the
  sheet of a member of the class at the level wearing nothing (decision 948). `ClassBattleStats.At` maps
  `ClassSheet.At(...)` by `MemberBattleStats.Of` from birth, never its own `MemberSheet.Of(ClassGrowth.StatsAt(...))`.
- **Done, with deviations:** split at its start (estimate past 320 lines): the Editor half is BT1b, with notes from
  BT1 in BT2 and BT3 for what rows 8, 9, 11 and 12 landed. Core: `ClassBattleStats` (15 lines), `EffectAmounts`
  (21), `MemberBattleStats` (40, its mapping shared); tests 107 lines (`ClassBattleStatsTests` 46,
  `EffectAmountsTests` 61). `EffectAmountsTests` gained a revive test and a throw per kind changing no HP; the
  -25 % test is named `..._ScaledByTheSum`. Simulations unchanged.
