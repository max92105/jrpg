---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/T1.md, slice T1 (page transition: Ui/Easing, Menus/PageTransition, GameMenuScene)
Round: 1 (verdict: Ready to commit)

- suggestion (open): PageTransitionTests has no midway golden value (e.g. Duration/2 -> Offset 3, Opacity 0.875); only the endpoints are pinned.
- suggestion (open): Easing.ShareOf with a zero length gives NaN (0/0) which Math.Clamp passes through; doc the length > 0 precondition or return 1 for zero, before the 6 backlog copies move onto it.
- follow-up: the 6 clamped-share copies and 2 cubic ease-out copies (backlog) can now move to Ui/Easing.
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
