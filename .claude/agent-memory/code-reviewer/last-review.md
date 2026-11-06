---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice MO1 (Ctrl + click selects one object, Esc clears, arrows nudge the selection;
MoveObjectsEdit), round 1 (2026-11-05). Verdict: fix the Criticals.

- Critical, open: ObjectPlacingMoveTests.NudgeSelectedObjects_Undo_PutsBothBackExactlyInOneStep: front moved to
  (2.3, 4.7) lies outside SelectBoth's rectangle (Y 1.5-3.5, chest 1x1), so only back moves; and 2.3/4.7 + 0.1 - 0.1
  is exact anyway. A per-object Do loop or offset-based revert passes. Fix: start at (0.1, 0.3), nudge by NudgeCells,
  select with a rectangle covering both.
- Warning, open: NudgeSelectedObjects_SelectedObjectUndoneAway_MovesOnlyTheOthers comment says "front chest's placement
  undone ... moves only the back one"; the test undoes a third chest and asserts both chests move.
- Warning, open: Escape with a selection is handled from any focus but a TextBox; with a toolbar ComboBox dropdown
  open (Layer, Tool, Brush), Esc clears the selection and the dropdown stays open. Guard like the arrows (map focus)
  or skip an open ComboBox.
- Suggestion: MoveObjectsEdit test's two different uids can't show the reverse order; ObjectEditing comment "nudge one".
