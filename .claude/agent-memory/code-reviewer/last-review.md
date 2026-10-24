---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/C1c.md, slice C1c (level-up raises stats by class growth)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical: Vitals.cs:88 RaiseMaxima summary line is 121 cols; rewrap with reflow-comments.pl (open)
- Suggestion: ClassStats -> CombatStats(Attack, Defense) conversion now in PartyMember ctor and LevelUp; a private static BaseStatsOf(ClassStats) (open)
- Suggestion: no PartyMember-level test that a fallen member levels up at 0 HP with raised max (open)
