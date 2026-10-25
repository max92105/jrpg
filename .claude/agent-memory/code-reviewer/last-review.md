---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/RG2.md, slice RG2 (quick skills keep the turn; TurnFlow extracted from BattleState)
Round: 1 (verdict: Ready to commit)

- Suggestion: TurnFlow.AnyoneStandsOn re-implements BattleState.LivingOn's liveness filter (two places decide "side still standing"); open
- Suggestion: Execute_QuickSkill_NoUpkeepRuns checks only per-turn gains, not that statuses don't count down; open
- Follow-up: constructor sets Phase from TurnFlow.PhaseOf(CurrentActor) without the stun skip, so a battle opening on a stunned actor would give it the turn (predates diff)
