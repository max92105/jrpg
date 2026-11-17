---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL5 (first part; the view model and Levels tab XAML split to CL5b):
Core Party/ClassSheet + tests, Editor Party/ClassLevelPreview, LevelPreviewRow + tests, BT1 note, map rows.
2026-11-17. Round 1. Verdict: Ready to commit.

- Suggestion: ClassLevelPreview.ProblemAt picks (size, bound) before the range check; guard first — open
- Suggestion: eight stat labels in page order now in ClassStatsBoxes.Problem and ClassLevelPreview.StatsOf — open
- Follow-up: StatFormula.MaxHp / AccuracyPermille wrap on absurd input after ProblemOf passes (tab shows max HP 1) — open
- Follow-up: Game/NewGame/LeaderProfile.cs:27 still inlines ClassSheet.At(class, 1, rules) — open
- Carried from CL4: "Per level" header vs "Growth" wording — fixed (header is "Growth (per level)")
- Carried from CL3b: ClassFixtures draft — moved to CL5b (its slice file lists it)
