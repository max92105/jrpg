---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/H1.md, slice H1 (menu footer hint line: IMenuPage.Hint, GameMenuScene.FooterHint)
Round: 1 (verdict: Fix the Criticals, then commit)

- critical (open): SavePage.cs:60-61 new `Hint` inserted between LeavesOnCancel's doc ("False while the overwrite question is open...") and the property; the doc now describes Hint and LeavesOnCancel lost it. Move Hint above the comment.
- suggestion (open): GameMenuScene.DrawFooterHint caches a MeasuredText whose Width is never read (left-aligned draw); draw FooterHint directly, or keep only if Q2/centering needs width.
- suggestion (open): MessageIds — one group doc on MenuHintInventory, seven MenuHint* consts undocumented, unlike every other id in the file; one line each.
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
