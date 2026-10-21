---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B7a.md, B7a (victory screen: ExperienceBarAnimation, VictoryScreen, BattleScene.Finish keeps victory on screen)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1275 tests)

- open (Critical): ExperienceBarAnimation complete state when last step IsLevelUp (gain ends exactly on a level, e.g. Lv1 50/100 +50, or reaching max level) shows final Level with last step's full bar (100/100) instead of 0 into the new level; untested.
- open (Suggestion): IsDone (VictoryScreen) vs IsComplete (bar) for the same idea; pick one. `_isLeaving` reads as in-progress, it means "has left".
- open (Suggestion): no test that the level text follows the bar across a level-up (ShowBars' change detection).
- open (Follow-up): a max-level member gets no steps and shows an empty 0/0 bar (accepted by slice note); revisit in B7b look.
