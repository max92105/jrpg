---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-sprite-cells-and-mirrors, slice SW1 (FacingSetup, FacingSource, FacingCycleSetup through four
facings, row boxes rebound). 2026-11-15. Round 1. Verdict: Ready to commit.

- Suggestion (open): FacingCycleSetup.OnFacingChanged raises `Facing.ToString()` as the property name; a renamed
  property or enum member breaks the relay silently (only Up is tested). A switch to nameof(Down)..nameof(Right).
- Follow-up: FieldSpriteSetupViewModel.PreviewFrameAt has no production caller (carried from SC3; not in backlog).
- Fixed from SC3: map.md SpriteArea names StandingFrame/WalkFrameAt; TileImages crops once; WalkPreview's Int32Rect
  is `source`.
