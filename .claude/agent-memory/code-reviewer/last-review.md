---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11g (Placing objects)
Round: 1 (verdict: Ready to commit; build, 733 tests, format exit 0 all pass)

- open (Warning): MainViewModel.cs:105/240 SelectedObjectUids not pruned after undo/redo/remove; uids are reused (lowest free), so a re-placed chest_1 inherits the selection and Delete removes it instead of the hovered object; Delete with only stale uids does nothing
- open (Warning): MainViewModel.cs:198 PlaceObjectAt has no map-bounds check; click in viewport margin places an object clipped out of sight (MapDrawing clips), saved since MapLayoutValidator has no bounds check
- open (Warning): MainViewModel.cs ~320 lines, three concerns (open/save, tiles, objects); split before 11h adds map settings (not blocking)
- open (Suggestion): MainWindow.ObjectEditing.cs:82 ShowObjectDragAt also shows the placement ghost with no drag; rename ShowObjectPreviewAt
- open (Suggestion): MainWindow.ObjectEditing.cs:146 selection mark thickness fixed at 2/zoom at selection time; ApplyViewport doesn't redraw marks
- open (Suggestion): tick 11g in the plan with deviations (PlacedObjectHits instead of MapDraft.ObjectsInRectangle, EditingMode tabs, Delete on hovered object, DrawObject)
- follow-up: plan decision 8 reuses freed object uids (chest_1 after deleting chest_1) while event ids deliberately don't; saves remember objects by uid, ask developer
- follow-up: TopAt ignores canopy Z; ObjectBrowserViewModelTests line 2 re-implements the sort (tautological)
