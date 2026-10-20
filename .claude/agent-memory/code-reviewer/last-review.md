---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B1b.md, B1b (random battles start; Run ends them)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1153 tests)

- open (Critical): StartMapScenarios.cs ~line 29-35: the BelowClosedDoor summary was left above NextToForestWarpAlwaysRolling's (two summaries on one property, none on BelowClosedDoor).
- open (Warning): GameFlowTests Update_WalkIntoForestThatAlwaysRolls_FightsAndReturnsToMap asserts only what the loop exit and Core already guarantee; a no-op BattleTransitions.Finish passes. Assert the map takes input after the fade (hold Left, position changes).
- open (Suggestion): Update_EncounterPending_DoesNotWalk passes without the scene's guard (Core refuses walking); the guard's real job (no Menu / no repeat BattleRequested while pending) could get a Menu-pressed test.
