---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice FX1 (Data/Content/CatalogReferences.Rebound,
CatalogProblems.OfTieredEffects, CatalogDraft.SetTieredEffects; code uncommitted). 2026-11-16. Round 1.
Verdict: Ready to commit.

- Warning (open): CatalogProblemsSkillTests.OfTieredEffects_RepositoryEffects_IsNull reads the test world, not the
  repository; rename OfTieredEffects_TestWorldEffects_IsNull (the slice's planned name was the wrong one).
- Suggestion (open): C7 test asserts only after redo; add the state after the two undos (Bolt named Bolt, Fire1).
- Suggestion (open): CatalogReferences' exception doc gives only the undo reason; OfTieredEffects also relies on the
  Effects page removing only added records (FX2).
- Confirmed: enemies (EnemyAction.SkillId) and classes (SkillIds) hold ids; only skills' and items' Effect hold
  resolved records, so Rebound covers every CatalogReferenceConverter user. Undo order with SetSkills is sound.
- SK7 items: NoChoices renamed PoisonOnlyChoices (fixed); SkillGrants.Problem now SectionRowProblem.FirstOf (fixed);
  TypedRecord comment suggestion not carried.
