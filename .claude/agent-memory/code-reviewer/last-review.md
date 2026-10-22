---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E13a.md, E13a (layout: splitters and tool bars per mode; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1537 tests)

- suggestion (open): shared + mode band share Band 0 (~1500 DIP at 150 % scaling); trailing items (Clear layer, Mark blocked) may fall into the overflow chevron; Band="1" for mode bands, or accept and check by hand.
- suggestion (open): ToolBarTray not IsLocked; bands can be dragged/reordered, against "the shared band never moves".
- follow-up: MainViewModel is 593 lines; split before more is added (carried, in backlog).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
