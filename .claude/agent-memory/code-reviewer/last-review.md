---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/M1b.md, slice M1b (resources load from content, mage has mana, playback accepts ResourceChanged)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical, open: ResourceDefinitionValidator.cs:10 `"resources": [null]` -> NullReferenceException on `resource.Id`, no file name; check null entries first (like FindSkillProblem), and test null + blank id (empty-id branch has no test).
- Suggestion, open: ResourceDefinitionValidator.cs:16 duplicate check could say "resources list 'mana' more than once" to match skillIds wording; taste.
