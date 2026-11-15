# BT2. Enemies' skills, classes and enemies, and the tier sheet rows

Editor, about 180 lines (about 90 of them tests). Decisions 992, 993. Design: design.md, sections 1
(`StartingPowers`) and 2.4. Builds on BT1 (phase 4, after row 12).

## Goal
The rest of the table's rows, headless: each enemy's damaging and healing skills at its own power; each class's max
HP and Power at the three levels; each enemy's HP, power, attack damage and type; and the developer's tier sheet, each
`effects.json` record at three typed Powers with the typed modifier (Power x `perPower` x (1 + modifiers), decision
704), through the same Core reading as the skills.

## New types
- `Editor/Balance/EnemySkillRow` (internal sealed record): one skill an enemy's tactics or phases name, with its power
  and amount: `EnemyName`, `Power`, `SkillName`, `TierLine`, `Hits`, `AmountText`.
- `Editor/Balance/ClassBalanceRow` (internal sealed record): one class's max HP and Power at the three levels
  (`MemberSheet.Of(ClassGrowth.StatsAt(...), StatBonuses.None, null, rules)`'s `MaxHp` and `Primaries.Power`), as
  "HP / Power" texts.
- `Editor/Balance/EnemyBalanceRow` (internal sealed record): one enemy's `MaxHp`, `Power`, attack damage and type.
- `Editor/Balance/TierBalanceRow` (internal sealed record): one tier's id, kind line ("damage, fire") and `perPower`,
  and its amounts at the three Powers: `EffectAmounts.PerHit` of an effect naming the tier, for `CombatStats` with
  that `Power` and the typed modifier, so a heal ignores the modifier as the battle does.

## Changes
- `Editor/Balance/BalanceTableViewModel`: `EnemySkillRows` (enemies in id order, each skill once in the order its
  tactics then phases name it, through `Combatant.FromEnemy(enemy).Stats` with the modifier), `ClassRows`,
  `EnemyRows`, `TierRows`; `FirstPowerText`, `SecondPowerText`, `ThirdPowerText` (`StartingPowers` 1, 10, 50; whole
  numbers from 0 to `DamageFormula.MaxPower`); `Problem` covers the Power boxes; every list rebuilt on a box change.

## Tests
- `Editor.Tests/Balance/BalanceTableViewModelTests` (BT1):
  - `EnemySkillRows_TieredSkill_ItsPowerTimesPerPower`.
  - `EnemySkillRows_SkillInTacticsAndPhases_ListedOnce`.
  - `ClassRows_LevelTen_HpAndPowerOfTheClassAtTen`.
  - `EnemyRows_Enemy_ReadsHpPowerAndAttack`.
  - `TierRows_SheetExample_PowerOneLowWithFourHundredSeventyThreePercent` (Power 1, `perPower` 2, modifier 473 %:
    11, the developer's sheet, decision 704).
  - `TierRows_HealRecordWithAModifier_ModifierIgnored`.
  - `Problem_PowerNotANumber_SaysSo`.

## Verify by hand
None: the page comes in BT3.
