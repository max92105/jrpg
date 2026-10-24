---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/K3.md, slice K3 (the intro map)
Round: 1 (verdict: Ready to commit)

- Warning: StartMapScenarios.AfterTheIntro places at (10, 3) but the real intro warp lands on (10.5, 3.5) (cell centre); doc "where every intro ends" is false; use Point2D(10.5, 3.5) and note the half-tile shift vs the old start in the slice (open)
- Warning: SaveGameRepair.RepairLocation sends a save whose map vanished to NewGame.StartLocation = intro; the once intro is done, so the party is stuck on the exit-less 6x3 map; ask the developer for the fallback (open)
- Suggestion: EveryLeaderHasOneIntro could also assert each intro's last step is WarpStep("start", (10, 3)); only Thaali's warp is exercised (open)
- Follow-up: carried from K2a-K2c: SkipEventPresentation end-of-event test; ExplorationState.cs near 300 lines; Arrive events gold in editor; StillPlayback NaN for sub-tick Duration; bare-map frame between stills (open)
