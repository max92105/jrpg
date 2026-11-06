---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice MO2 (Shift + drag on a selected object moves the whole selection;
NudgeSelectedObjects renamed MoveSelectedObjects; IsSelected), round 1 (2026-11-05). Verdict: ready to commit.

Carried from MO1, all fixed (checked in the tree): undo test from (0.1, 0.3); third-chest comment; Esc guarded by
MapViewport.IsKeyboardFocusWithin; MoveObjectsEdit same-object test; "nudge one" comment.

- Suggestion, open: ObjectPlacingMoveTests summary names only Ctrl + click and the arrows; `_TwoSelected_MovesBothByTheStep`
  and `_TwoSelected_MovesBothByTheOffset` are near-duplicates now.
- Suggestion, open: a wheel zoom mid Shift-drag runs ApplyViewport -> DrawSelectionMarks, which resets the marks'
  transform until the next mouse move while the ghost stays put.
