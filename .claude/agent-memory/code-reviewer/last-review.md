---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice SH3 (Shops window, Database menu: Trade/ShopCatalogWindow,
Shell/MainWindow.Database, Shell/MainViewModel.Database, ShopCatalogViewModel.CanApply). 2026-11-13. Round 1.
Verdict: ready to commit. XAML bindings all checked against ShopViewModel/ShopCatalogViewModel/PricedItemChoice: match.

- SH2 carried: RecordListProblem Warning fixed (Catalogs/RecordListProblem.FirstOf in use); SH2 Suggestions not
  carried (committed slice); ListOrder summary Follow-up is in backlog.md line 206, dropped.
- Suggestion (open): "Sells, in order" ListBox shows raw item ids while the picker shows names.
- Suggestion (open): every ShopViewModel PropertyChanged (SelectedItem, ItemToAdd, Label, Problem) reruns the whole
  check twice (Problem + CanApply); filter OnShopChanged to Id/Name.
- Suggestion (open): CanApply test interleaves act and assert; split appear/go or keep one act per test.
