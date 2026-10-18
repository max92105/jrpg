---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11d (Map documents: draft, undo and save)
Round: 1 (verdict: Fix the Criticals, then commit; caller said this is the single review round)

- open (Critical): MapViewModel Undo/Redo commands + Refresh (Map replaced, IsDirty raised, CanExecute refreshed) have no test; add MapViewModelTests for UndoCommand/RedoCommand
- open (Suggestion): CanClose()/CanLeaveCurrentMap() show a modal dialog behind a Can* predicate name; rename ConfirmClose / ConfirmLeaveCurrentMap
- open (Suggestion): verify by hand that refusing the discard question leaves the map list on the open map (SelectionChanged re-entrancy is the same risk the TwoWay binding had)
- open (Suggestion): tick 11d in the plan with deviations (tiles-only draft, TryOpenMap replaced SelectedMapId, Catalog updated on save, confirmDiscard Func)
- follow-up: ContentWriter.Write moves layout then settings; a failed second File.Move leaves a half-saved map
