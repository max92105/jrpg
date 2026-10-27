---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/S1a.md, slice S1a (Status page Combat, Resistances and Resources: StatusSheet.Combat/Resistances, StatusParametersView, DamageTypeNames; S1 split into S1a/S1b on 2026-10-27)
Round: 1 (verdict: Ready to commit; verify all pass, 2424 tests)

- W1 docs/map.md Menus line: StatusSheet/StatusSheetView still described as "profile, speed, parameters, equipment, resources, skills ... spaced-caps labels beside large values" -> "profile, Combat, Resistances, Resources, skills", drawing words move to StatusParametersView; open
- S1 src/Jrpg.Game/Menus/StatusParametersView.cs:17 Height assumes at most 2 resources (right column 312 of 318); a third class resource draws over Skills -> derive Height from the most resources, or a test over content classes; open (suggestion)
- S2 src/Jrpg.Game/Menus/StatusSheetView.cs:43 SkillsHeight repeats StatusSkillsView's "+ 10" band offset and guesses 70 px a skill -> read them from StatusSkillsView; open (suggestion)
- S3 src/Jrpg.Game/Menus/StatusParametersView.cs:31-33,135 LabelPixels/ValuePixels/LabelSpacing and Frame copy StatusSheetView's -> one shared source for the page's label/value fonts; open (suggestion)
