---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-cinematic-focus-point, slice FP2 (Game/Ui/ScreenZoom, StillFrame.At with ScreenZoom, CoverFit
.DrawZoomedOverScreen from the crop's top-left at zoom.PositionOf(0), ScreenZoomTests, StillFrameTests). 2026-11-15.
Round 1. Verdict: Ready to commit. No findings.

- FP1 Suggestion (Load_FocusMissingYPercent_ThrowsNamingTheFile asserts only the file prefix): fixed (asserts
  "yPercent" now).
- Checked: new draw is the old one exactly when the origin is the screen center (top-left = center * (1 - scale)).
