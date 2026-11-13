---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice NB1 (Facings.Opposite, NpcFacing.Of, EventRun.Source,
ExplorationState.NpcFacingOf, MapView's npcFacingOf delegate). 2026-11-12. Round 1.
Verdict: fix the Critical, then commit.

- Critical (open): NpcFacing.cs:15 compares by uid only; uids repeat across maps (tree_1_9 on 4 layouts), and a run
  continues after a WarpStep (ExplorationState.cs:132-137), so a same-uid object on the warped-to map faces the player.
  Fix: compare the map too (EventRun keeps the map id it started on) or ReferenceEquals (swaps keep the record,
  ActiveMap.cs:25); add a test: own event Dialogue, Warp, Dialogue onto a map with the same uid.
- Suggestion (open): NpcFacingTests.Of_SameUidAfterSwap models an impossible case: a swap never replaces the
  PlacedObject (ActiveMap.cs:22-26 go through WorldState); fix its comment or drop it with the Critical's fix.
- SI5 items: not re-checked (different slice); SI5 Warning on the two dead sorts was the developer's call.
