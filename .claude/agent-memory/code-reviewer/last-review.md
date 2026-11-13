---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice SI1 (CatalogProblems.OfFieldSprites, field-sprites.json in
ContentWriter's checked/serialized catalogs, ContentWriter.ImportArtImage/HasArtImage, CatalogDraft.SetFieldSprites,
TileImages.Forget drops sprite frames). 2026-11-12. Round 1. Verdict: fix the Criticals.

- Critical (open): CatalogDraft.FieldSprites.cs:15-23 SetFieldSprites replaces the whole record set, but the slice
  takes `changed` and decision 569 says no record is removed; SI3's ToSheets returns only edited ones -> others dropped.
  Fix: merge into before, no-op when every changed equals before's; test one changed sheet keeps the others.
- Warning (open): CatalogProblems.cs:23-26 and ContentWriter.cs:178-182 copy the same validate + ToDictionary +
  reference-check sequence; share one internal throwing method.
- Suggestion (open): ContentWriterArtImportTests.cs:91-95 comment says the reverse of the behavior; the `sheets` copy is a no-op.
- CW1 Warning 1 (WriteAll partial move): fixed (IsEditedOnDisk passes bytes equal to the write).
- CW1 Warning 2 (unchanged hand edits not rechecked): declined, design.md Risks accepts it.
- CW1 Suggestion TilesheetImagePath: fixed (ArtImagePath in ArtImports). NothingChanged test: dropped.
- FS5/FS3 Suggestions (MapDrawing summaries, TileImages past-edge reload, FittedSpriteArea FeetPixels, AreaOver odd
  width): not this diff, unchecked.
