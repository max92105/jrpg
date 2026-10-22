---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/I1.md, slice I1 (input device + ButtonLabels; Game Input only)
Round: 1 (verdict: Ready to commit; build pass; 1547 tests pass; format pass)

- warning (open): InputReader.DeviceInUse treats a stick held past the dead zone as Controller every update (level, not edge), so a held/drifting stick flips a key press back to Controller on the next update; switch only when the stick crosses the dead zone.
- suggestion (open): ButtonLabelsTests.Of_SameButtonTwice_ReturnsTheSameString passes without a cache (Enum.ToString returns a cached instance).
- suggestion (open): ButtonLabels.Of on keyboard gives raw enum names for future bindings ("Back", "Escape", "Enter", "LeftShoulder", "DPadUp"); fine per decision 138, note for G1a.
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
