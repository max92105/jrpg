# EN4. Tactics: weighted actions with their conditions

**Later: not in the approved plan** (README, "Later"); a draft kept, its decision numbers never given.

Editor, about 210 lines (105 of them tests). Design: key decisions 3 and 4. ADR 0034, decision 402. Builds on EN2.

## Goal
An enemy's tactics as a list: each action is the basic attack or a skill (the draft's skills, by name), a weight (from 1), and a condition: "always" or one of `TacticConditionKind` (generated from the enum), with a percent box only for the kinds that read one. Add, remove, move up and down. Data's message names the action by position when a rule fails (an unknown skill, a weight of 0).

## New types
- `Editor/Battle/EnemyActionViewModel` (internal sealed partial, `ObservableObject`): one action as edited: `SkillId` (null for the attack), `Weight`, `ConditionKind` (null for always), `Percent`, `ShowsPercent` (the kinds that read a percent: read from `Core/Battle/TacticConditions` when writing the slice, and pinned by a test per kind); `From(EnemyAction)`, `ToAction()`.
- `Editor/Battle/TacticListViewModel` (internal sealed partial, `ObservableObject`): an ordered list of actions with `SelectedAction`, Add (the attack, weight 1), Remove, Move up, Move down (`Events/ListOrder`), `ToActions()`; used by the enemy now and by each phase in EN5.
- `Editor/Battle/SkillChoice` (internal sealed record): one entry of the action's skill box: `SkillId?` and `Label` ("Attack", or the skill's name and id).

## Changes
- `Editor/Battle/EnemyViewModel`: `Tactics` (`TacticListViewModel`); its changes reach `Problem`; `ToEnemy` writes them.
- `Editor/Battle/EnemyCatalogViewModel`: the skill choices ("Attack" first, then the draft's skills by name).

## Tests
- `EnemyActionViewModelTests` (new): `From_AttackAlways_HasNoSkillNoCondition`; `ShowsPercent_EachKind_MatchesWhatTheConditionReads` (theory over `TacticConditionKind`); `ToAction_ConditionAlways_WritesNoWhen`; `ToAction_PercentKind_WritesPercent`.
- `TacticListViewModelTests` (new): `Add_EmptyList_AddsAttackWeightOne`; `MoveDown_First_SwapsWithSecond`; `ToActions_Order_IsTheListsOrder`.
- `EnemyViewModelTests`: `Problem_TacticWeightZero_IsDataMessage`; `ToEnemy_TacticsEdited_WritesThem`; `ConditionKinds_AreEveryEnumValue` (the box's list is generated).

## Verify by hand
None: the window is EN8.
