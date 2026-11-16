---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-sprite-cells-and-mirrors, slice SW2 (picking cells, boxes' members, FacingSourceChoice). 2026-11-15.
Round 1. Verdict: Fix the Criticals, then commit.

- Critical (open): FacingCycleSetup.OnFacingChanged relays derived notices (IsOnRow/IsOnCells/IsMirror/CellsText) and
  OnWalkChanged relays PickingFacing/IsAnyFacingOnRow to Problem; a Pick toggle marks the setup edited and clears the
  catalog's refused-copy line; Data's check runs ~8x per Source change. Relay typed fields only, plus a test.
- Warning (open): PickCellAt's column bound (x past the last whole column) and y < 0 untested; only the row side is.
- Warning (open): TogglePickingCommand.NotifyCanExecuteChanged on a Source change and the CellsText relay untested.
- Suggestion (open): ProblemWithin and ToCycle re-spell IsAnyFacingOnRow's `Source == Row` test.
- Fixed from SW1: the facing relay's Facing.ToString() is now NameOf's nameof switch.
- Dropped: PreviewFrameAt with no caller (in docs/plans/backlog.md).
