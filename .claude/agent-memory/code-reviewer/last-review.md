---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/BB1.md, slice BB1 (barbarian ferocity per hit taken; Taunt draws enemy attacks)
Round: 1 (verdict: Ready to commit; verify all pass, 1872 tests)

- Suggestion, open: no test pins the event order of a felling hit on a barbarian (EffectApplied, ResourceChanged, CombatantDefeated), the rule the slice note records.
- Suggestion, open: Execute_SelfTaunt_LastsTwoOwnTurnsAfterCast could also check that an enemy turn between the hero's turns targets the taunter (end to end through EnemyTactics).
- Follow-up: question 209 placeholder: Taunt on an enemy combatant has no effect; revisit when enemies get statuses.
