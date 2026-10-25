---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/CS1.md, slice CS1 (skills come from the class)
Round: 1 (verdict: Ready to commit)

- Warning, open: ClassCatalogValidator.cs FindSkillProblem: `skillIds: ["heal", null]` -> FrozenDictionary.ContainsKey(null) throws ArgumentNullException ("Parameter 'key'"), no file name; check null/blank entries first.
- Suggestion, open: CommandMenuTests.cs:22 uses BattleScenarios.SarxianFirstSeed for a Zanyr battle; name/doc no longer matches.
- Suggestion, open: Skill-definition test fixtures duplicated in PartyMemberTests and ClassCatalogValidatorTests (no SkillBuilder exists yet).
