---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice FS2 (FieldSpriteSheet.ColumnsNeeded/RowsNeeded,
ArtReferenceCheck.AddSpritesTooSmall, ArtReferenceCheckFieldSpriteTests).
2026-11-12. Round 1. Verdict: fix the Critical, then commit.

- Critical (open): FieldSpriteSheet.cs:36 RowsNeeded = max row + 1 wraps at row int.MaxValue to int.MinValue, so
  ArtReferenceCheck.cs:127 holdsRows passes; validator only checks rows >= 0. Fix: long, or compare `> highest row`;
  add a row-int.MaxValue test (code-quality rule line 57).
- Warning (open): FieldSpriteLayout.Holds (Data/Art/FieldSpriteLayout.cs:19) has no production caller left, only
  FieldSpriteLayoutTests; delete both now rather than wait for FS5.
- FS1 Warning (ContentCatalogLoaderFieldSpriteTests repository numbers): fixed (test world pins them, repository by id).
