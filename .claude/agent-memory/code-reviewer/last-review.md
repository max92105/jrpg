---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU15 (first part; IU15b = page, IEditedCatalog, menu line, opener).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical).

- Warning (open): AffixCatalogViewModel.Problem / CatalogProblems.OfAffixes check the pool alone, but a unique's value
  must stay on its affix's side of zero (UniqueItemRules.FindSignProblem, decision 866): flipping `power` to -5..-1
  passes the page, records the step, and Save all is refused on items.json. Summaries in both claim "a unique's values
  don't depend on the ranges". Fix: check the draft's uniques against the edited pool after OfAffixes, or a plan note.
- Checked, no finding: AffixDefinition.Equals/GetHashCode consistent; TickedKinds safe with equal-count/All check;
  ContentWriter test discriminates the order (+7 vs range 5..-1 refused by items check); WithFixedStatsFrom's
  pool[...] safe because saved affixes can't be removed (id editing locked by IU15b's page, while IsNew).
- IU14 Follow-up (AffixDefinition Equals): fixed. IU14 Suggestions: not carried.
