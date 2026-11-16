---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB6 (EnemyViewModel.Tactics partial, ToEnemy writes rules, TacticsSummary gone,
re-pinned ToEnemy tests, Apply_UntouchedSession_RecordsNothing). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): ToEnemy_Unchanged_EqualsTheRecord flattens phase rules with SelectMany (a rule moved
  between phases would pass; the chief has one phase today); compare per phase with Zip as IsSameEnemy does.
- Suggestion (open, optional): "Renamed from ... in EB6" / "Re-pinned ... in EB6" test comments are history; keep the why.
- Follow-up: EnemyCatalogViewModelTests at 299 lines; the next test there needs a split.
- Follow-up: DatabaseMenu.xaml Enemies tooltip "tactics and phases stay in enemies.json" (EB7 note records it).
- Carried from EB5: shared fixture suggestion for the tactics test files: dropped (optional, not carried further).
- Checked, no finding: IsSameEnemy compares Tactics and each phase's threshold and rules by content (EnemyAction,
  TacticCondition are value records); untouched test proves lists rebuilt (NotSame) and CanUndo false; content's chief
  has "when" conditions; On round-trip pinned by TacticListViewModelTests; notification exactly [Tactics, Problem].
