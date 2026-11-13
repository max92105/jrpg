---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice MV2 (moveNpc in content and on screen: MoveNpcStepValidator, the
warp rule in MapEventValidator, ActiveMap.PositionOf read by MapCollision/InteractionFinder, MapView.DrawNpc,
InteractionPromptView pose lookup, ExplorationScene.AdvanceNpcWalks). 2026-11-12. Round 1. Verdict: ready to commit.

- MV1 findings: all fixed (skip chain finishes the previous walk, MoveNpcStepTests equality, IsAnyNpcWalking gone,
  PositionOf now used, presenter never ContinueEvent on a move, move after warp refused).
- Warning (open): a walk ending on the player's centre traps the player: MapCollision.IsInsideAnObject now reads the
  walk position, PlayerWalker.TryMoveBy (line 78) refuses every target still inside the polygon. Villager (12,4), polygon
  x .2-.8 y .5-1, player (12.5, 5.6) facing up, path [down] -> stuck until load. Design call: ask the developer.
- Suggestion (open): MapView.DrawNpc re-reads currentDefinitionOf and uses `!`; MapView class summary still says only
  "facing what Core says".
- Suggestion (open): InteractionPromptView passes every object (chests too) through npcPoseOf for a position.
- Suggestion (open): ExplorationSceneMoveNpcTests.Update_UntilArrived_ShowsTheDialogueAfterTheMove asserts Core's step,
  not the presenter showing it.
- Follow-up: editor EventStepViewModel.From throws on a moveNpc step until MV3 (planned).
