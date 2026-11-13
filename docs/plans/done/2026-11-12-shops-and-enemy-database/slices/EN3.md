# EN3. Resistances by damage type, gold, drop table and boss

Editor, about 170 lines (85 of them tests). Design: key decision 3. Builds on EN2.

## Goal
An enemy's resistances as one row per damage type (whole percents, generated from `DamageType`, so a type added to Core appears), its gold range (none, or a min and a max), its drop table (none or one of the draft's), and the boss box.

## New types
- `Editor/Battle/ResistanceRow` (internal sealed partial, `ObservableObject`): one damage type's resistance as typed: `Type`, `Percent`; `PercentChanged` reaches the enemy's problem.

## Changes
- `Editor/Battle/EnemyViewModel`: `Resistances` (one `ResistanceRow` per `Enum.GetValues<DamageType>()`, from `Resistances.Of` in whole percents), `HasGold`, `GoldMin`, `GoldMax`, `DropTableId` (null for none), `DropTableChoices` (given by the catalog view model: "(none)" then the draft's table ids), `IsBoss`; `ToEnemy` builds `Resistances.From(type => permille of the row)`, `Gold` only with `HasGold`.
- `Editor/Battle/EnemyCatalogViewModel`: passes the drop table ids.

## Tests
- `EnemyResistanceTests` (new sibling of `EnemyViewModelTests`): `Resistances_Goblin_OneRowPerTypeFireMinus25`; `Resistances_RowCount_EqualsDamageTypes`; `ToEnemy_RowEdited_WritesPermille`; `Problem_ResistancePastCap_IsDataMessage`.
- `EnemyRewardTests` (new): `HasGold_Dummy_IsFalse`; `ToEnemy_GoldTurnedOff_HasNone`; `ToEnemy_DropTableNone_IsNull`; `Problem_GoldMaxBelowMin_IsDataMessage`; `IsBoss_GoblinChief_IsTrue`.

## Verify by hand
None: the window is EN7.

- **Done, with deviations:** the rows sit in the partial `EnemyViewModel.Resistances`, the gold and drop table in `.Rewards`, the boss box with the plain fields (the review found the first cut named all of it rewards); a row's percent is kept as typed (`PercentText`), a row's change reaching the list as the enemy's own `Resistances` notice; the drop table box lists `EnemyCatalogViewModel.DropTableChoices` (the empty one for none, as the event dialog's gates), bound like the damage types; the gold boxes only count while the enemy drops gold; Duplicate copies the rows and rewards as typed.
