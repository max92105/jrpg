---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-enemy-skills-and-bosses, slice T1 (EnemyAction, EnemyTactics pick, EnemyTargeting.ForSkill, Combatant.Knows). Round 1.
Verdict: Ready to commit.

- Open (suggestion): EnemyTargeting.ForSkill draws NextBelow(1) for a Targeting.Self skill, a certain outcome; take the only candidate without a draw, with a test
- Open (suggestion): new Dictionary<string, SkillDefinition>() repeated at 15 test call sites; a small Tactics(draws) helper in EnemyTacticsTests and in BattleTurnSequenceTests
