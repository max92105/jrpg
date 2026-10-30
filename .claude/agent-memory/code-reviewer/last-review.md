---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice L2a (the Shop's detail column: ShopDetailView, TradeRow.OwnedCount/AffixLines).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3080 tests).

- Warning, open: affix lines `piece.Affixes.Select(affix => AffixText.Of(affix, messages))` now in ForgeList, SaleList (inline .ToList()) and StockList (AffixLinesOf): third copy; propose AffixText.LinesOf(GearPiece, UiMessages)
- Suggestion, open: StockListTests.Rows_Item_CarriesItsDescription asserts row.Item.Description against the same catalog (tautological); rename to what it checks (owned count, no affix lines) or drop that line
- Suggestion, open: StatusSkillsView.DescriptionLayout now shared by Status and Shop; its doc ties the font to Status's 18 px; move to Ui
- Suggestion, open: StockListTests leaves a blank line before the class's closing brace after Font() was removed
- B3 carried items: all fixed in commit 1c2124b (TryChangeCategory, shop_hint {previous}/{next}, first-row test)
