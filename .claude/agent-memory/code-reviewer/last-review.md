---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice OM1 (map cell selection, Ctrl + drag in Tiles mode),
round 1 (2026-11-06). Verdict: ready to commit (no Criticals).

- Warning (open): docs/map.md Maps row has a stray `|` after the MapCellRectangle entry; 4 cells in a 3-column
  table, so the rest of the row's key types drop from the rendered table.
- Warning (open): SelectedMapCells survives a shrinking Map settings resize (same MapViewModel, OnOpenMapChanged
  doesn't fire); the outline draws past the edge and OM3's MapSelectionCanvas.Of would read outside the layout.
- Warning (open): DrawMapSelection(null) in FinishRectangle, TryClearMapSelection and OpenCurrentMap duplicates the
  existing Painting.PropertyChanged -> ShowBrushAt subscription (MainWindow.xaml.cs:61); Done note says that
  subscription doesn't exist.
- Suggestion (open): `_rectangleStart!.Value` in DrawMapSelection; pattern-match instead of the null-forgiving.
- Previous carried follow-up (MO2 wheel zoom mid Shift-drag) is in backlog.md line 208: dropped.
