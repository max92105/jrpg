---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/MO2b.md, slice MO2b (page lists glide and bob)
Round: 1 (verdict: Fix the Criticals, then commit)

- C1 InventoryPageTests.cs:311 — new test inserted between UseThirdItemOnLeader and its "third starting item is the elixir" comment; comment now describes the wrong method; open
- W1 band + pointer draw (rect at Position*RowHeight, DrawSelectedRow, MenuPointer at X - PointerShift) copied in 5 views; propose one Ui method taking glide/band, dim flag split into two methods; open
- S1 SaveFilesPanel.SelectionPosition public only for SavePage's test accessor; could read _panel's list via Entries-like accessor; open
- F1 TitleScene keeps its own bob clock while SelectionGlide now has one (recorded in Done note); follow-up
