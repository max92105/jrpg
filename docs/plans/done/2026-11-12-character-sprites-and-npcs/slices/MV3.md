# MV3. The step in the editor

Editor, about 210 lines (100 of them tests). Decisions 573, 579. Builds on MV2; its hand check uses SI5's NPCs.

## Goal
The event dialog offers **Move NPC**: an NPC box listing the placed NPCs of the event's map (object id and uid), the path built with four direction buttons and **Remove last**, shown as text ("left, left, down"), the problem line when the box is empty or the path is; the step's line in the steps list reads "Move villager_anna (npc_3): left, left, down".

## New types
- `Editor/Events/NpcChoice` (internal sealed record): one entry of the NPC box: uid, object id, `Label` ("villager_anna (npc_3)").
- `Editor/Events/EventStepViewModel.MoveNpc.cs` (partial; `EventStepViewModel.cs` is at 265): `MoveNpcUid`, `MovePath` (an `ObservableCollection<Facing>`), `MovePathText`, the commands Add up / down / left / right and Remove last, `ToMoveNpcStep()`, `MoveNpcSummary()`, `MoveNpcProblem`.

## Changes
- `Editor/Events/EventStepKind`: `MoveNpc`.
- `Editor/Events/EventStepViewModel`: `ToStep`'s switch and the summary's switch each gain one arm calling the partial's method (no logic in the arm, so neither switch passes CA1502: if one is already at the limit, its arms for warp and move go through one `ToMappedStep()` helper, named here so the slice doesn't discover it); `From` reads a `MoveNpcStep`.
- `Editor/Events/StepChoices`: `NpcChoices` (init, empty by default): the event's map's placed objects whose object has a field sprite, built by `EventEditingViewModel.Choices()` from the open map as it is now (unsaved placements included).
- `Editor/Events/EventStepEditor.xaml`: the Move NPC fields, visible for the kind.
- `Editor/Events/MapEventComparison`: a `MoveNpcStep` compares by uid and path.

## Tests
- `EventStepMoveNpcTests` (new sibling): `From_MoveNpcStep_ReadsUidAndPath`; `AddLeft_Twice_PathTextReadsLeftLeft`; `RemoveLast_EmptyPath_IsGreyed`; `MoveNpcProblem_NoNpcChosen_AsksForOne`; `MoveNpcProblem_EmptyPath_AsksForAStep`; `ToStep_MoveNpc_RoundTrips`; `Summary_MoveNpc_NamesObjectAndPath`.
- `EventEditingViewModelTests`: `Choices_MapWithNpcAndChest_OffersOnlyTheNpc`.
- `MapEventComparisonTests`: `AreSame_MovePathsDiffer_IsFalse`.

## Verify by hand
1. Place two NPCs on a map (SI5). Open the first's events (Events mode), add an event step of kind Move NPC: the box lists both NPCs; choose the second, click Left, Left, Down: the path reads "left, left, down"; Remove last: "left, left". OK.
2. Add a dialogue step after it, OK, Ctrl+S, play: talking to the first NPC makes the second walk two cells left, then the first speaks.
3. Ctrl+Z in the editor before saving removes the step with the event change.
4. Revert with `git checkout -- content` if this was only a check.

- **Done, with deviations:** the step's line reads "Move npc_3: left, left, down" (the uid alone: a loaded step knows no object id, and the box shows "villager_anna (npc_3)"); the four direction buttons and Remove last grey at the bounds (no step past 64, nothing to remove on an empty path); `MapEventComparison` needed no change, since `MoveNpcStep` compares its path by content (MV1), pinned by `MapEventComparisonTests`; the cell event dialog offers Move NPC too, as Data allows it.
