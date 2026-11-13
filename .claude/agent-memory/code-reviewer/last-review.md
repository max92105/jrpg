---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice SI2 (Editor/Tiles/GridCut moved out of SheetCutViewModel;
Editor/Sprites FacingCycleSetup, FieldSpriteSetupViewModel, FrameCutGuess, SpriteScaleGuess). 2026-11-12. Round 1.
Verdict: fix the Criticals.

- Critical (open): docs/map.md:20 Core table header and its `|---|` row merged into one line (table no longer renders);
  docs/map.md:84 Tiles row gained a 4th cell `| GridCut ... |` after its closing pipe. Restore line 20/21, move GridCut
  into the Tiles row's key types cell.
- Warning (open): FieldSpriteSetupViewModel.cs:155-156 From on a valid record whose image has spare pixels (Data's
  HoldsFrames is >=, e.g. 130x224 with 32 px frames) opens with "130 px doesn't split into 4 columns" no count clears.
- Suggestion (open): FieldSpriteSetupViewModel.cs:180 `Core.Maps.TileAnimation` qualified; SpriteScaleGuess 48 copies
  MapDrawing.CellPixels; SpriteScaleGuess "commonest" and its tie untested (only all-16 and empty catalogs).
- Follow-up: docs/map.md Catalogs row still says SetFieldSprites "the sprite records replaced whole" (SI1 now merges).
- SI1 Critical (SetFieldSprites replaces whole): fixed (merges by file, no-op when unchanged).
- SI1 Warning (CatalogProblems/ContentWriter copied check): fixed (ContentWriter calls CatalogProblems.CheckFieldSprites).
- SI1 Suggestion (ContentWriterArtImportTests comment): not rechecked, Suggestion, dropped.
