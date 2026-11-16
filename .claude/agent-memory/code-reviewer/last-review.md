---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB3a (first commit of EB3: TacticConditionChoice, TacticTargetChoice,
TacticSkillChoice.AllIn, EnemyCatalogViewModel.SkillChoices/ConditionChoices/TargetChoices, TacticChoiceTests, two
EnemyCatalogViewModelTests). EB3b (TacticRuleRow) follows as its own commit. 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- Checked, no finding: words equal design 3.1/3.2 row for row; enum order matches Core's TacticConditionKind and
  TacticTarget; AllIn filter is EnemySkillUse.RefusalOf is null; skills are fixed for the page's life (only drop tables
  are re-taken), so building SkillChoices once is right; test 1 defeats both id order and insertion order.
- `Of` (both choices) has no caller nor test yet: EB3b's TacticRuleRow.From is its user; check it is tested there.
- EB2's suggestion (name the trigger in the subject misfit message): committed without it, treated as declined.
