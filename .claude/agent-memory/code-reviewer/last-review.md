---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB8 (priority rules: EnemyAction.IsPriority, EnemyTactics.Pick, the
AbsentWhenFalseFilter row, TacticRuleRow/TacticListViewModel/EnemyTacticsView check box). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): EnemyTacticsView.xaml Priority tooltip says "While its When holds"; eligibility also
  needs a usable skill and, with On, someone to aim at; "While it can be chosen" or "While its When holds and it can
  act".
- Follow-up: TacticRuleRowTests.cs now 280 lines; the next tests there (EB9/EB10 flags) need a split.
- Fixed: EB7's map.md suggestion (EnemyTacticsView has its own entry after EnemyCatalogPage's).
- Dropped (in backlog.md): EnemyCatalogViewModelTests at 299 lines; VisibleWhenTrue converter in 10 views.
- Checked, no finding: only TacticRuleRow builds EnemyAction in src (no field-by-field copy loses IsPriority); every
  Core test's FixedSequenceRandom runs out on an unwanted draw (phase test: the start list's priority strike would
  draw for a single target); record equality covers IsPriority for the Ctrl+Z hand check.
