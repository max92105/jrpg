---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice LV1 (ClassSkill entries in classes.json, ClassSkillLevelCheck,
validator null/level-0 checks, messages say `skills`, decision 1012; ADR 0062).
2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Checked: classes.json's nine classes keep the same ids in the same order, no `level`, no stale `skillIds`
  (PowerShell ConvertFrom-Json compare against HEAD); a stale `skillIds` is refused (ComputedPropertyFilter removes
  the getter-only property, UnmappedMemberHandling.Disallow); ClassViewModel.ToClass keeps Skills through `with`.
- Suggestion: CatalogProblems.OfClasses's new CheckClassSkillLevels call (the editor's Classes problem line) has no
  test; CatalogProblemsPartyTests could add a level-past-max row — open
- Not a finding (drop): LeaderProfile.cs:27 calling `ClassSheet.At(class, 1, rules)` IS the CL5 fix (the shared rule), not an inline copy.
- Follow-up: eight stat labels in page order in ClassStatsBoxes.Problem and ClassLevelPreview.StatsOf — open (from CL5)
- From CL5b: ItemsControl ScrollViewer/IsVirtualizing no-op attributes Suggestion — not carried (other slice's taste)
