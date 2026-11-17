---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG9 (headless part; split, BG9b holds the dialog's look, the page's
GIF wiring and ArtImageImport's background GIFs). 2026-11-17. Round 1. Verdict: Ready to commit (no Critical).

- Suggestion (open): SheetProblem says "choose a smaller height" with "(0 at most)" for a panorama whose frame is wider
  than 4096 px at every offered height; no smaller height helps.
- Suggestion (open): FrameScaler.WidthAt(gif.Width, gif.Height, height) written three times in
  GifImportOptionsViewModel.Background.cs (SheetLine, SheetProblem, FitsOneSheet).
- Follow-up (open): Tiles/SheetImportResult now carries battle backgrounds (Tiles -> Core.Battle); it serves every art
  import, so Catalogs/ is its home (the slice text already says Catalogs/SheetImportResult).
- BG8b's Warnings (image cache in three places; FieldSpriteImagesIn without caller) and Suggestion
  (ReadableEffectImages forward): fixed (no longer in src .cs files).
- BG8's Follow-up (fresh row instead of mutating TakeAnimation): fixed (TakeImport builds with From(file, record, size)).
