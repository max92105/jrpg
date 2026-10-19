---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, M1 (Walking, walk animation and camera follow)
Round: 1 (verdict: Ready to commit; build 0 errors/warnings, 850 tests pass, format exit 0)

- open (Suggestion): JrpgGame.cs:55 skipping _input.Read while inactive leaves InputReader._previouslyHeld stale; a key held when focus left swallows its first press on return. Read always, then pick `IsActive ? input : default`
- open (Suggestion): ExplorationScene.Update returns before Stand() when an event starts mid-walk, so the sprite freezes on a stride frame (until M2 shows events)
- open (Suggestion): IsPlayerMoving_PushingIntoMapEdge_IsTrue doesn't assert the position stayed put, so it doesn't prove the edge blocked
- follow-up: PendingEncounter is never consumed in M1 (forest has encounters); ThrowIfBusy only checks _run, so walking continues and rerolls; M2 turns battles off
- verified: Follow formula, rounding of Origin, WalkAnimation leftover carry, default InputFrame is valid (record structs), deviation on Core test name noted by developer
