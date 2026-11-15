# EB4. One list of rules

Editor, about 200 lines (about 110 of code, 9 tests at about 10 lines). Needs EB3. Design: design.md, sections 2.3,
4.2.

## Goal
One list of rules, the enemy's from the start or a phase's: rules added after the chosen one, removed, moved up and
down, each numbered as Data numbers it; a phase's threshold box; the list's first problem named by Data's field; a
`RulesChanged` event for typed and listed changes only.

## New types (Editor, `Battle/`)
- `TacticListViewModel` (internal sealed partial, `ObservableObject`): one list of rules as edited, the enemy's from the
  start or a phase's with its threshold.
  - `public static TacticListViewModel FromStart(IReadOnlyList<EnemyAction> tactics)`; `public static
    TacticListViewModel ForPhase(EnemyPhase phase)`; `public static TacticListViewModel NewPhase(int threshold)` (one
    new rule in it, since Data refuses an empty phase); `public static TacticListViewModel CopyOf(TacticListViewModel
    list)` (rows through `TacticRuleRow.CopyOf`).
  - `public bool IsPhase { get; }`; observable `string ThresholdText` (a phase's, "" from the start) and `string Field`
    ("tactics", or "phases[1].tactics", set by the owner, EB5).
  - `public ObservableCollection<TacticRuleRow> Rules`; observable `TacticRuleRow? SelectedRule`.
  - `public string Label`: "From the start", or "Below 50 % HP" (the threshold as typed).
  - `public string? Problem`: the threshold's (`TypedNumber.ProblemOf("Threshold", ...)`, a phase only), then the first
    row's as "phases[0].tactics[2]: Weight must be a whole number (was 'x').".
  - `public IReadOnlyList<EnemyAction> ToActions()`; `public EnemyPhase ToPhase()` (throws for the start list or on a
    problem).
  - Commands: `AddRule` (after the chosen rule, or last; chooses it), `RemoveRule`, `MoveRuleUp`, `MoveRuleDown`
    (through `Events/ListOrder`, each greyed without a chosen rule).
  - `public event EventHandler? RulesChanged`: a row's typed box (`When`, `NumberText`, `SkillId`, `On`, `WeightText`),
    the threshold, or a row added, removed or moved; never a row's `Sentence`, `Index` or `Problem` notice. Rows
    followed through `Catalogs/ListItemWatch`, renumbered on every list change as `Loot/DropTableViewModel` does.

## Changes
- `docs/map.md`, Editor `Battle` row: `TacticListViewModel`.

## Tests
New `Editor.Tests/Battle/TacticListViewModelTests` (actions and phases built in the test):
- `ForPhase_Phase_ReadsTheThresholdAndRules`
- `AddRule_WithAChosenRule_AddsAfterItAndChoosesIt`
- `MoveRuleDown_FirstOfThree_RenumbersBoth` (the moved rows' `Index`)
- `RemoveRule_Chosen_ChoosesTheNext`
- `Problem_RowInAPhase_NamesThePhaseField`
- `Problem_ThresholdNotANumber_NamesThreshold`
- `ToPhase_Typed_BuildsThePhase` (a threshold other than the fixture's)
- `RulesChanged_WeightTyped_IsRaised`
- `RulesChanged_SentenceNotice_IsNotRaised` (a row's `Index` set again raises its own notices, never `RulesChanged`)

## Verify by hand
None: nothing shows yet (EB7).
