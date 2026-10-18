---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11f (Rectangle, flood fill, fill and clear layer)
Round: 1 (verdict: Ready to commit; build, 715 tests, format exit 0 all pass)

- open (Suggestion): MainWindow.xaml.cs OnViewportLostMouseCapture clears _rectangleStart but doesn't call ShowBrushAt(_brushCell), so the dragged-rectangle outline stays on screen after release until the mouse moves
- open (Suggestion): MainWindow.xaml.cs ShowBrushAt enumerates the whole rectangle/square for First()/Last() on every mouse move; compute the bounds from the corners
- open (Suggestion): tick 11f in the plan with deviations (TileTool enum + ComboBox, MapViewModel.PutTiles, Fill tool as well as Shift+click, layer the map lacks counts as empty)
- follow-up: FillLayer/ClearLayer commands always enabled (no CanExecute on open map / chosen tile)
