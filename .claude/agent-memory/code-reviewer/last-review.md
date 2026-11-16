---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB2 (TacticSubject, TacticConditionNumber, TacticConditionShape,
TacticConditionKinds.ShapeOf, TacticTargetFit.Fits/AimOf, EnemySkillRefusal, EnemySkillUse.RefusalOf; Data
EnemyTacticsValidator FindTargetProblem, FindConditionProblem on the shape's Number, FindUsabilityProblem mapping).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): a subject misfit's message ("subject doesn't fit attack, which aims at singleEnemy") names the
  aim but not the trigger that decided it (allyFallen); adding "after allyFallen" would tell the author why.
- Checked, no finding: fit table equals design 3.3 row by row (Subject after OwnHpBelow -> self, singleAlly;
  LowestHpOnItsSide -> singleAlly only; foes -> singleEnemy = the attack); the four existing usability/percent
  messages are pinned by EnemyTacticsValidatorTests 69/75/81/151 and CatalogProblemsSkillTests 70; skills[skillId] in
  FindTargetProblem is safe (FindSkillProblem runs first in the ?? chain); AimOf public is the right seam
  (AttackEffect internal, editor boxes will need it).
- EB1 items: both fixed (TauntingOrAll filters Side != enemy.Side; ShareOf cross-multiplies in long).
