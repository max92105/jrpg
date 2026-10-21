---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B7b1.md, B7b1 (victory layout: VictoryScreenView, VictoryRewardRow, Ui/PointedBarShape, VictoryScreen made pure)
Round: 1 (verdict: Ready to commit; verify all pass, 1279 tests)

- open (Suggestion): ShowCard's change detection (level/needed/experience text follows the bar across a level-up) untested; same gap as B7a's ShowBars.
- open (Suggestion): PointedBarShape.RewardBar repeats OutlinedShape.Pixels' mask-fill loop minus the edge; share a fill-only path in OutlinedShape.
- open (Suggestion): `_neededs` reads badly; `_nextLevelTexts` / `_experienceTexts` / `_levelTexts`.
- open (Follow-up): max-level member shows "EXP 0 /0" and an empty bar (carried from B7a).
- open (Follow-up): cards grow down by index/2 with no cap; more than 6 active members would run into the footer line at y=910 (MaxActiveMembers is content).
