---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, P1-1 (Inventory page: InventoryFilter, InventoryPage first tab, category via PreviousTab/NextTab, 10-row scroll, "Nothing here." empty state)
Round: 1 (verdict: Fix the Criticals; build 0 warnings, 1004 tests pass, format exit 0)

- open (Critical): empty-category state (null row cursor, SelectedRow null, Up/Down no-op, no ListCursor(0) throw) has no test; starting items fill every category. Add test using Inventory.TryRemove to empty Armor
- open (Suggestion): Update_NextCategory test doesn't assert SelectedRow == 2 before NextTab, so it can't tell reset from never-moved
- open (Suggestion): category order kept in three parallel places (enum, _categories, InventoryLabels.Categories); use Enum.GetValues / key labels by category
- open (Suggestion): tick P1-1 in plan and record test-name deviation (Update_NextCategory_... vs InventoryPage_NextCategory_...)
