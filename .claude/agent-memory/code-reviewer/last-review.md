---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice B2 (Buy/Sell tabs, SaleList, TradeRow, Ui/RowSelection, GameSession.Catalog).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3068 tests).

- Suggestion, open: ShopScene.cs:140 blank line before the class's closing brace, left by moving Draw up
- Suggestion, open: RowSelection.Selection reads as place.Selection.Selection.RowIn; rename to Highlight; ShopView.Draw param `list` is now a Place, rename `place`
- Suggestion, open: Down_InTheSellList_MovesItsOwnCursor does not assert the Buy list's row stayed (the "own cursor" half)
- Carried from B1, all fixed: rows inset by Margin 28; TradeRow.Owned is MeasuredText; test renamed Rows_GearEntry_IsItsRolledPieceAtItsQuote
- Not raised (slice note explains): TradeRow instead of SaleRow; RowSelection; CardHeader tabs with SelectionMotion.At; public GameSession.Catalog (SceneFactory has _catalog, but ShopScene ctor is at 4 params)
