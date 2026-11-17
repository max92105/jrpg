---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL6 first part (split; commands, picker and XAML are CL6b):
Catalogs/RecordChoice, Party/ClassSkillRow, ClassViewModel.Skills partial; decision 1014.
2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Checked: SkillsAsTyped keeps _source.Skills while SequenceEqual (ClassSkill is a record) so an untouched class
  equals its record; row LevelText -> class Problem + Changed once (not on the row's Problem notice); DuplicateOf
  makes new row objects; missing ids kept by ToClass for Data to name.
- Suggestion: no test that a missing-skill row survives ToClass and Data's unknown-skill message reaches the list — open
- From LV3: VictoryScenarios builder — fixed (tests/Jrpg.Game.Tests/Battle/VictoryScenarios.cs); SinceLastLevelUp
  doc — fixed; five-cards overflow and VictoryScreenView 281 lines — dropped (backlog.md lists them); LV3 suggestions
  not carried.
