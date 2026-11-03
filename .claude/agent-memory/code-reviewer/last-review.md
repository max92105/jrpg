---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice CO5 (ObjectComposerWindow, New object opens it). Round 1.
Verdict: Fix the Criticals, then commit.

- open (Critical): ObjectComposerWindow focuses IdBox on Loaded and the canvas Image takes no focus, so Ctrl+Z/Ctrl+Y
  reach the TextBox's own undo (typed id reverts, stroke stays); fix: Focusable canvas + Focus() in OnCanvasPressed
- open (Suggestion): MainViewModel.cs:242-243 double blank line left where CreateObject was removed
- open (Suggestion): docs/plans/backlog.md:24 still names CreateObject_* / CanStartNewObject_* tests (removed/renamed)
- open (Follow-up): TileBrowserView.OnDataContextChanged keeps _cut subscribed when DataContext goes null (picker can open Cut)
- open (Follow-up, from CO4a): StartNew's column clip still untested (only StartNew_RectangleTallerThanMaxCells_...)
- fixed (from CO4b): GridLines shared (MapDrawing.GridLines); TileImages field renamed _cells; Draw_CellAtTwoOne test added
