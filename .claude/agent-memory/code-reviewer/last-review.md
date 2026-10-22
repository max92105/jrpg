---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E11b.md, E11b (rectangle, snap, zoom, dimming; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1493 tests)

- open (Suggestion): ObjectShapesWindow.OnViewportMouseWheel zooms around the scroll origin, not the mouse; on a big object the spot under the cursor slides away. Keep the point under the mouse by adjusting ScrollViewer offsets after the zoom.
- open (Suggestion): no test pins snapping through SetRectangle / MovePoint (same Placed path as AddPoint, so low risk).
