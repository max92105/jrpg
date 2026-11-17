---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU15b (Affixes page, IEditedCatalog, menu line, opener).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Carried from IU15 (Warning, uniques' side of zero vs edited pool): fixed, CatalogProblems.OfAffixes now runs
  CheckItems against the edited pool (CatalogProblems.cs:86-92).
- Checked, no finding: every page binding names a real AffixViewModel/EquipmentKindToggle member; styles BasedOn;
  Min tooltip matches AffixCatalogValidator's same-side rule; ChosenKey/Choose match ItemCatalogViewModel's;
  fixture order makes Choose tests discriminate (fierce before fireproof); Done note's line counts match.
