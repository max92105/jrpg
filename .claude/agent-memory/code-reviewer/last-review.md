---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice FX2a (the rules half of FX2: Core TieredEffect.AmountAt, Editor
Catalogs/TierIds, Effects/TieredEffect{,Catalog,Editing}ViewModel; code uncommitted; the page is FX2b). 2026-11-16.
Round 1. Verdict: Ready to commit.

- Warning (open): TieredEffectCatalogViewModel.Problem reports the first broken effect in list order, not the chosen
  one's first as FX2.md's Goal (and FX5.md) say; matches every other catalog page, so reword the slices or record it.
- Suggestion (open): Duplicate on an added effect whose id is still empty gives "_1" (Stem("") is ""); grey Duplicate
  or give an empty id while the chosen id is empty.
- Suggestion (open): DuplicateOf builds the record at StartingPerPower then overwrites PerPowerText; a one-line why
  comment, or a constructor taking the per-Power text.
- FX1 carried items: OfTieredEffects_TestWorldEffects_IsNull renamed (fixed); CatalogReferences exception doc now
  names the page rule (fixed); C7 suggestion dropped.
