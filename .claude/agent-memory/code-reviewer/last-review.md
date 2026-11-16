---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice SK2 (CatalogProblems.OfSkills, CatalogDraft.SetSkills,
SkillViewModel; code uncommitted). 2026-11-16. Round 1. Verdict: Ready to commit. No findings.

- SK1 carry-overs, all fixed (checked in the tree): writer refusal tests for skills/statuses/effects.json exist
  (ContentWriterSkillTests); the Effects_/Statuses_ repository tests are gone; CheckSkills doc rewritten.
- Checked, no finding: duplicate ids reach ContentListValidator before either ToDictionary in CheckSkills/OfSkills;
  Effect overrides Equals (grants by content), so IsSameSkill's fresh-lists test is meaningful; the enemy test
  discriminates (with content's skills it would say unknown skill); Name.Trim matches Enemy/Shop view models.
