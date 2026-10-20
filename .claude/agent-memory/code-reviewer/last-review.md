---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-menu-cleanup/slices/C2.md, C2 (PartyState.CanMoveToReserve/CanChangePlace; PartyPage greys To reserve, refusal notice)
Round: 1 (verdict: Ready to commit; verify all pass, 1097 tests)

- open (Suggestion): C2 checkbox in README.md:88 not ticked (DoD step 5)
- open (Suggestion): Reorder's `.Where((m, i) => ...).Any()` could be a plain index loop / `Enumerable.Range(...).Any(...)`; reads as LINQ for an index check
- open (Follow-up): PartyPage ~385 lines, split already planned in P1/P2
