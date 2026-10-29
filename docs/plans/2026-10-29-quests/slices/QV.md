# QV. The quest condition in content

Part of [the quests plan](../README.md). After QC. Design: [design.md](../design.md), "What exists" (Data: `AbsentWhenNullFilter`).

- **QV. The condition in content: read, checked, left out of files while unset** (Data, about 110 lines)
  - **Goal:** a map's event reads `"onlyWhenQuest": { "questId": "...", "status": "active", "stageId": "..." }`; the content is refused when it names an unknown quest or stage, or a stage with a status other than active; and every existing map file writes back byte for byte.
  - **New types:** none.
  - **Changes:**
    - `Data/Maps/MapEventValidator.FindStartProblem`: `onlyWhenQuest has unknown quest id 'x'.`, `onlyWhenQuest names stage 'y', which only an active status can wait for.`, `onlyWhenQuest has unknown stage 'y' of quest 'x'.`
    - `Data/Content/AbsentWhenNullFilter`: `(MapEvent, "onlyWhenQuest")` and `(QuestCondition, "stageId")`, each with its one-line reason, as the two entries there.
  - **Tests:**
    - `MapEventValidatorQuestTests: FindProblem_ConditionUnknownQuest_NamesIt`
    - `MapEventValidatorQuestTests: FindProblem_ConditionStageWithCompletedStatus_SaysOnlyActive`
    - `MapEventValidatorQuestTests: FindProblem_ConditionUnknownStage_NamesQuestAndStage`
    - `ContentWriterQuestTests: WriteMap_EventWithoutQuestCondition_LeavesTheFieldOut`
    - `ContentWriterQuestTests: WriteMap_ConditionWithoutStage_LeavesStageIdOut`
    - `ContentWriterQuestTests: WriteMap_EventWithQuestCondition_ReadBackEqual`
  - **Watch:** the repository's map files must write back unchanged (the existing byte-for-byte writer test covers them).
  - **Verify by hand:** nothing to see yet.
