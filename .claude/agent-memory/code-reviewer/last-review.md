---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-game-code-health-cleanup, slice GB1 (no Qty title on the Weapons and Armor tabs), 2026-11-12.
Round 1. Verdict: ready to commit.

- Warning (open): InventoryColumnView.cs:34,52,70 — `_categories[place.Rail.Selected].Category` re-derives
  `CategoryRail.Category` (CategoryRail.cs:29); drop the field, use `ShowsCounts(place.Rail.Category)`.
- Suggestion (open): docs/map.md Menus line — `InventoryFilter` entry doesn't mention `ShowsCounts`.
- Verified: Rail.Selected indexes rail.Tabs (InventoryPage.cs:50 passes `_rail.Tabs`); no per-frame alloc;
  equipment always goes to Gear (Inventory.Add:27), RowOf(GearPiece) Quantity: null, so Weapons/Armor rows never count.
- R3 Critical (backlog -Fix line): fixed (backlog.md:206 restored, no duplicate). R3 Suggestion: not carried.
