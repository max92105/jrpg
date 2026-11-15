# EB8. Priority rules

Core + Data + Editor, about 160 lines (Core about 20 and 5 tests; Data a filter line and 2 tests; Editor about 25 and
2 tests). Needs EB7. Design: design.md, section 2.5 (the pick); decision 841.

## Goal
A rule may be marked priority: when one or more eligible rules are, the first of them in the list is chosen with no
draw; otherwise the weighted draw as today. This is what makes "when critically hit - skill X" certain once reactions
exist (EB9).

## Changes
- Core `Battle/EnemyAction`: `public bool IsPriority { get; init; }` ("chosen before any draw while eligible, the first
  in the list among several, decision 841").
- Core `Battle/EnemyTactics.ChooseCommand`: `eligible.FirstOrDefault(action => action.IsPriority)` before the weighted
  pick (a private `Pick(eligible)` keeps the method short); the class summary adds the rule.
- Data `Content/AbsentWhenFalseFilter`: `(typeof(EnemyAction), "isPriority")` (decision 848).
- Editor `Battle/TacticRuleRow`: observable `bool IsPriority`, copied by `From`, `CopyOf`, `ToAction`; the sentence ends
  ", priority" when set; `TacticListViewModel`'s `RulesChanged` relays it (its typed-box list gains the name).
- Editor `Battle/EnemyTacticsView.xaml`: a "Priority: chosen before any draw" check box in the rule's boxes.
- `docs/map.md`, Core `Battle` row (`EnemyTactics`: priority first).

## Tests
- New `Core.Tests/Battle/EnemyTacticPriorityTests` (enemies built with `EnemyDefinitionBuilder`, `FixedSequenceRandom`
  with no draws left for the pick, so a draw fails the test):
  - `ChooseCommand_EligiblePriorityRule_ChosenWithoutADraw`
  - `ChooseCommand_TwoEligiblePriorityRules_TheFirstInTheListWins` (the first has weight 1, the second weight 9, so a
    weighted or last-wins pick fails)
  - `ChooseCommand_PriorityRuleNotEligible_WeightedDrawAmongTheRest` (its condition fails; one draw)
  - `ChooseCommand_PriorityRuleInAPhase_ReadFromThePhase`
  - `ChooseCommand_PriorityRuleWithItsTargetMissing_NotChosen` (an `On` that finds no one)
- `Data.Tests/Content/ContentWriterEnemyTacticsTests` (EB2's file): `WriteEnemies_PriorityRule_WritesIsPriority`,
  `WriteEnemies_RuleWithoutPriority_LeavesItOut`.
- `Editor.Tests/Battle/TacticRuleRowTests` (EB3's file): `ToAction_PriorityChecked_IsPriority`,
  `Sentence_Priority_EndsWithPriority`.
- EB1's replay test runs unchanged.

## Verify by hand
1. Editor: Database > Enemies, `goblin`, Tactics: Add a rule, Use "Cleave", check Priority, apply, Save all. Run the
   game, New game, walk the forest until goblins attack: every goblin turn is Cleave (the plaque reads "Cleave"). Back
   in the editor, Ctrl+Z and Save all; `git diff content/enemies.json` is empty.
