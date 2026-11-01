# QC. Events gated on a quest

Part of [the quests plan](../README.md). After R1 and ADR 0033. Decisions 386 (the whole event), 389 (five kinds; reached is positional). Design: [design.md](../design.md), "Key decisions" (one gate; stage ids), "Replacement mapping".

- **QC. Events gated on a quest, through one start gate** (Core, about 230 lines)
  - **Goal:** an event with a quest condition starts only while the quest meets it, so an NPC's lines change as a quest advances; the three copies of the "may this event start" check become one.
  - **New types:**
    - `Core/Quests/QuestConditionKind` (public enum): `NotStarted`, `Active` (any stage), `AtStage`, `ReachedStage` (at it or past it, or completed), `Completed`.
    - `Core/Quests/QuestCondition` (public sealed record): what an event waits for: `QuestId`, `Kind`, `StageId` (named only by the two stage kinds). `internal bool IsMetBy(QuestLog log)` (internal: `QuestLog` is).
  - **Changes:**
    - `Core/Quests/QuestLog.HasReached(string questId, string stageId)`: false when not started, true when completed, else the current stage's position in the quest's stages is at or after the named one's.
    - `Core/Events/MapEvent.OnlyWhenQuest` (`QuestCondition?`, init), with a summary beside the other two gates.
    - `Core/Exploration/ActiveMap.CanStart(MapEvent mapEvent, string leaderId)`: the leader gate, not a "once" event already done, and the quest condition met (the world's `Quests`).
    - `InteractionFinder.FindInFront` (keeps `CanRunOn`), `ArrivalEvents.IsRunnable` and R1's `MapEventStarter` (the cell event) call it instead of their own checks; their summaries name the quest gate.
  - **Tests:**
    - `QuestConditionTests: IsMetBy_NotStartedBeforeStart_Met`
    - `QuestConditionTests: IsMetBy_NotStartedAfterStart_NotMet`
    - `QuestConditionTests: IsMetBy_ActiveAtAnyStage_Met`
    - `QuestConditionTests: IsMetBy_ActiveOnceCompleted_NotMet`
    - `QuestConditionTests: IsMetBy_AtStageAtAnotherStage_NotMet`
    - `QuestConditionTests: IsMetBy_ReachedStage_MetAtOrPastItOrCompleted` (theory: before it false, at it true, past it true, completed true, not started false)
    - `QuestConditionTests: IsMetBy_ReachedStageAfterAdvanceBack_NotMet` (decision 389's flagged reading)
    - `QuestConditionTests: IsMetBy_CompletedWhileActive_NotMet`
    - `ActiveMapTests: CanStart_QuestConditionUnmet_ReturnsFalse`
    - `ExplorationQuestTests: TryInteract_TwoEventsGatedOnStages_RunsTheCurrentStagesEvent` (new file)
    - `ExplorationQuestTests: TryInteract_AfterEventStartedTheQuest_RunsTheActiveEvent` (the lines change as the quest advances)
    - `ExplorationQuestTests: InteractionInFront_AfterEventCompletedTheQuest_FollowsTheCompletedEvent`
    - `ExplorationQuestTests: Walk_OntoCellGatedOff_StartsNoEvent`
    - `ExplorationQuestTests: New_ArrivalEventForQuestNotStarted_Runs` (the arrival path through the gate)
  - **Existing tests:** the leader and "once" tests of the three callers pass unchanged (the gate's refactor guard).
  - **Verify by hand:** nothing to see yet (no content); N1 checks it in play.

- **Done, with deviations:** the log's entry now keeps the quest definition (its first reader, `HasReached`); `AbsentWhenNullFilter` already lists `MapEvent.onlyWhenQuest` here, not in QV, since the writer's byte-identity test runs on every map as soon as `MapEvent` has the field. `ActiveMapTests` got both sides of the gate (the quest condition unmet then met; a done "once" event and another leader refused) and `ExplorationQuestTests` the cell's and the arrival's other side too (gated on). The `ReachedStage` theory is three rows plus a test for the earlier stage; an unknown stage on `ReachedStage` and a missing stage on either stage kind throw. Tests 3186 → 3210. Review: `AtStage` without a stage was true on a quest never started (null equals null); it throws now, tested.
