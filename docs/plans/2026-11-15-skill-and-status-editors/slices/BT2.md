# BT2. Enemies' skills, classes and enemies, and the tier sheet rows

Editor, about 180 lines (about 90 of them tests). Decisions 992, 993. Design: design.md, sections 1
(`StartingPowers`) and 2.4. Builds on BT1 (phase 4, after row 12). Split at its start (estimate past 320 lines): the tier
sheet rows and the Power boxes (`TierBalanceRow`, the Power texts, `TierRows`, their tests) are [BT2b](BT2b.md).

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
- **Note from BT1:** BT1 was split; BT2 builds on BT1c's `Balance/BalanceTableViewModel` (its boxes' line is
  `BoxesProblem`, never `Problem`: BT3's `IEditedCatalog.Problem` stays null). `ClassBalanceRow` reads
  `Party/ClassSheet.At(class, level, content.Rules.Stats)`'s `MaxHp` and `Primaries.Power` (decision 948), never its
  own `MemberSheet.Of(ClassGrowth.StatsAt(...), ...)`; classes and enemies in id order, as `Skills/SkillUsers` lists
  them (the catalog keeps no file order). An enemy's skills are its `Tactics` then each phase's `Tactics` by
  `SkillId`, the walk `EnemyDefinition.NamesSkill` makes (row 8), each skill once; its stats
  `Combatant.FromEnemy(enemy).Stats` with the typed modifier added to `DamageModifierPermille` as BT1b adds it. A
  tier row is `EffectAmounts.PerHit` of an effect naming the tier over `new CombatStats(0) { Power = p,
  DamageModifierPermille = typed x 10 }`, a heal ignoring the modifier there. `DamageFormula.MaxPower` is internal to
  Core (decision 1001's finding), so the Power boxes can't read it from the Editor: a whole number from 0 to a bound
  of its own or one Core makes public with a test, BT2's choice to raise. Mind the 300-line cap:
  `BalanceTableViewModel` with BT1c's boxes and BT2's rows passes it; split its rows' building into a partial or a
  builder.
- **Note from BT1b:** the class rows' building is `Balance/ClassSkillBalanceRows.Of(content, levels,
  modifierPermille)`, a static builder beside the view model; the enemy and tier rows take the same shape
  (`EnemySkillBalanceRows`, `TierBalanceRows`), so `BalanceTableViewModel` only reads the boxes (its properties keep
  their names: `EnemySkillRows`, `TierRows`). An enemy skill row reuses `Balance/SkillLines.TierLine` and
  `SkillLines.AmountText(effect, Combatant.FromEnemy(enemy).Stats, modifierPermille)`, which already adds the
  modifier to the sum: no addition of BT2's own. Filter skills with Core's `EffectAmounts.ChangesHp`, as the class
  rows do. `Editor.Tests/Balance/BalanceFixtures` holds the tests' classes and skills; add the enemies there.
- **Done, with deviations:** split at its start (estimate past 320 lines): the tier rows and the Power boxes are BT2b.
  `EnemySkillBalanceRows` (41 lines) and its row, named `EnemySkillBalanceRow` (20) beside `SkillBalanceRow`, with
  `IsHeal` for BT3's muted heals; `ClassBalanceRows` (28), `ClassBalanceRow` (6); `EnemyBalanceRows` (25) and
  `EnemyBalanceRow` (11) read `Combatant.FromEnemy`. `BalanceTableViewModel` (127) reads the boxes once through
  `ReadBoxes()`, each box-read list empty while `BoxesProblem` shows; `EnemyRows`, reading no box, built once. At
  review, the third copy of the tactics walk became Core's `EnemyDefinition.NamedSkillIds` (97 lines; tactics then
  phases, each id once, no attack), read by `NamesSkill`, Data's `EnemyCatalogValidator.FindPowerProblem` (87) and the
  enemy skill rows; `EnemyDefinitionTests` (41, 3 cases); simulations identical.
  Editor tests (15 cases): `EnemySkillBalanceRowsTests` 83, `ClassBalanceRowsTests` 29, `EnemyBalanceRowsTests` 25,
  `BalanceTableViewModelTests` 207 (+3 cases, the refusals emptying every list), `BalanceFixtures` 120 (an imp, a bat).
