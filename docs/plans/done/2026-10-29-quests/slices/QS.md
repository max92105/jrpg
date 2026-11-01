# QS. The quest steps

Part of [the quests plan](../README.md). After QL. ADR 0006 (a step applies when it becomes current), ADR 0016 (skipping). Design: [design.md](../design.md), "What exists" (events, Data), "Risks" (CA1502).

- **QS. The `startQuest`, `advanceQuest` and `completeQuest` steps** (Core + Data, about 220 lines)
  - **Goal:** a map event can start, advance and complete a quest; each step applies at once, as a swap does (G4 makes a changed one wait for its line), skipping a cutscene applies them, and a map is refused when a step names an unknown quest or stage.
  - **New types** (`Core/Events`, public sealed records deriving from `EventStep`):
    - `StartQuestStep(string QuestId)`: starts a quest (`QuestLog.Start`).
    - `AdvanceQuestStep(string QuestId, string StageId)`: moves a quest to a stage (`QuestLog.AdvanceTo`).
    - `CompleteQuestStep(string QuestId)`: completes a quest (`QuestLog.Complete`).
  - **Changes:**
    - `Core/Exploration/EventRun.ApplyAndCheckWait` (CA1502 budget): one `case StartQuestStep or AdvanceQuestStep or CompleteQuestStep:` calling a private `ApplyQuestStep(EventStep step)` (its own switch over the three, each `_context.World.Quests.X(_context.Catalog.Quests[...], ...)`), returning false. If the method is still over the limit, the rest case's member loop moves into its own method; the gold plan's I1 does that (`ApplyRest`) and runs first, so check before moving anything. The class summary names the quest steps among those that apply at once.
    - `Data/Content/ContentTypeDiscriminators`: `"startQuest"`, `"advanceQuest"`, `"completeQuest"`.
    - `Data/Maps/MapContentReferences.Quests` (init, `IReadOnlyDictionary<string, QuestDefinition>`, empty by default, as the gold plan's `ShopIds`); set by `ContentCatalogLoader` and `ContentWriter.ContentReferencesOf` (so the editor's map save refuses what the game would); the three test constructions stay.
    - `Data/Maps/MapEventValidator.FindStepProblem`: the three steps go to one helper `FindQuestStepProblem(step, references)` (CA1502): an unknown quest id, and for `advanceQuest` an unknown stage naming quest and stage.
  - **Tests:**
    - `EventRunQuestTests: Start_StartQuestStep_QuestActiveAtFirstStage` (new file: `EventRunTests` is at 269)
    - `EventRunQuestTests: Start_AdvanceQuestStep_QuestAtThatStage`
    - `EventRunQuestTests: Start_CompleteQuestStep_QuestCompleted`
    - `EventRunQuestTests: Start_QuestStepBeforeDialogue_AppliedAndWaitsOnTheDialogue`
    - `EventRunQuestTests: SkipPresentation_QuestStepBetweenStills_Applied`
    - `MapEventValidatorQuestTests: FindProblem_StartQuestUnknownQuest_NamesIt` (new file)
    - `MapEventValidatorQuestTests: FindProblem_AdvanceQuestUnknownStage_NamesQuestAndStage`
    - `MapEventValidatorQuestTests: FindProblem_CompleteQuestUnknownQuest_NamesIt`
    - `MapEventValidatorQuestTests: FindProblem_KnownQuestAndStage_ReturnsNull`
    - `ContentFileWriterTests: Write_EveryEventStepKind_ReadsBackAsTheSameSteps`: the three steps added (the type names round trip)
  - **Known gap:** the editor's `EventStepViewModel.From` throws on these steps until E1; no content uses them before N1.
  - **Verify by hand:** nothing to see yet; the tests are the check.

- **Done, with deviations:** as planned; `ApplyAndCheckWait` stayed under the complexity limit with the one combined case, so nothing else moved. The validator's helper reads the quest from `MapContentReferences.Quests` (the definitions, so the stage check needs no second lookup); `FindProblem_AdvanceQuestUnknownQuest_NamesIt` added beside the planned tests. Tests 3176 → 3186. Review: the two-stage errand the quest tests share is `TestQuests.Errand` in each test project.
