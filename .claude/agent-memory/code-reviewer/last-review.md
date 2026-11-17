---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG8b (BG8's split: Backgrounds/BattleBackgroundPage, FramePreview
Stretch and PictureHeight, row FrameMilliseconds, shell ReadableImagesIn/ReadableSizeOf, menu wiring).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical).

- Warning (open): decode-once image cache now in three places (BattleBackgroundPage.ImageOf,
  EffectStripCatalogPage.ImageOf, Catalogs/ArtPictureConverter); proposed shared Catalogs type with Of(file) and
  Forget(files).
- Warning (open): ArtFolders.FieldSpriteImagesIn (Data/Art/ArtFolders.cs:42) lost its only caller; only its test uses it.
- Suggestion (open): ReadableEffectImages is now a one-line forward to ReadableImagesIn(ArtFolders.Effects).
- BG8's Suggestion (Gone.png import test): fixed (BattleBackgroundImportTests).
- BG8's Follow-up (BG9.md plans a mutating TakeAnimation; build a fresh row and share TakeStillImage's
  replace-and-insert): open, for BG9.
