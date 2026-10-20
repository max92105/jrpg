---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, P2b-1 (Using items from the inventory: ItemTargetPicker, ItemTargetPickerView, ItemUseDialog, InventoryPage use + notice, ListCursor.Select)
Round: 1 (verdict: Fix the Criticals; build 0 warnings, 1025 tests pass, format exit 0)

- open (Critical): ItemUseDialog.ChoiceFor + Use decide who may use a Self item and who the user is (game rule in Game, duplicates Core's CanUse rule, no test) -> move to FieldItemUse (e.g. TargetChoiceForAnyUser / UseOn(item, targets)) with Core tests
- open (Warning): ItemUseDialog.CanUse is a forwarding wrapper of FieldItemUse.CanUse -> InventoryPage calls session.FieldItems.CanUse
- open (Warning): result-text mapping untested beyond HP single target: "all" item -> item_used, revive/MP texts -> one InventoryPage test for an all-allies item (plan hand-check covers it)
- open (Suggestion): ItemUseDialog keeps _picker and _item as two nullables + impossible-state guard in Update -> one nullable pair (record) so IsOpen and the item can't disagree
- open (Suggestion): ListCursor.Select's disabled-entry throw untested
- open (Suggestion): tick nothing yet; note ItemUseDialog as a new type in the P2b-1 slice note (developer says it is listed as deviation)
