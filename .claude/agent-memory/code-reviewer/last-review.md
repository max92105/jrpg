---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E11a.md, E11a (point handles: drag, insert, remove; uncommitted)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1489 tests)

- open (Critical): ObjectShapesWindow.xaml.cs drag keeps a stale `_draggedHandle`: Backspace/Ctrl+Z (OnPreviewKeyDown) or a right click on the dragged handle mid-drag removes the point, next MouseMove calls MovePoint(index == Count) -> ArgumentOutOfRangeException, no DispatcherUnhandledException handler -> editor crashes. Fix: end the drag before any other edit (key handler, right click) or ignore those while dragging.
- open (Warning): no LostMouseCapture handler: Alt+Tab mid-drag loses capture, MouseUp never arrives, point follows the mouse with no button held. Clear `_draggedHandle` in LostMouseCapture (and let MouseUp just release capture).
- open (Suggestion): ShapeEditing.EdgeAt `points.Count >= 2` buried in the for condition; use a guard clause.
- open (Suggestion): Brushes.Yellow for the last handle sits in the window; other overlay colours live in CollisionDrawing.
