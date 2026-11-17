---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU13 (unique form: Items/UniqueAffixesViewModel, FixedAffixRow,
AffixChoice, ItemForms.Unique, ItemViewModel.Unique and PieceValueLine, page Fixed affixes section). 2026-11-16.
Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): UniqueAffixesViewModel.From's pool doc comment states as present that "the Affixes page removes
  only an affix added in its own session"; that page arrives in IU15 (its Note records the requirement).
- Suggestion (open): the value box's tooltip spells Data's ±100 bound; it goes stale if Data's constant changes.
- Checked, no finding: untouched unique ToItem equals source (ItemDefinition.Equals uses SequenceEqual on
  FixedAffixes; FixedAffixConverter takes each stat from the pool, so ToFixedAffixes' pool stat matches; Duplicate test
  asserts the whole record); PieceValueLine uses PriceFormula.PieceValue/SalePrice as Core's Pricing.PriceOf(piece)
  does; MoveBy at an edge makes no Move so no Changed; Changed relays to the catalog's Problem (OnItemChanged);
  no-missing-id decision sound (loader refuses, no Affixes page yet, IU15 Note).
- IU12 Suggestions: not carried (round closed).
