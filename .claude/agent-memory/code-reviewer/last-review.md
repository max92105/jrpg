---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice S3 (Trade/TradeRules, GameRules.Trade, TradeRefusal.NotForSale,
PriceFormula.SalePrice, Pricing.SalePriceOf x2, Trade/ItemSales, GameSession.Sales; ItemSalesTests 12,
GameSessionTradeTests 1, PriceFormulaTests +3, PricingTests +2).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (2985 tests).

- Warning: ItemSales piece overload's refusals untested (unpriced-base piece -> NotForSale, unowned piece -> false) - open
- Suggestion: S3 Done note says 2986 tests; verify counts 2985 (2967 + 18) - open
- Suggestion: QuoteSale(item, quantity) `each * quantity` int wraps for a huge quantity (same shape as Shop.QuoteBuy) - open
- S2 carried: map.md Loot row double parenthetical - fixed
- S2 carried: TryBuy_PieceNotInStock bag/stock assertions - fixed (asserts gold and empty gear)
- S2 carried follow-up: Pricing KeyNotFound for unknown affix - fixed (session builds the map from every catalog affix)
