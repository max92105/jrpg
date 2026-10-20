---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/C2.md, C2 (BattleState.TargetChoiceForAttack, private ChoiceFor(Targeting) shared with ThrowIfIllegalTargets, AttackTargeting const)
Round: 1 (verdict: Ready to commit; verify all pass, 1190 tests)

- open (Suggestion): ThrowIfIllegalTargets keeps an `actor` parameter used only for the message while ChoiceFor reads CurrentActor; drop the parameter or pass actor into ChoiceFor so one actor is used.
- open (Suggestion): comment "An attack hits one living enemy." on AttackTargeting restates the value; say why it's a const (shared by offer and Execute) or remove.
- follow-up: none.
