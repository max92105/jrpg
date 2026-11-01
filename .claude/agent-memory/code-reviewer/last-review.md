---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-enemy-skills-and-bosses, slice D1 (EnemyReferences, EnemyTacticsValidator, loader wiring). Round 1.
Verdict: Ready to commit.

- Open (suggestion): EnemyCatalogValidatorTests._goblinTable now holds an EnemyReferences, not a table; rename _references
- Open (suggestion): EnemyTacticsValidatorTests._cleave is a field used once, inside _references; inline it in the array
- Open (follow-up, from T1): new Dictionary<string, SkillDefinition>() repeated 9 times in EnemyTacticsTests; a small helper
- Fixed (from T1): EnemyTargeting.ForSkill no longer draws for a Targeting.Self skill
