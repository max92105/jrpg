---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/C5.md, slice C5 (InventoryPage split: InventoryRow, InventoryListView)
Round: 1 (verdict: Ready to commit)

- S1 InventoryPage/InventoryListView — BodyPixels = 26 now declared in both; description and rows can drift apart; expose one const from the view; open
- S2 InventoryListView.Measure — name doesn't say which font; MeasureInRowFont or a doc tying it to the quantity column; open
- S3 InventoryListView.Place — `internal` while every other view's Place is `public`; open
