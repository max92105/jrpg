---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/ST1.md, slice ST1 (statuses in Core)
Round: 1 (verdict: Ready to commit; verify all pass, 1847 tests)

- Warning, open: TurnUpkeep.CountDownStatuses's StatusEnded on run-out has no battle-level test (only StatusSet.CountDown's return is tested).
- Suggestion, open: Execute_TargetFalls_StatusesCleared uses Contains; pin CombatantDefeated -> StatusEnded order.
- Suggestion, open: EffectValidator accepts power > 0 on a grantStatus effect, which Effect's doc says is unused.
- Follow-up: ActionPlayback.Read throws on StatusApplied/StatusEnded (known gap, FT1).
