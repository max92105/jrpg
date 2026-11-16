---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU1 (writer writes items.json and affixes.json; CatalogProblems.CheckItems
and CheckAffixes; item/equipment filter rows; StatBonusesConverter.Write in StatKind order; both files relaid).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): CheckItems validates `[.. content.Shops.Values]` and `[.. content.DropTables.Values]` in
  dictionary order; the loader reports the first problem in file (id) order. Sort them by id, or note why not.
- Checked, no finding: CheckItems order = loader (items, rules crafting, shops, drop tables, new game); layouts check
  item ids via ContentWriter.cs CheckLayouts; relay is layout only (PowerShell semantic compare of HEAD vs tree: 23/23
  items, 23/23 affixes, no value differs, ids ordinal-sorted); reference converters only touch TieredEffect and
  StatusDefinition values, so an item with no tier/grants writes as before.
- EB12 suggestions: the unafflictedFoe Data test now exists (fixed); PoisonSkill suggestion dropped (other plan,
  optional).
