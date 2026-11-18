---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice BT1c (Editor `Balance/BalanceTableViewModel`: three level boxes, a
damage modifier box -99..10000 % (decision 1021), `BoxesProblem`, `ClassSkillRows` computed on read).
2026-11-18. Round 1. Verdict: Ready to commit.

- Carried from BT1b, fixed: the rows type renamed `ClassSkillBalanceRows`; `SkillLines.AmountText` total now invariant.
- Suggestion, open: `LevelProblem(int box, ...)`: `box` is a 1-based position, `boxNumber` reads better.
- Suggestion, open: test casts the view model to `INotifyPropertyChanged` though `ObservableObject.PropertyChanged`
  is public.

Checked: ClassSkillRows on read matches `Party/ClassViewModel.LevelRows`; content has ~5 classes and 29 skills, one
build is a few dozen `ClassBattleStats.At` + `EffectAmounts.PerHit` calls. Core's `TieredAmount`/`ModifiedFlat` work in
a long and clamp to MaxPower, so 100000 permille is safe; the int add in `SkillLines.AmountText` is the only int step.
