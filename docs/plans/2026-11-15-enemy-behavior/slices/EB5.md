# EB5. An enemy's lists

Editor, about 170 lines (about 90 of code, 8 tests at about 10 lines). Needs EB4. Design: design.md, sections 1 (a new
phase's threshold), 2.3, 4.2.

## Goal
All of one enemy's rules as edited: the list from the start, then one list per phase in content's order; phases added
after the last and removed; each list's Data field kept right ("phases[1].tactics") when one goes; the "Uses:" line;
the first problem; the enemy's `Tactics` and `Phases` built back.

## New types (Editor, `Battle/`)
- `EnemyTacticsViewModel` (internal sealed partial, `ObservableObject`): an enemy's lists as edited, phases added and
  removed.
  - `public static EnemyTacticsViewModel From(EnemyDefinition enemy)`; `public static EnemyTacticsViewModel
    CopyOf(EnemyTacticsViewModel tactics)` (every list through `TacticListViewModel.CopyOf`).
  - `public ObservableCollection<TacticListViewModel> Lists` (the start list first); observable
    `TacticListViewModel SelectedList` (the start list on opening).
  - Commands: `AddPhase` (after the last, threshold `NewThreshold`: 50 for the first phase, else half the last's typed
    threshold rounded down, at least 1, or 50 while the last's isn't a number; chooses it), `RemovePhase` (the chosen
    phase; greyed on the start list; chooses the list before it).
  - `public string SkillsUsedLine`: "Uses: cleave, field_dressing" (the distinct skill ids the rows name, in list
    order), or "Uses: only the attack".
  - `public string? Problem`: the first list's problem, in list order.
  - `public IReadOnlyList<EnemyAction> ToTactics()`; `public IReadOnlyList<EnemyPhase> ToPhases()`.
  - `public event EventHandler? Changed`: a list's `RulesChanged`, or a phase added or removed; its `SkillsUsedLine` is
    raised with it.

## Changes
- `docs/map.md`, Editor `Battle` row: `EnemyTacticsViewModel`.

## Tests
New `Editor.Tests/Battle/EnemyTacticsViewModelTests` (enemies built in the test with two phases):
- `From_EnemyWithTwoPhases_ListsStartThenPhasesInOrder`
- `AddPhase_AfterAPhaseAt40_StartsAt20AndIsChosen`
- `AddPhase_NoPhaseYet_StartsAt50`
- `RemovePhase_FirstOfTwo_RenamesTheOthersField` ("phases[0].tactics")
- `RemovePhaseCommand_StartListChosen_CannotExecute`
- `SkillsUsedLine_SkillInTwoLists_NamedOnce`
- `ToPhases_Typed_BuildsEveryPhase`
- `Changed_RuleAddedInAPhase_IsRaised`

## Verify by hand
None: nothing shows yet (EB7).
- **Note from EB4:** `ForPhase` and `NewPhase` start a list's `Field` as "phases[0].tactics": set every phase's from its
  place after `From`, `AddPhase` and `RemovePhase`. A list raises `RulesChanged`, not its own `Problem`, on a rule typed.
