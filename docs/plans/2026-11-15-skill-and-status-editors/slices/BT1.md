# BT1. Core's readings and the table's class skill rows

Core + Editor, about 240 lines (about 130 of them tests). Decisions 992, 993, 997. Design: design.md, sections 1
(`StartingLevels`, damage modifier), 2.2 and 2.4. **Phase 4, after row 12:** builds on row 6 (`CombatStats.Power`,
`DamageModifierPermille`, `DamageFormula.TieredAmount`) and row 12's skills learned at a level (its LV1-LV3, ADR
0062); re-read row 12's class skill entries before starting, the names below assume its "a skill entry may name a
level, absent meaning 1".

## Goal
The balance table's first rows, headless: every class's damaging and healing skills with what one hit and all its hits
come to at three typed levels, a typed damage modifier added to the modifiers' sum, worked out by the battle's own
rule (a member of the class at that level wearing nothing); a skill the class learns above a level reads "-" at that
level. Two Core readings make the battle's internal rule readable; nothing in the battle changes.

## New types
- `Core/Battle/ClassBattleStats` (public static): a fresh member's battle stats for a class at a level, nothing worn.
  `At(ClassDefinition characterClass, int level, StatRules rules)`: `MemberSheet.Of(ClassGrowth.StatsAt(...),
  StatBonuses.None, weaponDamage: null, rules)` mapped by `MemberBattleStats.Of(sheet, rules.UnarmedDamageType)`;
  throws `ArgumentOutOfRangeException` below level 1.
- `Core/Battle/EffectAmounts` (public static): what an effect deals or heals in one hit before its target, as the
  battle scales it (ADR 0045's balance table). `PerHit(Effect effect, CombatStats user)`: `EffectScaling.ScaledFor
  (effect, user).Power` for damage, restoreHp and revive; throws `ArgumentException` for a kind that changes no HP
  (the table never asks).
- `Editor/Balance/SkillBalanceRow` (internal sealed record): one skill of one class with its amounts at the table's
  levels: `ClassName`, `SkillName`, `TierLine` ("fire_1 x2", "flat 15"), `Hits`, `IsHeal`, `AmountTexts` (three:
  "24", or "28 x 4 = 112" with hits).
- `Editor/Balance/BalanceTableViewModel` (internal sealed partial, `ObservableObject`): the table's boxes and rows.
  - `BalanceTableViewModel(ContentCatalog content)`; `FirstLevelText`, `SecondLevelText`, `ThirdLevelText`
    (`StartingLevels` 1, 10, 20; whole numbers from 1 to the experience table's `MaxLevel`),
    `ModifierPercentText` (0; a whole number above -100).
  - `ClassSkillRows` (`IReadOnlyList<SkillBalanceRow>`): each class in content order, its skills in its list's order,
    only damage, restoreHp and revive, "-" at a level below the one it is learned at; rebuilt when a box changes;
    empty while `Problem` shows.
  - `Problem` ("Level 2 must be a whole number from 1 to 20.").
  - The member's stats `with { DamageModifierPermille = typed percent x 10 }`, so Core's rule decides what the
    modifier raises (damage, not heals, decision 786).

## Changes
- `Core/Battle/MemberBattleStats`: `Of(PartyMember member)` maps through a new internal `Of(StatSheet sheet,
  DamageType attackType)`, which `ClassBattleStats` shares; no behavior change.
- `docs/map.md`: the Core Battle line names `ClassBattleStats` and `EffectAmounts`; an Editor `Balance` line.

## Tests
- `Core.Tests/Battle/EffectAmountsTests` (new; a class, a member's stats and effects built in the test):
  - `PerHit_TieredDamage_PowerTimesPerPower` (Power 15, `perPower` 2: 30).
  - `PerHit_TieredDamageWithMinusTwentyFivePercent_RaisedByTheSum` (22).
  - `PerHit_TieredHealWithAModifier_ModifierIgnored`.
  - `PerHit_FlatDamage_RaisedByDamagePercent`.
  - `PerHit_GrantStatus_Throws`.
- `Core.Tests/Battle/ClassBattleStatsTests` (new; a class built in the test):
  - `At_LevelOne_PowerIsTheBase`; `At_LevelTen_PowerGrewNineTimes`; `At_LevelZero_Throws`.
  - `At_SameClassAndLevel_EqualsARecruitedMembersStats` (a member recruited and levelled in the test, through
    `Combatant.FromPartyMember(...).Stats`).
- `Editor.Tests/Balance/BalanceTableViewModelTests` (new; classes and skills with `with` over the test world):
  - `ClassSkillRows_TieredSkill_AmountsAtTheThreeLevels`.
  - `ClassSkillRows_FourHits_ShowsPerHitAndTotal`.
  - `ClassSkillRows_StatusOnlySkill_NotListed`.
  - `ClassSkillRows_SkillLearnedAtFive_DashAtLevelOne`.
  - `ModifierPercentText_MinusTwentyFive_LowersDamageNotHeals`.
  - `Problem_LevelPastTheMax_SaysSoAndEmptiesTheRows`.

## Verify by hand
None: the page comes in BT3.
