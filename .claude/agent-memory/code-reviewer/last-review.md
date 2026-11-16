---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-sprite-cells-and-mirrors, slice SW3 (window: Shows boxes, picking on the image, SequenceMarks,
column numbers, PaceText). 2026-11-15. Round 1. Verdict: Ready to commit.

- Suggestion (open): FieldSpriteWindow.xaml.cs OnWalkChanged comment says the problem line doesn't follow the cells;
  it does (Walk relays Cells as the facing's name). The real reason: PickingFacing never reaches the setup's notices.
- Suggestion (open): at the 16 px zoom step, column 1's number overlaps row 1's number on the first frame.
- Suggestion (open): the pace line sits under the Frame time box, not beside it as the slice says; note it.
- Follow-up: Frozen(Brush) now in 4 Editor files and the clipped DrawingGroup + transparent pin + Freeze skeleton in 6.
- Fixed from SW2: relay of typed fields only (OnFacingChanged), PickCellAt column/negative bounds tested,
  TogglePickingCommand CanExecuteChanged tested.
- Checked: ButtonBase.OnCommandParameterChanged exists in WindowsDesktop 10.0.12 (metadata probe), so the Pick
  CheckBox re-queries CanExecute once CommandParameter binds; ToggleButton toggles by SetCurrentValue, keeping the
  one-way MultiBinding.
