# SV. Quests in saves, format 6

Part of [the quests plan](../README.md). After QL (QV may land before or after). ADR 0022 (bump and refuse until release), ADR 0007. Q12's repair rule. Design: [design.md](../design.md), "Replacement mapping", "Risks" (save format).

- **SV. Quests in saves, format 6, repaired against content** (Core + Data, about 230 lines)
  - **Goal:** a save keeps every started quest's stage and status in start order and restores them; a format 5 save is refused naming its version; a saved quest or stage content no longer has is repaired and reported.
  - **New types:**
    - `Core/Quests/QuestProgress` (public sealed record): one quest's progress as a save keeps it: `QuestId`, `StageId`, `IsCompleted`.
  - **Changes:**
    - `Core/Quests/QuestLog`: keeps the order quests were started in; `Progress` (`IReadOnlyList<QuestProgress>`, in that order) and a restore constructor `QuestLog(IEnumerable<QuestProgress> saved)`; the class summary names the order.
    - `Core/World/WorldState(completedEvents, objectSwaps, quests)`: the restore constructor takes the progress.
    - `Core/Saving/SaveGame.Quests` (required, `IReadOnlyList<QuestProgress>`, its summary: in start order, the journal's order).
    - `Core/Session/GameSession.Capture` writes it; `Restore` passes it to `WorldState`.
    - `Data/Saving/SaveFile.CurrentFormatVersion` 6.
    - `Data/Saving/WorldStateRepair.KeepKnownQuests(quests, catalog, changes)`: a quest no longer in content is forgotten; an unknown stage goes back to the quest's first stage; a quest listed twice keeps its first entry; each reported. `SaveGameRepair.Repair` calls it.
  - **Tests:**
    - `QuestLogTests: Progress_TwoQuestsStarted_OldestFirst`
    - `QuestLogTests: New_FromProgress_RestoresStatusStageAndOrder`
    - `GameSessionQuestSaveTests: Capture_StartedQuest_KeepsItsStage` (new file: `GameSessionSaveTests` is at 270)
    - `GameSessionQuestSaveTests: Restore_CapturedQuests_SameStatusesStagesAndOrder`
    - `FileSaveStoreQuestTests: Write_ThenRead_KeepsQuests`
    - `FileSaveStoreFormatTests: Read_FormatVersion5_ThrowsNamingTheVersion` (the old format refused; the existing version-4 test becomes it)
    - `SaveGameRepairQuestTests: Repair_UnknownQuest_ForgetsItAndSaysSo`
    - `SaveGameRepairQuestTests: Repair_UnknownStage_BackToFirstStageAndSaysSo`
    - `SaveGameRepairQuestTests: Repair_QuestListedTwice_KeepsTheFirst`
    - `SaveGameRepairQuestTests: Repair_KnownQuests_Unchanged`
  - **Existing tests to update:** `FileSaveStoreFormatTests` (the written version 5 to 6, older 4 to 5, newer 6 to 7, the version inside the duplicate-field and truncated-file cases: about 6 lines); `Quests = []` in the four `SaveGame` constructions (`SaveGameFixtures` twice, `SaveGameRepairWornTests`, `SaveGameRepairGearTests`); `WorldStateTests`' restore construction.
  - **Verify by hand:** save from the menu, quit, load: the game loads. Load a save written before this slice: the list says it can't be read (format 5).
