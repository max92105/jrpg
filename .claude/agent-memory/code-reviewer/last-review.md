---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice SK3 (SkillChoices, SkillUsers, SkillCatalogViewModel,
SkillEditingViewModel, SkillViewModel.New/DuplicateOf; code uncommitted). 2026-11-16. Round 1. Verdict: Ready to commit.

- Ruled: Problem in ShopCatalogViewModel's order (first wrong skill in list order) is fine, not a finding: the slice's
  own New Types line specifies Shop's; a skill's own problem is only a new skill's typed id; recorded in the Done note.
- Ruled: SK6 note correct (SkillCatalogValidator.FindTierProblem refuses untiered damage; FindDamageTypeProblem runs
  first, which is why the flat start carries a type).
- Suggestion (open, optional): AddSkill_EmptyCatalogOfTiers_StartsAFlatDamage doesn't assert the damage type the
  implementer added for Data's tier message.
- Follow-up: SK6 step 3 ("Tier none ... OK lights") contradicts Data's tier rule (decision 704); developer to decide.
