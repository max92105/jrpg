---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-code-health-cleanup, slice B2 (tile cells and animations past the image refused as counts, decision 542),
2026-11-11. Round 1. Verdict: ready to commit.

- Open (Warning): FirstTileOutside's `IsCellInside &&` clause (ArtReferenceCheck.cs:154) has no test on its row side;
  the ContentWriter import test's tile (city 31 at (3, 1)) is caught by IsLastFrameInside alone.
- Open (Suggestion): name the combined check `IsTileInside(tile, sheet, size)`; line 154 is ~165 columns.
- Verified: count checks equal the old pixel sums for every in-range value (TileWidth >= 1, Margin/Spacing >= 0,
  column/row >= 0 validated).
- start.json residue: the developer's hand test, excluded by the caller.
