---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL9b (`Party/StrainBoxes`, `Party/DominanceBoxes`,
`Party/ResourceRivalRuleTests`; CL9c-d follow). 2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning.
Blanks checked against `ResourceDefinitionValidator` (strain perMissingPoint >= 1, dominance bonusPercent >= 0; an
empty id is refused by Data until chosen, as decision 1016 says).

- Suggestion: `NoticesOf`/`ChangesOf` now in two test files (`ResourceRuleTests`, `ResourceRivalRuleTests`); a third
  copy (CL9c's resource tests) should move them to one shared test type — open (optional)
- (CL9's two Suggestions: the base now relays by allow list through `OnTyped`, and the subclasses read `shown` once —
  fixed.)
