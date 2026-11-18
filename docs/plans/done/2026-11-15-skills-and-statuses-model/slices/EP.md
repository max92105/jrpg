# EP. The Enemies window edits an enemy's Power

Editor, about 100 lines (about 50 of them tests). Needs T3. Design: README, decision 794.

## Goal
Database > Enemies..., Stats tab: a Power box under Damage type. It shows the enemy's power (0 when the file has none),
a typed value that isn't a whole number names "Power" in the problem line and greys OK, Duplicate copies it, and OK
writes it. Data's own refusal (power 0 with a tiered skill, T3) reaches the problem line through
`CatalogProblems.OfEnemies` as every Data message does.

## Changes
- `Battle/EnemyViewModel` (188 lines, about 196 after):
  - `[ObservableProperty] [NotifyPropertyChangedFor(nameof(Problem))] public partial string PowerText`, read from
    `original.Power` in the constructor and copied by `DuplicateOf`.
  - `ToEnemy`: `Power = TypedNumber.Parse(PowerText)!.Value`.
  - `FirstNumberProblem`: `TypedNumber.ProblemOf("Power", PowerText)` after Damage, in the form's order.
  - `_blank` keeps `Power` at its default 0.
- `Battle/EnemyCatalogWindow.xaml` (205 lines): the Power label and `NumberBox` row under Damage type, bound to
  `PowerText`; the tab's existing grid rows shift by one.

## Tests
- `Battle/EnemyViewModelTests` (165 lines, about 210 after; enemies built in the test):
  - `From_EnemyWithPower_ShowsItsPower` (power 5: "5").
  - `ToEnemy_TypedPower_SetsPower` (typed "7": 7, not the original 5).
  - `Problem_PowerNotANumber_NamesPower`.
  - `DuplicateOf_TypedPower_CopiesIt` (typed, not the record's).
- `Battle/EnemyCatalogViewModelTests` (160 lines): `Problem_TieredSkillWithPowerZero_ShowsDataMessage` (a draft with a
  skill naming a tiered effect built in the test and an enemy naming it in its tactics; the line holds T3's message);
  `Problem_PowerAboveTenThousand_ShowsDataMessage` (decision 818's bound reaches the line).

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line (the Enemies window is in its default list).
2. Run the editor, Database > Enemies..., choose `goblin`, Stats tab: Power shows 0.
3. Type `x` in Power: the problem line says Power isn't a whole number and OK is greyed. Type `3`, OK, File > Save all:
   `content/enemies.json` gains `"power": 3` after `"damageType"` for the goblin. Undo (Ctrl+Z), Save all: the line
   is gone and `git diff content/enemies.json` is empty.

- **Done:** as planned (the Stats tab is stacked panels, so the row went in under Damage type with no rows to shift).
  The implementer ran step 1: the Enemies window opened with no `ERROR`, its capture showing Power 0 under Damage
  type. Steps 2 and 3 are the developer's.
