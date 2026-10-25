---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/MO1.md, slice MO1 (monk multi-hit, chakra per hit landed)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical: ResourceTriggers.cs:27-31 OnHitLanded inserted between OnCombatantFallen's summary and its method; OnHitLanded has two summaries, OnCombatantFallen none; open
- Suggestion: no test of a multi-hit damage effect with a status (grant once per target after the hits); open
- Suggestion: ActionPlayback.AddLanding adds one attack strip per hit, so 4 identical strips play at once on one target; open
- Follow-up: CommandMenuTests.cs at 393 lines (388 before the slice); split by entry
- Follow-up: BattleState.cs at 292 lines
