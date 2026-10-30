---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice B1 (the Buy list: StockRow, StockList, ShopListView, ShopScene Up/Down).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3057 tests).

- Warning, open: ShopListView.cs:57 rows (and the selection band) start at the column's own left edge (area.X), so the pointer tip sits on the column edge with its body outside the column; Forge/Inventory inset rows by a 28 px Margin
- Suggestion, open: StockRow.Owned is a string measured every frame in ShopListView.DrawRow:80; make it MeasuredText like Price
- Suggestion, open: test Rows_GearEntry_IsItsRolledPieceInRarityColor checks no color; rename (e.g. ..._IsItsRolledPieceAtItsQuote)
- Not raised (slice note explains): one QuoteBuy per row, no Pricing parameter; refused piece greyed whole incl. rarity color; RowsTop 96 reserved for B2/B3; shop_empty text; extra Refresh test
- Not raised: Owned counts bag pieces only (GearCountOf), consistent with decision 369 "owned pieces only"
