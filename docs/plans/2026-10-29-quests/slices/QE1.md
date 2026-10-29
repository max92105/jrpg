# QE1. Quests in the editor's catalog draft

Part of [the quests plan](../README.md). After E3 (any time after QS's references). Decision 394. ADR 0014 (editing shared catalogs), ADR 0012. Design: [design.md](../design.md), "Key decisions" (quests are a catalog in the editor), "Risks" (hand-written quests). The rules and catalog step of the Quests window (QE2 to QE4).

- **QE1. Quests in the editor's catalog draft: one undo step, saved with the catalogs, an undo removing a used quest refused** (Data + Editor, about 210 lines)
  - **Goal:** the editor's draft can replace its quests as one undo step; Ctrl+S writes `quests.json` with the other catalogs, checked as the loader checks it and with every map checked against it (nothing written on a problem); an undo that would remove a quest or a stage a map's event names is refused, naming it, as for objects and tiles; the event dialogs offer the draft's quests, so a quest set in the draft is offered at once.
  - **New types:** none.
  - **Changes:**
    - `Editor/Catalogs/CatalogDraft.SetQuests(IReadOnlyList<QuestDefinition> quests)`: replaces the quests as one `CatalogEdit`; nothing when they are the same by content (records hold their stages as a list: compared by content, as `IsSameObject` does). `QuestStagesRemovedByUndo()`: the `(string QuestId, string StageId)` pairs the next undo would remove (a removed quest counts with all its stages); pairs, not `Events/QuestStageChoice`, since `Events` reads `Catalogs` (design.md, "Structure").
    - `Editor/Catalogs/CatalogUsage.UsersOfQuest(ContentCatalog catalog, string questId, string? stageId, MapLayout? openLayout)`: the maps whose events name the quest (a step or a condition), or that stage when one is given.
    - `Editor/Shell/CombinedHistory.RefusalOfCatalogUndo`: "Can't undo: stage {stage} of quest {quest} is used on {map}." (and "quest {quest} is used on {map}." for a whole quest).
    - `Data/Content/ContentWriter.WriteMapCatalogs`: also validates (`QuestCatalogValidator`) and writes `quests.json`, sorted by id, with the three others, all or none; the name stays (ten test sites), its summary lists the quests.
    - `docs/decisions/0014-editing-shared-catalogs.md`: a dated note under Consequences: `quests.json` joins the catalogs Ctrl+S writes (decision 394); the README line's "all three files" becomes "all the catalog files".
  - **Tests:**
    - `CatalogDraftQuestTests: SetQuests_ChangedQuest_OneUndoStep`
    - `CatalogDraftQuestTests: SetQuests_SameContent_RecordsNoStep`
    - `CatalogDraftQuestTests: Undo_SetQuests_RestoresTheQuests`
    - `CatalogDraftQuestTests: QuestStagesRemovedByUndo_AddedStage_ListsIt`
    - `CatalogUsageQuestTests: UsersOfQuest_StepAndCondition_ListTheirMaps`
    - `CatalogUsageQuestTests: UsersOfQuest_StageNamedByAnAdvanceStep_ListsTheMap`
    - `CombinedHistoryQuestTests: Undo_RemovesAStageAnEventNames_RefusedNamingIt`
    - `ContentWriterQuestTests: WriteMapCatalogs_WritesQuestsSortedById`
    - `ContentWriterQuestTests: WriteMapCatalogs_StageAnEventNamesMissing_RefusesAndWritesNothing`
    - `EventEditingViewModelTests: StartEditingAt_QuestSetInTheDraft_OffersIt`
  - **Verify by hand:** nothing new to see (the window is QE4); open the editor, change a map, Ctrl+S: `quests.json` is written back unchanged (byte for byte with the repository's file).
