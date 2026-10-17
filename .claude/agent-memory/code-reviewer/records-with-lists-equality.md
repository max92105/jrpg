---
name: records-with-lists-equality
description: Recurring in Core/Data reviews - records holding IReadOnlyList/dictionaries get reference equality for those members; also combined multi-field validator messages and tautological "doesn't throw" tests
metadata:
  type: feedback
---

Records that carry lists or dictionaries (`ExperienceTable`, `EquipmentStats.AllowedClassIds`, `ContentCatalog`, `GameRules`) compare those members by reference. Flagged in the slice 2 and 3a reviews and again in 4b (copying the list in made two tables from equal data unequal).

**Why:** A `record` promises value equality; a silent reference compare surprises the next test or cache. The plan's agreed workaround is "compare definitions by Id".

**How to apply:** For any new record with a collection member, check whether equality is used anywhere. If not, a Suggestion is enough. If a test or lookup compares them, it's a Warning.

Also watch for, seen in 4b: validator messages that combine two fields ("attack and defense must be...") when the item validator uses one field per message. And loader tests whose only assertion can't fail (`Select(...).ToList().Count == source.Count`).
