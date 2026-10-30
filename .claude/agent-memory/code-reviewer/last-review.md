---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice S2 (affix goldValue, Trade/PriceFormula.PieceValue, Trade/Pricing, ShopStock
pieces + RolledAtLevel + TryTake, Shop.QuoteBuy/TryBuy(GearPiece); PriceFormulaTests 3, PricingTests 4, ShopPieceTests 5).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (2967 tests).

- Warning: docs/map.md:31 Loot row `AffixDefinition` has two parentheticals (new field list + old one without gold value) - open
- Suggestion: TryBuy_PieceNotInStock_ReturnsFalse doesn't assert the bag stays empty / stock unchanged - open
- Follow-up: Pricing.PriceOf(GearPiece) throws KeyNotFoundException for an affix id missing from the map (S3 builds it) - open
- S1 carried: TryBuy StackFull untested - fixed (ShopBuyTests.TryBuy_StackFull_ChangesNothing exists)
- S1 suggestions (MostBuyable outside stock, ShopDefinition list equality) - not re-raised
