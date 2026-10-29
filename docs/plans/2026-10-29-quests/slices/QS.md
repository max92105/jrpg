# QS. The quest steps

Part of [the quests plan](../README.md). After QL. ADR 0006 (steps that apply at once). Design: [design.md](../design.md), "What exists" (events, Data).

- **QS. The `startQuest`, `advanceQuest` and `completeQuest` steps** (Core + Data, about 200 lines)
  - **Goal:** a map event can start, advance and complete a quest; each step applies at once, as a swap does, and the map's content is refused when a step names an unknown quest or stage.
  - **New types** (`Core/Events`, public sealed records deriving from `EventStep`):
    - `StartQuestStep(string QuestId)`: starts a quest (`QuestLog.Start`).
    - `AdvanceQuestStep(string QuestId, string StageId)`: moves a quest to a stage (`QuestLog.AdvanceTo`).
    - `CompleteQuestStep(string QuestId)`: completes a quest (`QuestLog.Complete`).
  - **Changes:**
    - `Core/Exploration/EventRun.ApplyAndCheckWait`: three cases, each `_context.World.Quests.X(_context.Catalog.Quests[step.QuestId], ...)`, returning false; the class summary names them among the steps that apply at once.
    - `Data/Content/ContentTypeDiscriminators`: `"startQuest"`, `"advanceQuest"`, `"completeQuest"`.
    - `Data/Maps/MapContentReferences.Quests` (init, `IReadOnlyDictionary<string, QuestDefinition>`, empty by default so the two test constructions don't change); set by `ContentCatalogLoader` and `ContentWriter.ContentReferencesOf` (so the editor's map save refuses what the game would).
    - `Data/Maps/MapEventValidator.FindStepProblem`: an unknown quest id (each step), an unknown stage for `advanceQuest` (naming quest and stage).
  - **Tests:**
    - `EventRunQuestTests: Start_StartQuestStep_QuestActiveAtFirstStage` (new file: `EventRunTests` is at 269)
    - `EventRunQuestTests: Start_AdvanceQuestStep_QuestAtThatStage`
    - `EventRunQuestTests: Start_CompleteQuestStep_QuestCompleted`
    - `EventRunQuestTests: Start_QuestStepBeforeDialogue_AppliedAndWaitsOnTheDialogue`
    - `MapEventValidatorQuestTests: FindProblem_StartQuestUnknownQuest_NamesIt`
    - `MapEventValidatorQuestTests: FindProblem_AdvanceQuestUnknownStage_NamesQuestAndStage`
    - `MapEventValidatorQuestTests: FindProblem_CompleteQuestUnknownQuest_NamesIt`
    - `MapEventValidatorQuestTests: FindProblem_KnownQuestAndStage_ReturnsNull`
    - `ContentWriterQuestTests: WriteMap_QuestSteps_ReadBackEqual` (the three type names round trip)
  - **Known gap:** the editor's `EventStepViewModel.From` throws on these steps until E1; no content uses them before N1.
  - **Verify by hand:** nothing to see yet; the tests are the check.
