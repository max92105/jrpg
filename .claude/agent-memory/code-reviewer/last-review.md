---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice XP2 (first part; XP2b holds Add/Remove level and the max level note).
2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Checked: TotalToReach sums in long? (lifted, null propagates down); ExperienceTable record equality is by list
  reference, but SetExperienceTable compares with SequenceEqual, tested by SameList and Apply_Untouched.
- Suggestion (open): ExperienceFixtures' Classes override is redundant (ClassFixtures.World already holds only the
  scholar, no skills, and the archer) and its stated reason (no repository class) doesn't hold; drop it or reword.
- XP1 Suggestion (item-before-rules order in XP1's Done note): not carried, outside this slice.
