---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL3b (CL3's undo step): Catalogs/CatalogDraft.Party (SetClasses,
IsSameClass), Party/ClassEditingViewModel, CatalogDraftClassTests (5), ClassEditingViewModelTests (3). 2026-11-17.
Round 1. Verdict: Ready to commit.

- Suggestion: the "draft over test-world with Archer and Scholar" constructor is copied in both new test classes; a
  ClassFixtures member before a third copy (CL4's page tests) — open
- Carried from CL3: Duplicate's skills copy untested — fixed (DuplicateClass_ClassWithSkills_CopiesThem)
- Carried from CL3: move `_archer` into ClassFixtures — fixed (ClassFixtures.Archer)
