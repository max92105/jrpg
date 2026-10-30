---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice S4 (ContentCatalog.Shops, shops.json + Data/Trade/ShopCatalogValidator,
item price 1..MaxGold, trade.sellPercent 0..100, prices in items.json; ShopCatalogValidatorTests,
ItemCatalogValidatorPriceTests, GameRulesValidatorTests +2 theories, ContentCatalogLoaderShopTests).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3007 tests).

- Warning: ShopCatalogValidator.cs:24-25 `shop.ItemIds is null` impossible (RespectNullableAnnotations +
  RespectRequiredConstructorParameters); the null-elements comment sits above it, not above FindItemProblem's null arm - open
- Suggestion: GameRulesValidator sellPercent check hand-rolls RangeProblems.Within's message - open
- Suggestion: S4 Done note says 3005 tests; verify counts 3007 - open
- Suggestion: ContentCatalogLoaderShopTests pins the placeholder share 50 (retuning breaks it) - open
- S3 carried: ItemSales piece refusals untested - fixed (PieceWithUnpricedBase, PieceNotOwned tests)
- S3 carried suggestions (test count, QuoteSale int wrap) - not re-raised (suggestions, previous slice)
