---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice EN3 (Editor/Battle: ResistanceRow, EnemyViewModel.Rewards partial,
EnemyCatalogViewModel.DropTableChoices; percents and gold kept as typed strings, drop table box bound to the catalog's
choices: developer decisions, don't re-raise). 2026-11-13. Round 1. Verdict: fix the Critical, then commit.

- EN2 Critical (DuplicateOf through ToEnemy): fixed (DuplicateOf copies over chosen._original).
- EN2 Suggestion (catalog reacting to Label/Problem): fixed (OnEnemyChanged skips them).
- EN2 Suggestion (TypedNumber in Battle/): dropped, backlog.md line 208 records it.
- Critical (open): EnemyViewModel.Rewards.cs names (partial `.Rewards`, ReadRewards, CopyRewardsFrom, WithRewards,
  FirstRewardProblem) also cover resistances and IsBoss, which aren't rewards; FirstRewardProblem returns
  "Ice resistance ...". Fix: rename to ResistancesAndRewards, or split a `.Resistances` partial and move IsBoss.
- Suggestion (open): DropTableChoices allocates a new list per get; compute once in the constructor.
- Suggestion (open): ResistanceRow.Problem never notified (PercentText lacks NotifyPropertyChangedFor); EN7 binding.
- Suggestion (open): Resistances `{ get; private set; } = []` set once; get-only assigned in the constructor.
