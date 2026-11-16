---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB7 (Battle/EnemyTacticsView, the Enemies page's Tactics tab, Database menu
tooltip, map row). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): docs/map.md Battle row: the EnemyTacticsView clause is spliced inside EnemyCatalogPage's
  parenthesis, so "the battle sprite previewed..." now reads as part of the Tactics tab; give EnemyTacticsView its own
  entry after EnemyCatalogPage's.
- Follow-up: EnemyCatalogViewModelTests still at 299 lines; the next test there needs a split.
- Follow-up: BooleanToVisibilityConverter "VisibleWhenTrue" declared locally in 10 views; one key in EditorTheme.
- Fixed: DatabaseMenu.xaml Enemies tooltip (EB6 follow-up) reworded in this slice.
- Checked, no finding: every binding path matches TacticListViewModel/TacticRuleRow/EnemyTacticsViewModel/
  EnemyCatalogViewModel (choices are records, Of returns entries of All); rules rows hold no boxes, so no RowFocus;
  no local Style, page styles via DynamicResource; probe showed SelectedList.Rules/SelectedList.SelectedRule keeps each
  list's chosen rule across list switches (no null write-back).
