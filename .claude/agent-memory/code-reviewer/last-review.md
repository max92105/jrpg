---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice T3 (EnemyDefinition.Power, Combatant.FromEnemy copy,
AbsentWhenZeroFilter power, EnemyCatalogValidator.FindPowerProblem, EnemyCatalogValidatorPowerTests). 2026-11-15.
Round 1. Verdict: Ready to commit.

- Suggestion (open): BattleTieredSkillTests class header still says "against one goblin ... The shuffle (1) keeps the
  member first"; the new enemy test uses shuffle 0 and the enemy first (its own comment says so).
- Checked: editor EnemyViewModel.ToEnemy builds from `_original with`, so a hand-written power survives an editor save;
  CombatStats.Power is read only by EffectScaling's tier path, so an enemy's basic attack is unaffected; "(was 0)" is
  only reached at Power 0 after the range check.
- T2 items dropped: suggestions on a committed slice; FindDamageTypeProblem follow-up is in backlog.md (line 277).
