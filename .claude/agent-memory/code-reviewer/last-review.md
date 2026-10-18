---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11e (Tile painting)
Round: 1 (verdict: Fix the Criticals, then commit)

- open (Critical): dotnet format --verify-no-changes fails, exit 2: ENDOFLINE at src/Jrpg.Editor/Shell/MainViewModel.cs:47 (LF line in CRLF file); run dotnet format
- open (Warning): Save (Ctrl+S) mid-drag doesn't end the stroke: file holds the partial stroke, stroke recorded after MarkSaved, Undo then reports clean while draft differs from file; also close mid-drag skips the discard question. Fix: MarkSaved (and ConfirmLeaveCurrentMap/close) call EndStroke first
- open (Warning): EndStroke inside Undo/Redo/Do (mid-drag closes the stroke) has no test; add MapViewModelTests Undo_MidStroke_UndoesTheStroke
- open (Suggestion): MainViewModel.ContinueStroke/EndStroke are pure forwarders to CurrentMap
- open (Suggestion): TileBrowserView redraws on every PropertyChanged (3 redraws per Select, SheetTiles LINQ re-run each time); filter on property name
- open (Suggestion): tick 11e in the plan with deviations (Square doesn't clip, TileStroke/TileBrush/TileImages/TilesheetMarks added, MapDraft keyed by Z, EditHistory.Record)
- follow-up: Layers ListBox doesn't set LayerZ on selection (could pick the paint layer)
