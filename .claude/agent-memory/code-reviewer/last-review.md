---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice SL (Slowed: statChange + StatusPart.Stat, ChangeableStats,
StatusDefinition.MarkLetter, validator rules, cone_of_cold). 2026-11-16. Round 1. Verdict: Ready to commit.
No Critical, no Warning.

- Suggestion, open: StatusCatalogValidatorTests Validate_OneFamilyWithAnotherStat_Throws carries the comment
  "Decision 797: a stat change is listed once per stat", which describes the duplicate-stat row, not the family shape.
- Suggestion, open: StatusMarksViewTests Refresh_StatusWithMarkLetter_ShowsIt splits `with` / `{ MarkLetter = "L" };`
  over two lines (formatter layout); a local then a one-line `with` reads better.
- Suggestion, open: BattleSlowedTests has no unslowed twin proving draw 800 hits at 90 %; rests on TestStatRules.Neutral
  BaseAccuracy 100.
- Fixed (from WK): ModifiedFlat summary now says "any flat damage"; Enfeeble loader comment no longer says warlock's.
- Dropped (from WK): ModifiedFlat MaxPower is in docs/plans/backlog.md (line ~289).
- Checked: dodge clamp at 0 change is a no-op (member dodge capped in StatFormula, enemy dodge by EnemyCatalogValidator);
  Strengthened and StatusSet.Refresh's Zip keep Stat (part `with`); validator refuses a stat on any other kind even when
  the (kind, stat) key lets a second damageEachTurn through; CurrentStats is Core-only, not per frame.
