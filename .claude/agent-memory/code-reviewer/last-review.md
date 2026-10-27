---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/L1.md, slice L1 (StatNames/StatText, StatFormula.WholePercentOf, leader choice shows LV, HP, speed, five primaries in two columns)
Round: 1 (verdict: Ready to commit; verify all pass, 2401 tests)

- S1 src/Jrpg.Game/Ui/StatText.cs:18 IsPercent is a Game-side "not points" list, so a new StatKind defaults to percent; the unit is Core's fact (StatSheet holds permille) -> StatKinds.IsHeldInPermille in Core; open (suggestion)
- S2 tests/Jrpg.Core.Tests/Stats/StatFormulaTests.cs WholePercentOf_NegativeHalfPercent_... input is -25.5 %, not -0.5 %; rename e.g. WholePercentOf_MinusTwentyFiveAndAHalfPercent_IsMinusTwentySix; open (suggestion)
- S3 src/Jrpg.Game/NewGame/LeaderProfileView.cs:33-36 two-columns comment runs into the font-size constants with no blank line; open (suggestion)
- F1 P4b's W1-W3/F1/F3 status not rechecked here; P4b F2 (LeaderProfile second max-HP path) fixed by this slice
- F2 stat_attack/stat_defense still in ui.json until E1/S1 (planned, M2b checks); follow-up
