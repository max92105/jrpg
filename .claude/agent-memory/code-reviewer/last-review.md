---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice B3 (category rails on the Shop's lists, column titles, Q/R tab keys).
Round 1. Verdict: Fix the Criticals, then commit. verify.ps1 all pass (3078 tests; the Done note says 3079).

- Critical, open: ShopScene.ChangeCategory(bool forward) is a flag argument; shape it as InventoryPage.TryChangeCategory (PreviousTab -> MoveUp, NextTab -> MoveDown, else return) then refresh the shown list
- Critical, open: "the cursor goes to the first row" untested: ShopSceneCategoryTests Down then NextTab passes with the cursor kept at 1 (potion, hi_potion both consumables); StockListTests asserts SelectedIndex 0 on a fresh list
- Warning, open: shop_hint says "the tab buttons", never Q/R or the shoulders; slice Watch says ButtonLabels reads the keys for the footer; fill them via ButtonLabels.Of per device or record the deviation
- Suggestion, open: Refresh(category) `_rows = []` is a dead store (Refresh overwrites it), in both lists; a private Rebuild(int selected) would say it
- Suggestion, open: Done note's test count 3079, verify reports 3078
- B2 suggestions: blank line fixed; Highlight rename fixed; Down_InTheSellList Buy-row half not raised again (Suggestion only)
