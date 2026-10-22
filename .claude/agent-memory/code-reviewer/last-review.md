---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E10.md, E10 (above-player zones in the shapes window; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1477 tests)

- open (Warning): ObjectShapesWindow.xaml.cs OnZoneListSelectionChanged ignores -1, so Ctrl+click deselecting the chosen zone leaves the list showing none while clicks/Backspace/Clear/Delete zone still act on that zone; put the list's choice back when it goes to -1 while the view model still has one.
- open (Suggestion): ObjectShapesWindow.xaml.cs class summary and `shapes` param doc still say "collision outline" only / "OK when the outline can be applied".
- open (Suggestion): no test that RemoveLastPoint / ClearShapeCommand act on the chosen zone and leave the outline (slice goal names them).
- open (Suggestion): ToDefinition's one-line zone-dropping expression; a private ZonesWithPoints() would read better.
