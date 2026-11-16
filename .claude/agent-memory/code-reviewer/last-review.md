---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB11 (FoeHpBelow, EveryNthTurn: TacticCondition.Turns, TacticConditionNumber.Turns,
TacticMemory.TurnsActed/NoteTurnActed, validator LeftOutProblem + MostTurnsBetween, AbsentWhenZero "turns", editor
choice phrases + TacticRuleRow ConditionOf/NumberName). 2026-11-16. Round 1. Verdict: Ready to commit (no Critical).

- Warning (open): EnemyEveryNthTurnTests - turns 1, 3, 6 also pass with `TurnsActed + 1 >= Turns` ("from the 3rd turn
  on"); add ChooseCommand_EveryThirdTurnOnItsFourthTurn_IsNotEligible (DefendFor rounds: 3, Tactics(0), AttackCommand).
- Warning (open): TacticRuleRow.WhenWords - EveryNthTurn reads "When every Nth turn of its own 3 turns: ..."; word the
  Turns case (e.g. "Every 3 turns of its own") or relabel, with a Sentence test in TacticRuleRowSentenceTests.
- Suggestion (open, optional): TacticRuleRowSentenceTests has no FoeHpBelow/EveryNthTurn sentence pinned (covered by
  the Warning's test for the turns one).
- EB10 carried suggestions dropped (optional, not re-raised).
- Checked, no finding: quick skill counting twice is unreachable today (enemies hold no resources, Combatant.FromEnemy;
  Data requires a cost on isQuick, SkillCatalogValidator:23), a readied extra turn counts as a turn acted (decision 847's
  "turns acted"); stun skip arrives in the member's Execute so NoteOwnTurn isn't called for the stunned enemy; validator
  bounds 1/11 refused, 2/10 accepted, percent 1/99 accepted; Turns 0 % would throw in Core but Data refuses it.
