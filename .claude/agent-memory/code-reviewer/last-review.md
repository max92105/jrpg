---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice NG2c (the new game's starting items; split from NG2b). 2026-11-17.
Round 1. Verdict: Ready to commit. No Critical.

- Warning, open: third copies with ClassViewModel.Skills and NewGameViewModel.Leaders: the "record or missing" lookup
  (StartingItemRowsOf / LeadersOf / ClassViewModel.ChoiceOf) and the add-box query (ItemsToAdd / LeadersToAdd /
  SkillsToAdd); proposed RecordChoice.Of(id, name-or-null) and a shared by-name-then-id ordering of unlisted records.
- Suggestion, open: no test that with two bad quantities the first row's is named (StartingItemsProblem's order).
- Suggestion, open: the "record's list kept while equal" check is now three two-liners (Skills, Leaders, Items).
- Follow-up, open: StartingItemRow and Party/ClassSkillRow share a shape (choice + typed number + New + ToEntry);
  share at a third copy.
- NG2b items: both suggestions fixed in this slice (notice counted once; Leaders comment); WatchRowsOf follow-up
  dropped (backlog.md lists it).
