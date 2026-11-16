---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU4 (GearRepair.RepairAffixes -> FollowRecord / plain / ClampAffixes,
ShopStockRepair.IsKept keeps a unique with a gone affix, ForgeRefusal.FixedAffixes first in QuoteReroll, PieceName
names a Unique by its base name). 2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): GearRepairTests' IsNotClamped test feeds the same input as ReportsNothing; feed it a
  non-record piece (e.g. Magic power 3) so it alone proves the record's 9 escapes the clamp.
- Suggestion (open, optional): the `sturdy` AffixDefinition is now built in two GearRepairTests tests; hoist a field.
- Follow-up (open): ItemDefinitionBuilder has no AsUnique; the `with { IsUnique = true, FixedAffixes = ... }` block is in
  about 15 test files.
- Follow-up (open, for IU15, carried from IU3): ContentWriter.CheckCatalogs runs CheckItems before CheckAffixes, the
  reverse of the loader's order; swap them in IU15.
- Checked, no finding: SavedGear unchanged (no save format change); SavedGear.Equals compares affixes by sequence, so
  the "follows its record" report fires only on a real difference; RepairWorn runs before BonusesOf
  (SaveGameRepair.cs:80-84) and the worn test's 215 -> 203 fails if the Unique affix were summed (221 max with it).
