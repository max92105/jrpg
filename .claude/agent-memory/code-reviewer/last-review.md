---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL3 (first part; undo step split to CL3b): Party/ClassCatalogViewModel,
ClassViewModel.DuplicateOf, ClassCatalogViewModelTests (22). 2026-11-17. Round 1. Verdict: Ready to commit.

- Suggestion: Duplicate's skills copy (slice goal) untested; ClassFixtures.Scholar has no skills — open
- Suggestion: move the test's `_archer` into ClassFixtures before CL3b's CatalogDraftClassTests builds a second one — open
- Carried from CL2b: Id/Name PropertyChanged tests — fixed (ClassViewModelTests PropertyChanged_* tests)
- Carried from CL2b: CL2.md Goal describing CL2b's class view model — fixed (Goal now points to CL2b)
