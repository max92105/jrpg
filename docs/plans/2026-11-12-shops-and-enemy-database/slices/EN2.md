# EN2. The Enemies window's rules: the list and an enemy's numbers

Editor, about 220 lines (110 of them tests). Design: key decision 1, "Tunables" (the new enemy). Decisions 581, 582, 585; design key decision 4. Builds on EN1.

## Goal
The Enemies window's state, headless: every enemy in id order, new ones after; Add (from the tunable defaults), Duplicate (a copy of the chosen one, id empty to type) and Remove (only while new); an enemy's id, name and numbers (max HP, damage, damage type, accuracy, dodge, crit chance, crowd control resistance, experience); the problem line; the enemies it gives the draft. Fields EN3 and EN6 add, and the tactics and phases no form edits yet (Later), are carried through untouched from the record (`ToEnemy` is `record with { ... }` over the original), so the window never wipes them; a duplicate copies them too.

## New types
- `Editor/Battle/EnemyViewModel` (internal sealed partial, `ObservableObject`): one enemy as edited: `Id` (typed while `IsNew`), `Name`, `MaxHp`, `Damage`, `DamageType` (from `DamageTypes`, `Enum.GetValues<DamageType>()`), `Accuracy`, `Dodge`, `CritChance`, `CrowdControlResistance`, `ExperienceReward`; `From(EnemyDefinition)`, `New()`, `DuplicateOf(EnemyViewModel)`; `Problem` (an empty new id or one breaking `ObjectIds`' rule); `ToEnemy()` (the original record `with` the edited fields; a new one from the tunables).
- `Editor/Battle/EnemyCatalogViewModel` (internal sealed partial, `ObservableObject`): `Enemies`, `SelectedEnemy`, Add, Duplicate, Remove (greyed for a saved enemy), `Problem` (an enemy's own problem by id or position, two new enemies sharing an id, then `CatalogProblems.OfEnemies` on the whole list), `ToEnemies()`.
- `Editor/Battle/EnemyEditingViewModel` (internal sealed): `Start()` / `Apply(edited)` (`CatalogDraft.SetEnemies`).

## Changes
None outside the new folder.

## Tests
- `EnemyViewModelTests` (new): `From_Goblin_ReadsItsNumbers`; `ToEnemy_Unchanged_EqualsTheRecord` (tactics, phases, art and gold kept); `ToEnemy_MaxHpEdited_ChangesOnlyIt`; `New_Defaults_PassDataChecks`; `DuplicateOf_Goblin_CopiesNumbersWithEmptyId`; `DuplicateOf_GoblinChief_CopiesTacticsAndPhases`; `Problem_NewWithoutId_AsksForIt`.
- `EnemyCatalogViewModelTests` (new): `Enemies_Draft_InIdOrder`; `Duplicate_Chosen_AddsAfterAndChoosesIt`; `Remove_SavedEnemy_IsGreyed`; `Problem_DodgePastCap_IsDataMessage`; `Problem_TwoNewSameId_SaysSo`.
- `EnemyEditingViewModelTests` (new): `Apply_DuplicatedEnemy_IsOneUndoStep`; `Apply_Unchanged_RecordsNothing`.

## Verify by hand
None: the window is EN7.
