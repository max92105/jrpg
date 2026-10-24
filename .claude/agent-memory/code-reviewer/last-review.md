---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/K1.md, slice K1 (arrival events, leader-gated events)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical: ArrivalEvents' object branch (objects before cells, CanRunOn on the current definition) has no test; every ExplorationArrivalTests map puts arrivals on cells (open)
- Suggestion: no test that a non-once arrival event runs again on a second arrival (goal bullet 3) (open)
- Follow-up: Editor EventViewModel.cs:53 describes any non-Touch event as Press "{Prompt}", so an Arrive event reads Press "" (K2b) (open)
- Follow-up: ExplorationState.EnterMap doc still says "(later) a loaded save" (open)
