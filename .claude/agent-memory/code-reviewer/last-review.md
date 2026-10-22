---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/N1.md, N1 (An NPC in the game; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1524 tests)

- suggestion (open): StartMapVillagerTests.Move_IntoVillagersFeet_IsBlocked has no control proving the villager (not a tile) stops the player.
- suggestion (open): AbsentWhenNullFilter summary says "these fields" but holds one.
- follow-up: editor PlacedObjectHits.CoversWithATile hits by tiles only, so a click can't select the villager (rectangle select can); for N2.
- follow-up: MainViewModel is 573 lines; split before more is added (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
