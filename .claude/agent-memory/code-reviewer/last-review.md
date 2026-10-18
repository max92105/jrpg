---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11c (Viewport)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

- open (Warning): MainWindow.xaml.cs has no LostMouseCapture handler; capture lost mid-pan (Alt+Tab) leaves _panFrom set so the map follows the mouse with no button held
- open (Suggestion): Ctrl+NumPad0 / Ctrl+NumPad1 not handled (legacy handled both)
- open (Suggestion): OnFitClicked routes through HandleZoomKey(Key.D0); extract RefitMap() shared with ShowCurrentMap and Ctrl+0; MapToScreen is test-only API
- open (Suggestion): tick 11c in the plan with deviations; ShowCursorAt "No tile" branch untested
