---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice SI3 (Editor/Sprites FieldSpriteChoice, FieldSpriteCatalogViewModel,
FieldSpriteImport, FieldSpriteEditingViewModel). 2026-11-12. Round 1. Verdict: fix the Criticals.

- Critical (open): FieldSpriteCatalogViewModel.cs:92-93 CopySetupFrom integer-divides; decision 570's "problem when the
  image doesn't split into that frame size" never shows (144x192 from a 32 px 3-frame source -> 36x32 frames, Problem
  null); doc line 81 and the slice note ("48 px frames do split 128 px") are wrong.
- Warning (open): RefreshAfterImport (:114-119) keeps an opened setup across a replacement of its image (stale size).
- Warning (open): ToSheets (:104-107) / Problem: a not-set-up image merely chosen becomes a record on OK, or greys OK
  with its guess's problem.
- Suggestion (open): FieldSpriteImport copies SheetImport; CopySetupFrom :97 redundant raise, :86 re-reads size.
- Follow-up: SI2 Warning FieldSpriteSetupViewModel.cs:91-92 From on spare pixels still open.
- SI2 Critical (map.md table/GridCut cell): fixed. SI2 Follow-up (Catalogs row "replaced whole"): fixed.
