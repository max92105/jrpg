---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice PM4b (the Party members page's pictures and imports). 2026-11-17.
Round 2. Verdict: Ready to commit.

- PM4 Suggestion (missing-art sentence built twice): fixed (BattleArtChoices.MissingNoteOf).
- PM4 Follow-up (TakeImport raising MissingArtNote): fixed.
- Critical (SpriteImageFile.Decode threw on a PNG cut after its header): fixed (StreamSource + OnLoad, decoder
  refusals caught to null, SpriteImageFileTests x3).
- Follow-up (header-only check at import): dropped, backlog.md records it.
- Follow-up, open: StripFromCellsWindow.xaml.cs:68-69 catch keeps NotSupported/FileFormat/COM clauses Read no longer
  throws; a corrupt sheet now reads "not a PNG image.".
- Follow-up, open: TileImages.LoadImage (Tiles/TileImages.cs:180) turns a header-readable but corrupt tilesheet into
  InvalidOperationException "found readable" (was COMException): still the error box, now with a "bug" message.
