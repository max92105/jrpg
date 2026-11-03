---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice CO2 (CanvasPainting). Round 1.
Verdict: Ready to commit.

- open (Warning): EndStroke's ReferenceEquals counts every Erase/Stamp as a change (ObjectCanvas returns a new
  instance always); a right-click on an empty cell records an undo step that undoes nothing
- open (Warning): RowsText's hook and its "Rows must be..." arm have no test; deleting OnRowsTextChanged passes all
- open (Suggestion): TypedCounts' summary still says "the map dialogs' boxes" with the composer as a new caller
- open (Suggestion): PaintAt's missing-image check is a copy of ObjectReskinning.CanReskin's; fine while CO6 deletes it
- fixed (carried from CO1): ObjectCanvas.Of overloads name their ArgumentOutOfRangeException
- open (Follow-up, CO5): a Shift+drag rectangle over 32 columns or rows makes ObjectCanvas.Of throw when the
  composer opens from it; CO5's slice doesn't say yet how it's clamped or refused
