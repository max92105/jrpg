---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice OM4 (Replace the map cells with the new object), round 1 (2026-11-06).
Verdict: ready to commit (no Criticals).

- Warning (open): no test erases a layer over the base (level > 0); all tests take Decoration as the top layer.
- Warning (open): MainViewModel.ObjectFromMap.cs:93-94 FirstOrDefault + `is not null` + `!` guard a layer that can't
  vanish while the modal composer is open; `Single` instead.
- Suggestion (open): ErasedCells' Zip of two Selects over the same tiles; ErasedOn reads TileIdAt three times.
- Suggestion (open): MainWindow.ObjectEditing.cs:24/28 mix `Composing?.StartNew()` with `Composing!.TryApply`.
- Follow-up: OM3 leftovers still open: no DrawingWarningOf test with both warnings; StartObjectFromMap's
  `CurrentMap is not { } shown` guard.
- OM3 carried, fixed: MapLayerWarning collapsed when null; ObjectComposing summary names the map selection;
  ObjectComposingLevelTests helper now last.
