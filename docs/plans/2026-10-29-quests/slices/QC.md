# QC. Events gated on a quest's status

Part of [the quests plan](../README.md). After QS and the ADR on where conditions live (Q1). Q4's condition kinds. Design: [design.md](../design.md), "Key decisions" (one gate), "Replacement mapping", "Risks" (`ExplorationState` at 296).

- **QC. Events gated on a quest's status, through one start gate** (Core, about 190 lines)
  - **Goal:** an event with a quest condition runs only while the quest is in that status (at that stage, when one is named), so an NPC's lines change as a quest advances; the three copies of the "may this event start" check become one.
  - **New types:**
    - `Core/Quests/QuestCondition` (public sealed record): the quest status an event waits for. `QuestId`, `Status` (`QuestStatus`), `StageId` (optional, only with `Active`: at that stage rather than any). `internal bool IsMetBy(QuestLog log)` (internal: `QuestLog` is).
  - **Changes:**
    - `Core/Events/MapEvent.OnlyWhenQuest` (`QuestCondition?`, init), with a summary beside the other two gates.
    - `Core/Exploration/ActiveMap.CanStart(MapEvent mapEvent, string leaderId)`: the leader gate, not a "once" event already done, and the quest condition met (the world's `Quests`).
    - `InteractionFinder.FindInFront` (keeps `CanRunOn`), `ArrivalEvents.IsRunnable` and `ExplorationState.TryStartCellEvent` call it instead of their own checks; `ExplorationState` must not grow (296 lines). Their summaries name the quest gate.
  - **Tests:**
    - `QuestConditionTests: IsMetBy_NotStartedCondition_MetBeforeStartOnly`
    - `QuestConditionTests: IsMetBy_ActiveWithoutStage_MetAtEveryStage`
    - `QuestConditionTests: IsMetBy_ActiveAtStage_NotMetAtAnotherStage`
    - `QuestConditionTests: IsMetBy_CompletedCondition_NotMetWhileActive`
    - `ActiveMapTests: CanStart_QuestConditionUnmet_ReturnsFalse`
    - `ExplorationQuestTests: TryInteract_TwoEventsGatedOnStages_RunsTheCurrentStagesEvent` (new file)
    - `ExplorationQuestTests: TryInteract_AfterEventStartedTheQuest_RunsTheActiveEvent` (the dialogue changes as the quest advances)
    - `ExplorationQuestTests: InteractionInFront_AfterEventCompletedTheQuest_FollowsTheCompletedEvent`
    - `ExplorationQuestTests: Walk_OntoCellEventForAnotherStage_DoesNotStartIt`
    - `ExplorationQuestTests: New_ArrivalEventForQuestNotStarted_Runs` (the arrival path through the gate)
  - **Existing tests:** the leader and "once" tests of the three callers pass unchanged (the guard for the gate's refactor).
  - **Verify by hand:** nothing to see yet (no content); N1 checks it in play.
