---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice SN1 (snap to grid: GridSnap, IsGridSnapOn, ...OnGrid methods, Alt inverts
the box, G toggles), round 1 (2026-11-05). Verdict: ready to commit.

Carried from MO2: ObjectPlacingMoveTests summary and near-duplicate tests, fixed (checked in the tree).

- Suggestion, open: PlaceChosenAt re-reads OpenMap! and Browser.SelectedObject! after ChosenTopLeftAt checked them;
  one private place method taking the landing function would drop both `!`.
- Suggestion, open: ToggleGridSnapCommand's only user is the G key (the box binds IsGridSnapOn); a plain public method
  avoids Execute without CanExecute in code-behind (editor.md).
- Suggestion, open: SN1 hand check step 1 names the box "Snap to grid (Alt)"; it reads "Snap to grid (Alt, G)".
- Follow-up: the window's key guards exclude only TextBox, so B/R/F and now G (Objects mode) eat a focused ComboBox's
  type-to-select (the Layer box's "Ground").
- Follow-up (from MO2, still present): a wheel zoom mid Shift-drag runs DrawSelectionMarks, which resets the marks'
  transform until the next mouse move.
