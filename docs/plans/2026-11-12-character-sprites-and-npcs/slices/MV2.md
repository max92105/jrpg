# MV2. The step in content and on screen

Data + Core + Game, about 240 lines raw (120 of them tests). Design: key decision 5, "Risks" (draw order, collision after a walk). Decisions 574, 575, 578. Builds on MV1 and FS4.

## Goal
A `moveNpc` step can be written in a map's events and is checked at load; in the game the NPC walks on screen through its walk frames at its runtime position, the prompt follows it, and after the walk it blocks and talks where it stands, for the rest of the visit.

## New types
- `Data/Maps/MoveNpcStepValidator` (internal static): a `moveNpc` step's rules, so `MapEventValidator` (236) grows by one call only: a non-null path of 1 to `MaxSteps` steps with no null entry; the uid names a placed object on the same map whose object has a field sprite; every cell along the path on the map (walls and objects allowed, decision 575). Messages name the map, the event and the step's position.

## Changes
- `Data/Content/ContentTypeDiscriminators`: `new JsonDerivedType(typeof(MoveNpcStep), "moveNpc")`.
- `Data/Maps/MapEventValidator`: its step switch calls `MoveNpcStepValidator.FindProblem` (one arm; if the switch is at CA1502, the arm joins the null-entry check's existing helper).
- `Data/Maps/MapEventSteps` and the audio and art checks: no change (the step names no file).
- `Core/Exploration/MapCollision.IsInsideAnObject` and `InteractionFinder`: read `map.PositionOf(placed)` instead of `placed.Position` (decision 574).
- `Game/Exploration/ExplorationScene.Update`: `_exploration.AdvanceNpcWalks(delta)` once per update, beside `_view.Advance(delta)`.
- `Game/Exploration/EventPresenter` (294): `MoveNpcStep` joins the `case null or OpenForgeStep or OpenShopStep` line (nothing to show; Core continues the event): no new line.
- `Game/Exploration/ObjectScreenArea.Of(Point2D topLeft, ObjectDefinition, sprites, origin)`: the position given, not read from the placed object; `MapView` passes `npcPoseOf(placed).Position` for an NPC and the frame `WalkCellAt(pose.Facing, pose.Walked)` while walking, `StandingCell` otherwise (its constructor's facing lookup becomes the pose lookup: still four parameters).
- `Game/Exploration/InteractionPromptView`: takes the pose lookup (text, definitions, sprites, poses: four parameters) and places the prompt over the NPC's runtime position; `SceneFactory.CreateExploration` and `StartMapScenarios` follow.

## Tests
- `MoveNpcStepValidatorTests` (new): `Validate_PathOnTheMap_Passes`; `Validate_PathThroughAWall_Passes`; `InvalidMoves` theory rows: empty path, 65 steps, a null step, an unknown uid, a uid of a tile object, a path leaving the map.
- `ContentCatalogLoaderMoveNpcTests` (new sibling): `Load_EventWithMoveNpc_ReadsPathInOrder`.
- `MapCollisionTests`: `CanStandAt_NpcWalkedAway_FreesItsPlacedCell`; `CanStandAt_NpcWalkedHere_Blocks`.
- `InteractionFinderTests`: `FindInFront_NpcAtItsWalkEnd_IsFound`.
- `ObjectScreenAreaTests` (re-pinned to the position parameter).
- `ExplorationSceneMoveNpcTests` (new, Game): `Update_DuringMove_AdvancesTheWalk` (the scene's update moves the pose); `Update_MoveStep_PresenterDoesNotThrow`.

## Verify by hand
1. In a map layout (`content/map-layouts/...`), give a placed NPC's talk event a step before its dialogue: `{ "type": "moveNpc", "objectUid": "<its uid>", "path": [ "left", "left", "down" ] }`, with the uid of that NPC.
2. Start the game, talk to it: it walks two cells left and one down through its walk frames at the player's pace, facing each way, then says its line; the player can't move meanwhile.
3. After the event: it stands at the new cell facing down; walk into it: it blocks there; its old cell is free; face it: the prompt sits over its head there; talk again: it walks the path again from where it stands (the path is relative).
4. Leave the map and come back: it stands at its placed cell.
5. Put the step after a still in a cutscene, skip the still: the NPC appears at its path's end.
6. Revert with `git checkout -- content`.

- **Done, with deviations:** the validator skips the null-entry and unknown-direction checks, since the content serializer refuses both for an enum list before the validator runs; the "through a wall" test is a "through an object" one, as the validator reads no tiles; a `moveNpc` after a `warp` in the same event is refused (the run keeps the map it started on, MV1's review); `ActiveMap.PositionOf` came back with its callers; `MapView.DrawNpc` draws the NPC at its pose; `StartMapScenarios.WithVillagerSaying` beside the chest's. The review found a walk ending on the player would trap them: decision 593, `MapCollision.CanMoveTo(from, to, radius)` lets a body leave a polygon already holding it, with a walker test.
