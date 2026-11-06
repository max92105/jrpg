---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice PO1 (PlacedObjectEditing, ReplacePlacedObjectEdit, MapDraft.ReplaceObject),
round 1 (2026-11-05). Verdict: fix the Criticals.
CP2 items checked in the tree: Edit menu CommandTarget fixed; Paste off-map check fixed; IsCopyingObjects gone (fixed);
CanPaste now covers swapped-to objects (fixed). Removal of a shown object's definition: impossible (Remove and catalog
undo refused while placed; purge needs every map saved and rewrites maps before Current).

- Critical, open: PlacedObjectEditing.Coordinate accepts "NaN" (double.TryParse ignores NumberStyles for the symbols);
  floor(NaN) casts to 0 so the center check passes, Apply stores a NaN position, and the save's JSON writer throws an
  uncaught ArgumentException. Fix: `&& double.IsFinite(value)`; theory row X "NaN".
- Warning, open: no CatalogDraft.Current subscription; an Edit object resize (or its catalog undo) leaves Problem and
  ApplyCommand.CanExecute stale (editor.md rule); test with SetObject wider while the panel shows the chest.
- Suggestion, open: ObjectPlacingViewModel class summary doesn't mention Copying/Editing.
- Follow-up: EventStepViewModel seconds parse accepts "Infinity" (> 0), which the save can't write.
- Follow-up (from SN1, still present): window key guards exclude only TextBox, so B/R/F/G eat a focused ComboBox's
  type-to-select.
- Follow-up (from MO2, still present): a wheel zoom mid Shift-drag runs DrawSelectionMarks, resetting the marks'
  transform until the next mouse move.
