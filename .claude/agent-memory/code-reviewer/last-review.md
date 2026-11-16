---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU3 (FixedAffixConverter, UniqueItemRules, loader reads affixes.json
before items.json, writer's ItemConverters, CheckItems passes content.Affixes, placeholder chiefs_cleaver).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Follow-up (open, for IU15): ContentWriter.CheckCatalogs runs CheckItems before CheckAffixes, the reverse of the
  loader's new order; once the Affixes page edits the pool, an affix edited to span 0 with a unique at -1 on it is
  named as items.json's sign problem instead of affixes.json's range. Swap the two lines in IU15.
- Checked, no finding: five old validator test files' helpers keep each file's name and every assertion; side-of-zero
  rule can't meet a 0-spanning range at load (AffixCatalogValidator refuses it, now read first); loader move only puts
  affixes.json ahead of items/crafting/shops/quests/cinematics, no other pair reordered; int.MinValue tested; null
  entry tested end to end.
- IU2's two optional Suggestions dropped (other slice, optional).
