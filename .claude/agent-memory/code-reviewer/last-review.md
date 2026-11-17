---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice EC5 (the Encounters page, decision 1006), last slice of the plan.
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion: MainViewModelEncounterTests `ReadEncounterChoices_ImageAddedToTheFolderAfterStart_IsOffered` writes the PNG before `Start()`; rename "AfterTheEditorOpened" — open
- Suggestion: `EncounterEditing_TestWorld_StartsOnItsEncountersInIdOrder` pins TestWorld's four ids and the name-order test's `Take(3)` assumes no test-world enemy sorts before "Goblin"; assert relative order of needed ids — open
