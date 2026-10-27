---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/H2.md, slice H2 (critical hits: CombatStats crit fields,
DamageFormula crit factor, EffectLanding crit roll, EffectApplied.IsCritical, Critical tone 1.4x gold)
Round: 1 (verdict: Ready to commit; verify all pass, 2502 tests)

- W1 MemberBattleStats.cs:25 CritDamagePermille copy untested: Neutral rules' CritDamagePercentPer100Power 0 gives 1500 == CombatStats default, so dropping the line passes every test; open (warning)
- S1 DamageFormula.cs:165,167 empty <param name="power"> and <param name="mitigation"> tags (added only to satisfy CS1573); fill them; open (suggestion)
- S2 FloatingNumberTests SizeOf_Critical_IsBiggerThanDamage asserts two things; split or pin 1.4; open (suggestion)
