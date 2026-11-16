---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice AN7 (SheetCells.Cut, Catalogs/CellStripImport,
MainViewModel.ImportCellStrip + ConfirmsReplacing/ReportArtImport, the tool's OK and RefusalText, the page's
makeStripFromCells + TakeImport). 2026-11-16. Round 1. Verdict: Ready to commit (no Critical).

- Warning (open): the strip's width (picks x max(cell side)) and height against TextureLimit.MaxSide aren't on the
  tool's Problem line, so OK stays lit for a strip Data refuses; the refusal arrives after the cut, pad and encode
  (editor rule: OK greys for every input the apply refuses). Fix: a StripFromCellsViewModel problem reading
  TextureLimit.MaxSide, tested at MaxSide and MaxSide + 1.
- Suggestion (open): SheetCellsTests only use a Bgra32 sheet; a Bgr24 or Indexed8 sheet would pin the conversion.
- Suggestion (open): SheetCells.AreaOf and PreviewFrames.OfCells compute the cell rectangle and its bound two ways.
- Follow-up: MainViewModel.SheetImport.cs:35-48 keeps its own replace question ("tilesheet art") and report.
- Checked, no finding: 988 on every path (write at once, Written only on Imported > 0, record through TakeImport);
  RefusalText cleared only by user-driven VM changes (window only reads _strip; no timer raises); declined replace
  gives Summary "" so the line stays empty; bindings are UpdateSourceTrigger=PropertyChanged so Enter on IsDefault
  sees the typed text; catch list matches ArtImageImport's minus InvalidDataException (no decode here).
- AN6 carried: Decoded copy fixed (SpriteImageFile.Read), script split (172 lines) fixed, TileImages on the shared
  read fixed, RowAt suggestion dropped (committed slice).
