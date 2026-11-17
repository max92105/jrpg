---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice LV2 (SkillsByLevel, LearnedSkill, PartyMember.Skills by level,
SkillsLearnedAt, ExperienceGain.LearnedSkills filled by BattleRewards.LearnedOn; ADR 0062).
2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Checked: `step.Level + 1` is right (PartyMember.cs:226 builds the step with Level before LevelUp at :230; the
  two-levels test would read (1,spark),(2,flame) if off by one); level-1 skills never re-listed (LearnedAt exact
  level, steps reach >= 2); Skills keep class order (test class lists meteor(3), spark(1), flame(3)); Combatant.cs:62
  snapshots Skills at battle start, Game's BattleSkillChoice/StatusSheet read fresh.
- Suggestion: PartyMember.SkillsLearnedAt public with only a Core reader (BattleRewards); internal like RestoreProgress — open
- Follow-up: SkillDefinition test factory copied in ~29 test files (2 more in this diff); a Builders/SkillDefinitionBuilder — open
- Follow-up: eight stat labels in page order in ClassStatsBoxes.Problem and ClassLevelPreview.StatsOf — open (from CL5, grep 2026-11-17 still present)
- From LV1: CatalogProblems.OfClasses level-check test Suggestion — not carried (other slice's taste)
