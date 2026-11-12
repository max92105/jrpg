# MV1. The `moveNpc` step in Core

Core, about 260 lines (140 of them tests). Design: "Core changes", key decisions 4 and 5. Decisions 573-577. Builds on NB2. ADR 0044 written first.

## Goal
An event can hold a `moveNpc` step naming a placed NPC on its map and a path of one-cell steps. When the event reaches it, the NPC starts walking (Core moves it as the Game advances time, at `rules.json`'s `exploration.walkSpeed`, facing each step's way), the event waits, and once the NPC stands on the path's last cell the event goes on by itself. The NPC stays there, facing its last step, for the rest of the visit; nothing is saved. A skip pressed on a still before it runs past the step with the NPC placed at its end.

## New types
- `Core/Events/MoveNpcStep(string ObjectUid, IReadOnlyList<Facing> Path)` (public sealed record): `IsPresentation` true (decision 576); `MaxSteps = 64`; compares its path by content.
- `Core/Exploration/NpcWalk` (internal sealed): one NPC walking its path from where it stands now (a path is relative, so a second run starts from the first's end); a step that would leave the map is dropped when the walk starts (Data checks the path from the placed cell only, decision 578): `Advance(TimeSpan delta, double cellsPerSecond)`, `Position` (between cells while moving), `Facing` (the current step's), `Walked` (time walked), `IsFinished`, `End` (the last cell's position and the last step's facing). Never allocates per update.
- `Core/Exploration/NpcPose` (public readonly record struct): `Position`, `Facing`, `Walked` (zero when standing).

## Changes
- `Core/Geometry/Facings`: `Step(Point2D, Facing)` (one cell that way).
- `Core/Exploration/ActiveMap`: the visit's walks by uid and the ends of finished ones (`StartWalk(PlacedObject, MoveNpcStep)`, `AdvanceWalks(delta, speed)`, `FinishWalks()`, `IsAnyNpcWalking`, `PositionOf(placed)` (the walk's or the end's position, else the placed one), `PoseOf(placed)`); built anew on each map entry, so the visit's state goes with it (decision 574).
- `Core/Exploration/EventRun`: `ApplyAndCheckWait` is near CA1502's limit, so the swap and voice cases first move into `ApplyAtOnce(EventStep)` (both return false; no behavior change), then `case MoveNpcStep move` starts the walk (the uid's placed object on `_map`) and waits; `SkipPresentation` calls `_map.FinishWalks()` on every move step it runs past, so each NPC lands at its path's end.
- `Core/Exploration/NpcFacing.Of`: decision 577's order: walking, then the player while its own event runs, then its finished walk's facing, then the placed facing; it takes the pose's walk facing (four parameters at most: `placed`, `speaking`, `playerFacing`, `walkFacing`).
- `Core/Exploration/ExplorationState` (about 255 after NB1): `NpcFacingOf` becomes `NpcPoseOf(PlacedObject)`; `AdvanceNpcWalks(TimeSpan delta)`: moves the walks, and when the current step is a `MoveNpcStep` whose walk has finished, continues the event (`_events.Continue()`).
- `Game/Exploration/MapView`, `ExplorationScene`: follow the rename (`NpcPoseOf(...).Facing`); the Game draws positions in MV2.

## Tests
- `FacingsTests`: `Step_EachFacing_MovesOneCell` (theory).
- `NpcWalkTests` (new): `Start_StepLeavingTheMap_IsDropped`; `Advance_HalfACell_IsBetweenCells`; `Advance_PastTheWholePath_StopsOnLastCell` (no overshoot); `Facing_SecondStep_IsItsDirection`; `IsFinished_AfterTheLastCell_IsTrue`; `Advance_ZeroDelta_Stays`.
- `ExplorationMoveNpcTests` (new, through `TestExplorationMap`): `AdvanceNpcWalks_UntilArrived_ContinuesTheEvent` (the next dialogue shows); `NpcPoseOf_DuringWalk_FacesTheStep`; `NpcPoseOf_AfterWalk_StaysAtEndFacingLastStep`; `NpcPoseOf_AfterLeavingAndComingBack_IsThePlacedOne` (warp out and in); `SkipEventPresentation_StillThenMove_PlacesNpcAtPathEnd`; `PlayerWalk_WhileNpcWalks_DoesNotMove` (the event is running).
- `NpcFacingTests`: `Of_Walking_FacesItsStep` (over the player's event); `Of_WalkedThenEventEnded_KeepsLastStepFacing`.
- `EventRunTests`: the existing tests pass unchanged after `ApplyAtOnce` (the extraction's guard).

## Verify by hand
None: Data refuses the step's kind until MV2, so no content holds it yet.
