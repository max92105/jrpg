---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice BT2b: Editor `Balance/TierBalanceRows`, `TierBalanceRow`, the three
Power boxes on `BalanceTableViewModel` (0 to `MaxTypedPower` 10000, decision 1022). 2026-11-18. Round 1. Verdict: Fix
the Criticals, then commit.

- Carried from BT2, fixed: `EnemyDefinition.NamedSkillIds` is in Core, Data's validator and `NamesSkill` read it.
- Carried from BT2, fixed: `Typed()` is now `ReadBoxes()`.
- Critical, open: `BalanceTableViewModel.AllRead` reads as a bool predicate but returns `int[]?`; rename `ReadAll`.
- Suggestion, open: the "damage, fire" kind spelling is in `TierChoice.Line` and `TierBalanceRows.KindLineOf`; share it
  on a third copy.
- Suggestion, open: merge the two Power refusal theories into one `BoxesProblem_PowerRefused_...` (test file at 285).
- Suggestion, open: `MaxTypedPower` comment's "past its 1,000,000 reads as that clamp" is vague.

Checked: amounts 6/9/120, 11, 30 match `DamageFormula.TieredAmount` (long product, clamp 1,000,000); Data's
`EnemyCatalogValidator.MostPower` is 10_000; Data limits tier kinds to damage/restoreHp, so `PerHit` can't throw.
