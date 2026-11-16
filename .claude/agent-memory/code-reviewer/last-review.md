---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WD2 (Workspaces/IEditedCatalog, CatalogDialog, DialogBounds; Trade/ShopCatalogPage
replaces ShopCatalogWindow; OpenInDialog in MainWindow.Database). 2026-11-16. Round 1. Verdict: Ready to commit.
No Critical, no Warning.

- Suggestion, open: DialogBounds.FitTo reads owner.Left/Top for a Normal owner; hand-check a Win+Arrow snapped main
  window (WPF may keep the pre-snap Left/Top), else read RestoreBounds-free bounds another way.
- Follow-up, open: .claude/rules/editor.md says a dialog's OK greys "through CanApply"; CatalogDialog greys on
  IEditedCatalog.Problem (the slice note records CanApply's removal), so the rule's wording is stale for pages.
- Fixed (from WD1): OnDatabaseMenuClicked now throws naming the Header when Tag isn't a known title.
- Checked: no stale ShopCatalogWindow/"Shops window" in src, tests or live docs (NET1's mention is a done slice);
  CatalogDialog's OK style is BasedOn; page binds AncestorType=trade:ShopCatalogPage; no SizeToContent on the five
  FitTo'd windows.
