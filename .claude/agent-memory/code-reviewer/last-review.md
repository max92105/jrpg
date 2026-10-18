---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 5c (Save repair against current content)
Round: 1 (verdict: Ready to commit)

Accepted (don't re-raise): public RepairedSave returned by Read; SaveGameRepair instance + static Repair(slotName, save, catalog); static WorldStateRepair; level<1/negative experience clamped; inventory merge then MaxStack clamp; position outside an existing map left to WarpArrival; test names without class prefix (matches the project's test files).

- open (Warning): SaveGameRepair.RepairInventory `quantities[itemId] += quantity` overflows (potion int.MaxValue + potion 1 -> int.MinValue, passes ClampToMaxStack, Inventory.Add throws on restore); clamp per entry or add in long
- open (Warning): duplicate member ids in Members kept -> GameSession.ActiveOrder .Single throws InvalidOperationException on Restore; dedupe (first wins + change line) or refuse as damaged
- open (Suggestion): RepairActiveOrder drops duplicate active ids / active ids absent from Members with no change line
- open (Suggestion): event/swap change lines say "which no longer exists" without saying whether map or event; name the missing part
- open (Suggestion): worn non-equipment (Equipment is null) and swap on unknown map clauses untested
