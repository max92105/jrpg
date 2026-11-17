---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice LV3 (victory screen's "X learned Y." lines: LearnedSkillLine.AllOf,
VictoryLearnedSkillsView, VictoryScreen.OpacityOf; decisions 964, 1013).
2026-11-17. Round 1. Verdict: Ready to commit. No Critical.

- Checked: fade vs a later level-up resetting SinceLastLevelUp is right (only the line whose LevelsToFill equals
  LevelUps reads the timer; OpacityOf_FirstLevelSkillJustAfterTheSecondFill_StaysWhole at 3.3 s discriminates); skip
  nulls SinceLastLevelUp -> 1; LevelsToFill = learned.Level - Steps[0].Level right incl. a gain ending on a level up.
- Warning: third victory test file copies Sarxian/half-a-level/spoils builders; backlog.md:145 says share them at the
  third file -> a VictoryScenarios test builder — open
- Warning: five cards fit two lines; a third crosses FooterLineTop (910), 4th+ under the footer bar; unreachable today
  (no skill in content/classes.json above level 1); not in backlog.md -> add a backlog line naming the options — open
- Suggestion: one card -> lines centered between both columns, off the lone left card — open
- Suggestion: Update_NothingLearned_AsksNothing has no level up either, so it repeats the existing no-sound case — open
- Follow-up: backlog.md:86 VictoryScreenView "277 lines" now 281; its split trigger is met — open
- Follow-up: ExperienceBarAnimation.SinceLastLevelUp doc says "for the pop"; now also the learned line's fade — open
- From LV2: SkillsLearnedAt Suggestion — fixed (internal, PartyMember.cs:110); SkillDefinition factory and stat-label
  follow-ups — dropped (backlog.md:347, :349 list them)
