---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG4 (startup art check on backgrounds' size and animated grid,
import refuses backgrounds over 4096 a side, placeholder ForestPulse.png + script).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Fixed (from BG3): FrameOrigin's row overflow is now bounded: the art check requires rows needed <= height /
  frameHeight, so a passing record's row origin is <= image height.
- Suggestion (open): TextureLimit summary "The import checks a battle image's width only" reads against the next
  sentence; say "a battler's or effect strip's width".
- Suggestion (open): GridProblemOf_ImageOneFrameWide_HoldsOneColumn asserts Null; name it _IsNull like its siblings.
- Suggestion (open): no test for an animated record's sheet named in the wrong case (shared path with maps, tested).
- Follow-up (BG7): GridProblemOf divides by FrameWidth/FrameHeight and reads Animation; the editor must call it only
  on a record its validator passed.
