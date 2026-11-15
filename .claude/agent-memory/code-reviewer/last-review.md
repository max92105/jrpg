---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-sprite-cells-and-mirrors, slice SC3 (SpriteFrame, FacingSequence.MirrorOf, StandingFrame/WalkFrameAt,
validator mirror rules, both shells' flip). 2026-11-15. Round 1. Verdict: Ready to commit.

- Warning (open): docs/map.md:41 the `SpriteArea` entry still names `StandingCell`, `WalkCellAt` (renamed to
  `StandingFrame`, `WalkFrameAt`).
- Suggestion (open): TileImages.cs:128 builds `new CroppedBitmap(sprite, area)` on both arms; crop once.
- Suggestion (open): WalkPreview.cs:54 `shown` (SpriteFrame) beside `frame` (Int32Rect) and DrawFrame's `frame`
  (BitmapSource); name the Int32Rect `source`.
- Follow-up: FieldSpriteSetupViewModel.PreviewFrameAt has no production caller (PreviewCellAt had none at HEAD either).
- Fixed from SC2: validator "JSON null" comment gone (Count > 0 now); facing spelled through ContentJsonOptions.EnumName.
