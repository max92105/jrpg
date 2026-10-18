---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11b (Editor skeleton and map view)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

- open (Suggestion): MapDrawing.DrawCanopy pushes one clip per zone, so overlapping zones draw semi-transparent pixels twice; game's CanopyMask is the union. Use one GeometryGroup clip (no multi-zone objects today)
- open (Suggestion): test Open_StartMap_ExposesItsLayersInZOrder names a method that doesn't exist; rename Constructor_LayersOutOfZOrder_ExposesThemBottomFirst
- open (Suggestion): tick 11b in the plan with deviations (DrawingGroup+Image instead of DrawingVisual, test rename, MainViewModel tests, ArtReferenceCheck gate)
- follow-up: art-problem branch of MainViewModel (ArtReferenceCheck -> LoadError, no maps) has no test
- follow-up: a corrupt (present but undecodable) tilesheet PNG throws from MapDrawing in the PropertyChanged handler, crashing the editor
