---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E4b1.md, E4b1 (Ctrl+Z undoes catalog changes)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1372 tests)

- open (Critical): journal/catalog redo desync. AddTile's RecordChangeOutsideHistory clears the catalog redo stack but not the journal's undone Catalogs entries; reachable when painting a new sheet cell off the map (StartPainting/FloodFill/PaintRectangle call AddTile, no map step recorded). Redo then pushes a phantom Catalogs entry and the next Undo reverts an older catalog step ahead of newer map steps. Fix: event from CatalogDraft when redo is dropped -> journal drops undone Catalogs entries; test via MainViewModel.
- open (Critical): Redo routing (MainViewModel.RedoCommand, CatalogDraft.Redo) has no test at all; add undo-twice/redo-twice order test.
- open (Suggestion): UndoStep enum names the history, not a step (UndoTarget / StepHistory).
- open (Suggestion): MainViewModel.Undo/Redo near-identical bodies; one private method taking the journal move + the two actions, or keep.
- open (Follow-up): MainViewModel.cs 435 lines (recorded in slice note); split undo routing / unsaved-change questions out.
