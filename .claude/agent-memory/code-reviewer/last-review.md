---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB12 (TacticTarget.UnafflictedFoe, TacticTargetCandidates.For takes the skill,
EnemyTactics.SkillOf, EnemyTargeting.ForRule one draw after the taunt, TacticTargetFit.GrantsHarm, Data
FindTargetProblem harmful-grant message, editor words). The plan's last slice. 2026-11-16. Round 1. Verdict: Ready to
commit (no Critical, no Warning).

- Suggestion (open, optional): EnemyTacticRuleValidatorTests.PoisonSkill builds Skill(id, SingleEnemy) then replaces
  its whole Effect; build the record once (a grants parameter on Skill).
- Suggestion (open, optional): no Data test pins the attack with unafflictedFoe ("(attack).on: unafflictedFoe needs a
  skill that grants a harmful status.").
- EB11 Warnings (EveryThirdTurnOnItsFourthTurn test, TacticRuleRow "Every N turns of its own") fixed: both in the tree.
- Checked, no finding: fallen foes never legal (TargetRules.CanTarget SingleEnemy needs IsAlive); poison_2 held vs
  poison_1 granted matches by FamilyKey; boon grant ignored; draws exact (FixedSequenceRandom throws on an extra draw):
  no taunt 1 draw, taunt 1 roll + 1 draw; Data's singleEnemy branch equals Core's !GrantsHarm today (only reached when
  Fits is false); content has no family mixing harmful and boon tiers.
