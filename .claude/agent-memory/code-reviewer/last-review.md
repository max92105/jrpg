---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, M2 (Warps with a fade; random battles off)
Round: 1 (verdict: Ready to commit; build 0 errors/warnings, 860 tests pass, format exit 0)

- open (Suggestion): ScreenFade._inPhase reads as a bool; rename _timeInPhase
- open (Suggestion): record the same-map-warp limitation (camera eases instead of snapping, same MapDefinition reference) in the M2 slice note so later slices don't rediscover it
- follow-up: content/map-layouts/forest.json has no cellEvents, so the M2 hand check "Walk back" (and the plan goal "to the forest and back") can't be done with current content
- follow-up: M2 checkbox and hand checks in the plan not ticked yet (DoD 5)
- verified: fixed timestep (JrpgGame IsFixedTimeStep, 1/60) means a frame always lands in the 0.08 s Holding phase, so ContinueEvent can't be skipped; forest (0,1) is walkable so ExplorationSceneTests can fail if IsBusy stops blocking; M1 suggestion "sprite freezes on stride frame" now fixed (Stand while busy)
