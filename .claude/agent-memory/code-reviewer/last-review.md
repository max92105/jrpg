---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E9b.md, E9b (object shapes window; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1466 tests)

- open (Warning): ObjectShapesWindow.DrawOutline StrokeThickness=1 and PointMarkPixels=3 are in map pixels inside the Viewbox, so a 1x1 object (chest, 48 px -> 560) gets a ~12 px stroke and ~35 px marks; divide by the Viewbox scale like CollisionDrawing's StrokeScreenPixels.
- open (Suggestion): OnEditShapesClicked re-fetches the definition via `_viewModel.Catalog!.Objects[shapes.ObjectId]`; the picture could come from the same definition StartEditingShapes used.
- open (Suggestion): `System.Windows.Shapes.Polygon` fully qualified next to `using System.Windows.Shapes` (Core.Geometry.Polygon clash); an alias `using WpfPolygon = ...` reads cleaner.
