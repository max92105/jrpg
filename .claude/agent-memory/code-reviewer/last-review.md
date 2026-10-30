---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice S6 (ShopStock.ToSaved; ShopStocks.Capture/Restore + StacksOf;
GameSession Capture/Restore; Data/Saving/ShopStockRepair; GearRepair.RepairAffixes internal; fixture general_store).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3025 tests).

- Suggestion: ShopStockRepair.IsKept reports changes from inside a Where predicate (side effect hidden in a query) - open
- Suggestion: saved RolledAtLevel not bounded by the XP table's max level: a lowered cap leaves the stock never rerolling - open
- S5 carried suggestions (borrowed Catalog(), reroll asserted by count) - not re-raised (suggestions, previous slice)
