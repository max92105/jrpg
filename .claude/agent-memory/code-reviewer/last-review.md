---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice CO4b (ObjectCanvasDrawing, TileImages.CellImageOf). Round 1.
Verdict: Ready to commit.

- open (Warning): ObjectCanvasDrawing.GridLines copies MapDrawing.DrawGrid's line loops; share a
  MapDrawing.GridLines(columns, rows) geometry (GridBrush could then stay private or be one GridPen)
- open (Suggestion): Draw's cell placement (CellArea) untested; pin a cell at (2,1) drawn at (96,48)
- open (Suggestion): TileImages._tiles now caches any cell; rename _cells
- fixed (carried from CO4a): CanEdit capped at MaxCells (test CanEdit_ObjectTallerThanMaxCells_IsFalse)
- fixed (carried from CO4a): TryApply chooses _catalog.Current.Objects[id] (test TryApply_EditWithNoChange_...)
- open (Follow-up, from CO4a): StartNew's column clip still untested (only StartNew_RectangleTallerThanMaxCells_...)
