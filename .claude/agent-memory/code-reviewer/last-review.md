---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice S1 (buying stacks in Core: ItemDefinition.Price, Trade/ShopDefinition, ShopStock,
TradeRefusal, TradeQuote, Shop with QuoteBuy/MostBuyable/TryBuy; ShopBuyTests 12). Round 1.
Verdict: Ready to commit. verify.ps1 all pass (2951 tests).

- Warning: Shop.cs:62 TryBuy's `quote.IsRefused` untested for StackFull (refused test is ShortOfGold, which TrySpend also refuses); deleting it takes the full total while Inventory.Add clamps - open
- Suggestion: MostBuyable on an item outside the stock returns >0 while TryBuy refuses it - open
- Suggestion: ShopDefinition record holds ItemIds list (reference equality; no equality user yet) - open
- G4 carried items (VictoryScreenView reward bar overlap, RewardRows EXP twice, parallel collections) - not re-raised (other slice, Warning/Suggestions only)
