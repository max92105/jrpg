---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MF4 (MapFolderChoiceViewModel, MainViewModel.MapFolders,
MapNavigation.SessionFolders), 2026-11-10. Round 1. Verdict: fix the Criticals.

- Withdrawn (Critical, wrong premise): "the map list is rebuilt only on an IsDirty flip". MapViewModel.Refresh raises
  PropertyChanged(IsDirty) on every step, undo and redo, so the rows are rebuilt every time; the undo test the review
  asked for passes with no change (kept). The developer dropped TryMoveMapTo's redundant ShowMapList call and the
  settings event, and reworded OpenMaps.DirtyChanged's summary. The Map settings follow-up falls with it.
- Fixed (Warning): TryMoveMapTo checks the row's folder before TryOpenMap, with a test.
- Open (Suggestion): split the ~170-column lines (Folders initializer, TryMoveMap, KnownFolders).
- MF3 items all fixed (ContextMenu Tag on the TextBlock, NoteExpanded ignored while filtering, SelectMap clears the
  stale row, PART_Header/ItemsHost). Session folder after a map moves out: recorded in MF4's Done note, dropped.
- start.json residue: the developer's hand test, excluded by the caller.
