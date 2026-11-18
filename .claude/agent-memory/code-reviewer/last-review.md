---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice XP4 (Experience page: Fill from a curve).
2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning.

Checked by hand: first 100, 20 %, max level 21 gives rules.json's 20 entries exactly (ending 3154); bounds at
int.MaxValue (first, growth) fit the long product (at most 2^31 * (2^31 + 100) < 2^63).

- Suggestion: `ExperienceCurve.Steps` comment says "a factor within int", but 100 + growth reaches int.MaxValue + 100:
  open.
- Suggestion: map.md Progression row's `ProblemOf` omits the "first below 1" refusal: open.
- Suggestion: Fill raises the table's Problem (Data's check) once per replaced/removed row, ~100 checks on a 98-row
  fill: open.

XP3's three Suggestions (header column width, Remove last level ShowOnDisabled, "this box or one above" tooltip):
fixed in the tree.
