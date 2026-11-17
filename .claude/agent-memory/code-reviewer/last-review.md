---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG6 (Editor GifImport: FrameGrid, PngWriter, FrameScaler.ScaleAveraged,
GifConversion.ConvertToBackground, ConvertedGif.Background).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Carried from BG5: Suggestion SwapDecodedBattleBackgrounds fixed (ExplorationScene.cs:169); playthrough-cache
  Follow-up dropped (backlog.md:329); GridProblemOf-on-validated-record Follow-up dropped (BG7.md:89 note records it).
- Suggestion (open): FrameGrid.WritePng's cell origin repeats AnimatedBackgroundDefinition.FrameOrigin's arithmetic;
  pin it with a test reading every frame back at FrameOrigin(index, sheetWidth), or build the record first and use it.
- Follow-up (open): GifImportOptions.BattleHeight now also means a background sheet's frame height; a neutral name
  (FrameHeight) once BG9 settles the dialog.
