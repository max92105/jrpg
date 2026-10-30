---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice G1 (Core/Party/Purse, PartyState.Purse, PurseTests). Round 1.
Verdict: Ready to commit. verify.ps1 all pass (2916 tests).

- Suggestion: CanSpend(-1) is true while TrySpend(-1) throws; guard it the same way or document it - open
- Suggestion: PartyState.Purse summary "(the gold and shops plan)" names a plan that moves to done/; say what it holds instead - open
- Suggestion: no test that adding exactly up to MaxGold loses 0 (the plan's "both bounds tested"); Add_PastMax pins the arithmetic meanwhile - open
