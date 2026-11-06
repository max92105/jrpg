---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice PO3 (PlacedObjectPanel view, EventEditingViewModel.StartEditing(uid),
EditObjectEvents shared, key guards skip ComboBox), round 1 (2026-11-06). Verdict: ready to commit.
SN1 follow-up (key guards ate a focused ComboBox's type-to-select): fixed in PO3.

- Suggestion, open: MainWindow.xaml.cs HandleToolKey comment "Anywhere but a text box" is stale after the ComboBox
  guard; a focused sheet box (Tiles panel) now takes B/R/F as type-to-select, switching sheet instead of tool.
- Suggestion, open: StartEditing_UidOnTheMap_StartsOnItsEvents compares only Events.Count; assert Title names the uid.
- Follow-up (from MO2, still present): a wheel zoom mid Shift-drag runs DrawSelectionMarks, resetting the marks'
  transform until the next mouse move.
