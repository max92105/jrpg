---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice PO2 (PlacedObjectEditing swaps the placement's object; Warning for stranded
gated events), round 1 (2026-11-05). Verdict: ready to commit.
PO1 items checked in the tree: NaN coordinate fixed (IsFinite); CatalogDraft.Current subscription fixed; ObjectPlacingViewModel
summary names Copying/Editing (fixed); EventStepViewModel seconds Infinity fixed (IsFinite).

- Suggestion, open: the center rule now reads the chosen object's size, untested; test a swap near the right edge to a
  wider object giving "The object's center must stay on the map."
- Suggestion, open: no test that Apply stays enabled while Warning is shown (the slice's "a warning, not a problem").
- Suggestion, open: ObjectIds hand-rolls its notification; [ObservableProperty] with a private setter keeps the
  SequenceEqual guard.
- Follow-up (from SN1, still present; now matters for PO3's object box): window key guards exclude only TextBox, so
  B/R/F/G eat a focused ComboBox's type-to-select.
- Follow-up (from MO2, still present): a wheel zoom mid Shift-drag runs DrawSelectionMarks, resetting the marks'
  transform until the next mouse move.
