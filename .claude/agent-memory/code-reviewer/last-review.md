---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice G2 (format-6 save: SaveGame.Gold/ShopStocks, Core/Trade/SavedShopStock,
FileSaveStore gold range + stock null checks, GameSessionGoldSaveTests). Round 1.
Verdict: Ready to commit. verify.ps1 all pass (2927 tests).

- Suggestion: docs/map.md Trade row inserted between Crafting and Dialogue; table is alphabetical, move it after Statuses - open
- Suggestion: GameSessionGoldSaveTests header comment "(S5, S6)" goes stale when S6 fills the stocks; S6 should reword it - open
- G1 carried suggestions (CanSpend(-1), PartyState.Purse summary, Add exactly to MaxGold test) - fixed (grep confirmed)
