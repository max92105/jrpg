---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice MV1 (moveNpc step in Core: MoveNpcStep, NpcWalk, NpcPose,
ActiveMap walks, EventRun move case + ApplyAtOnce, NpcFacing.Of over a pose, ExplorationState.NpcPoseOf/AdvanceNpcWalks).
2026-11-12. Round 1. Verdict: fix the Criticals.

- Critical (open): skip past two move steps of one NPC: second StartWalk reads PoseOf = the unadvanced first walk's
  start (ActiveMap.StartWalk), so [still, move Left, move Down] skipped lands (3,2) not (2,2). Start from the previous
  walk's End, or Finish it first; add a skip test with two moves.
- Critical (open): MoveNpcStep.Equals (path by content) has no test; GearPiece/SavedGear precedents do.
- Warning (open): NpcWalk ctor `break` cuts the rest of the path at the first off-map step, docs (NpcWalk summary,
  decision 578, test name Start_StepLeavingTheMap_IsDropped) say "a step is dropped".
- Warning (open): ActiveMap.IsAnyNpcWalking has no caller in MV1-MV3 (speculative; LINQ boxing).
- Suggestion (open): NpcWalk ctor first `Facing = facing;` dead assignment.
- Suggestion (open): ActiveMap.PositionOf unused and untested until MV2; move it there or test it.
- Suggestion (open): EventRun class summary lists what waits for Continue; a move step now waits and continues itself.
- Follow-up: EventRun keeps the ActiveMap it started on; a move after a warp walks on the left map and the event goes on
  next frame (MV2 validator: refuse a move after a warp, or follow the map).
- Follow-up: ExplorationState.ContinueEvent on a MoveNpcStep advances past a walk under way; MV2 presenter must not call it.
- NB3 suggestions: not carried (NB3 committed, suggestions only).
