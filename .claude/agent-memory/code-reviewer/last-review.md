---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice MS3 (UndoStep; UndoJournal picks the newest step of the shown map or the
catalogs; CombinedHistory keeps steps across switches), round 2 (2026-11-05). Verdict: ready to commit.

- Fixed (Critical): catalog redo skipping a hidden map's newer undone step; UndoJournal.IndexOfNextRedo holds it
  (greyed, decision 503 amended), MainViewModelSwitchUndoTests covers the input.
- Fixed (Warning): Undo_OnAnotherMap_DoesNotTouchTheHiddenMap now executes Undo.
- Fixed (Suggestions): CombinedHistory param doc, MainViewModelOpenMapsTests comment, UndoStep private constructor.
