---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice BT1b (class skill rows: Core `EffectAmounts.ChangesHp`, Editor
`Balance/ClassSkillRows`, `SkillLines`, `SkillBalanceRow`; the boxes split to BT1c).
2026-11-17. Round 1. Verdict: Ready to commit.

- Warning, open: `Balance/ClassSkillRows` returns `SkillBalanceRow`s while `Party/ClassSkillRow` (Classes page's typed
  row) exists in the same assembly; proposed `ClassSkillBalanceRows` (BT2's planned `EnemySkillRows`/`TierRows` follow).
- Suggestion, open: `SkillLines.AmountText` formats the long total by interpolation (current culture) beside
  `TypedNumber.Text` (invariant) for the per-hit; non-negative (clamped 0..MaxPower), so harmless today.

Checked by hand: SkillLinesTests' 17 (sum 100 - 250) differs from replacing (15) and ignoring the table (22);
Nova's levels 4, 5, 6 give "-", 48, 54 (both sides of the bound); id-order test has the mage inserted first.
