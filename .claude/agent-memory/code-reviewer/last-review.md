---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 4a (Warps and the new game's start location)
Round: 1 (verdict: Fix the Criticals, then commit)

Accepted deviations (don't re-raise): WarpArrival searches from ring 0 (the clamped cell); GameSession.World not added until 5a; facing kept across a warp. All three need a note in the plan's slice entry.

- open (Critical): ExplorationWarpTests.ContinueEvent_WarpOntoTouchCell_DoesNotFireArrivalCell lands on town (3,2), which has no cell event (the town warp cell is (3,3)), so it can't fail. Make the cave warp land on a touch cell.
- open (Warning): bounds check written again. WarpArrival `isOnMap` is redundant (IsWalkable(targetCell) already returns false off the map), and NewGameValidator's isInside copies MapLayout.Contains(GridCell.Containing(position))
- open (Suggestion): ContentCatalogBuilder's DefaultStart points at "test_map", which WithMaps/WithTiles drop silently, so StartNew on such a catalog throws KeyNotFoundException
- open (Suggestion): Find_TargetOffMapNextToBlockedEdge_SearchesFromClampedCell would also pass under legacy ring-1; the ring-0 deviation is pinned only by Find_TargetOffMap_ClampsBeforeSearching. Rename one to say so.
