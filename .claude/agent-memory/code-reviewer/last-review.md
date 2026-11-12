---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice CW1 (CatalogFileStamps owned by ContentWriter, StampCatalogs after
load, only changed catalogs written, a changed one edited on disk refused; image imports moved to ContentWriter.ArtImports.cs).
2026-11-12. Round 1. Verdict: ready to commit (no Critical).

- Warning (open): ContentWriter.cs:259 WriteAll partial-move failure: moved catalogs' disk bytes differ from stamps
  (not renewed), so the next save refuses them as "edited on disk" instead of "writes them all again" (comment stale);
  fix: IsEditedOnDisk false when disk bytes equal the bytes about to be written.
- Warning (open): ContentWriter.cs:153-166 validation checks the editor's in-memory catalogs, but an unchanged file's
  hand edit stays on disk, so disk can end inconsistent (hand-removed tile + editor object using it) -> game refuses at startup.
- Suggestion (open): ContentWriter.cs:221 TilesheetImagePath only used by ArtImports partial; move it there.
- Suggestion (open): slice's WriteMapCatalogs_NothingChanged_WritesNothing test absent (covered indirectly by TwoSaves).
- FS5 Suggestions (MapDrawing summaries, TileImages past-edge reload, FittedSpriteArea FeetPixels): not this diff, unchecked.
- FS3 Suggestion AreaOver odd width: open (not this diff).
