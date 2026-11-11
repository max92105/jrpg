---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-code-health-cleanup, slice B3 (a zoom mid-drag keeps the selection's marks; one
`ShowObjectDragAt(_mapPoint)` call in `ApplyViewport`'s zoom-changed branch), 2026-11-11. Round 1. Verdict: ready to commit.

- No Critical, no Warning.
- Verified: no recursion (ShowObjectDragAt -> PlaceOnMap/ShowGhost/Snapped, none reach ApplyViewport; RenderTransform
  and Canvas children don't drive MapViewport.SizeChanged); call order vs `_gridDrawnFor` irrelevant (not read by it);
  `_mapPoint` only drives the preview, FinishObjectDrag reads the mouse-up point.
- Open (Follow-up): OnOpenMapChanged (MainWindow.xaml.cs:145) also calls DrawSelectionMarks, so an undo/redo during a
  Shift + drag snaps the marks back the same way (and `_movedObject.Placed` may be a stale record).
- B2's open Warning/Suggestion (FirstTileOutside row-side test, IsTileInside name): not re-checked here, B2 committed.
