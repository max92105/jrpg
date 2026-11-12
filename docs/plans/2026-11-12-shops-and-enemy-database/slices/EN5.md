# EN5. Phases: actions taken below a share of HP

**Later: not in the approved plan** (README, "Later"); a draft kept with EN4.

Editor, about 160 lines (80 of them tests). ADR 0034, decisions 404, 405. Builds on EN4.

## Goal
A boss's phases as a list, highest threshold first: each a "below HP percent" (1 to 99) and its own tactics (EN4's list). Add (50 %, empty tactics), remove, and the list kept in descending order as Data requires: a phase whose threshold breaks the order shows Data's message, the window doesn't re-sort behind the developer's back.

## New types
- `Editor/Battle/EnemyPhaseViewModel` (internal sealed partial, `ObservableObject`): one phase as edited: `BelowHpPercent`, `Tactics` (`TacticListViewModel`); `From(EnemyPhase)`, `ToPhase()`.

## Changes
- `Editor/Battle/EnemyViewModel`: `Phases` (an `ObservableCollection<EnemyPhaseViewModel>`), `SelectedPhase`, Add phase, Remove phase, Move up, Move down; changes in any phase reach `Problem`; `ToEnemy` writes them.

## Tests
- `EnemyPhaseViewModelTests` (new): `From_GoblinChiefPhase_ReadsThresholdAndTactics`; `ToPhase_Unchanged_EqualsTheRecord`.
- `EnemyPhasesTests` (new sibling of `EnemyViewModelTests`): `AddPhase_Defaults_FiftyPercentNoTactics`; `Problem_ThresholdsNotDescending_IsDataMessage`; `Problem_PhaseTacticEditedBroken_Refreshes` (an edit two levels down reaches the enemy's problem); `ToEnemy_PhaseRemoved_WritesTheRest`.

## Verify by hand
None: the window is EN8.
