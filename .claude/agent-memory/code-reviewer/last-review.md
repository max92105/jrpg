---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice OM2 (MapSelectionCanvas, ObjectCanvas.OfCells), round 1 (2026-11-06).
Verdict: ready to commit (no Criticals).

- Warning (open): MapSelectionCanvas.SheetCellOf is a second copy of ObjectCanvas's private SheetCellOf (the
  TileDefinition -> SheetCell conversion also inline in TileBrowserViewModel:180, TileImages:36, :92); share one.
- Suggestion (open): SheetCellsOf's "no layer at that z" branch; iterate layout.Layers with their level instead.
- Suggestion (open): ObjectCanvasTests.cs:140 no blank line between the last test and the new [Fact].
- Follow-up: docs/map.md Viewport row has a stray `|` after the ZoomedScroller entry (4 cells in 3 columns).
- OM1 carried items: resize clipping fixed (OnOpenMapContentChanged); Maps-row stray `|` fixed; DrawMapSelection
  duplicate / `_rectangleStart!` were OM1 round items, OM1 committed, not carried.
