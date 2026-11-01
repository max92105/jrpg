# SV. Quests in saves, format 7

Part of [the quests plan](../README.md). Right after N1. Decisions 391 (the journal's order rides on the saved start order), 396 (repair). ADR 0022 (bump and refuse until release), ADR 0007. The gold plan's G2 made format 6. Design: [design.md](../design.md), "Replacement mapping", "Risks" (save format).

- **SV. Quests in saves, format 7, repaired against content** (Core + Data, about 240 lines)
  - **Goal:** a save keeps every started quest's stage and completion in start order and restores them; a format 6 save is refused naming its version; a saved quest or stage content no longer has is repaired and reported; a null entry in the saved quests is refused as damage.
  - **New types:**
    - `Core/Quests/QuestProgress` (public sealed record): one quest's progress as a save keeps it: `QuestId`, `StageId`, `IsCompleted`.
  - **Changes:**
    - `Core/Quests/QuestLog`: keeps the order quests were started in; `Progress` (`IReadOnlyList<QuestProgress>`, in that order); `static QuestLog Restore(IEnumerable<QuestProgress> saved, IReadOnlyDictionary<string, QuestDefinition> quests)` (for a repaired save: every id is known); the class summary names the order.
    - `Core/World/WorldState(completedEvents, objectSwaps, QuestLog quests)`: the restore constructor takes the restored log.
    - `Core/Saving/SaveGame.Quests` (required, `IReadOnlyList<QuestProgress>`, its summary: in start order, which the journal reads).
    - `Core/Session/GameSession.Capture` writes it; `Restore` builds the log and passes it to `WorldState`.
    - `Data/Saving/SaveFile.CurrentFormatVersion` 7.
    - `Data/Saving/FileSaveStore.CheckNoNullEntries`: `game.Quests.Contains(null)` too.
    - `Data/Saving/WorldStateRepair.KeepKnownQuests(quests, catalog, changes)`: a quest no longer in content is forgotten; an unknown stage goes back to the quest's first stage; a quest listed twice keeps its first entry; each reported. `SaveGameRepair.Repair` calls it.
  - **Tests:**
    - `QuestLogTests: Progress_TwoQuestsStarted_OldestFirst`
    - `QuestLogTests: Restore_FromProgress_SameStagesCompletionAndOrder`
    - `GameSessionQuestSaveTests: Capture_StartedQuest_KeepsItsStage` (new file: `GameSessionSaveTests` is at 270)
    - `GameSessionQuestSaveTests: Restore_CapturedQuests_SameStagesCompletionAndOrder`
    - `FileSaveStoreQuestTests: Write_ThenRead_KeepsQuests`
    - `FileSaveStoreFormatTests: Read_FormatSix_ThrowsOlder` (the gold plan's `Read_FormatFive_ThrowsOlder`, moved on one)
    - `FileSaveStoreFormatTests`: `"quests"` added to `ListsWithANullEntry` and its case (the null-entry row, `tests.md`)
    - `SaveGameRepairQuestTests: Repair_UnknownQuest_ForgetsItAndSaysSo`
    - `SaveGameRepairQuestTests: Repair_UnknownStage_BackToFirstStageAndSaysSo`
    - `SaveGameRepairQuestTests: Repair_QuestListedTwice_KeepsTheFirst`
    - `SaveGameRepairQuestTests: Repair_KnownQuests_Unchanged`
  - **Existing tests to update:** `FileSaveStoreFormatTests` (the written envelope six to seven, older five to six, newer seven to eight, the version inside the duplicate-field and truncated-file cases); `Quests = []` in the four `SaveGame` initializers (`SaveGameFixtures` twice, `SaveGameRepairWornTests`, `SaveGameRepairGearTests`); `WorldStateTests`' restore construction.
  - **Watch:** `GameSession` (about 255 after the gold plan, +3); `FileSaveStoreFormatTests` (about 230 after the gold plan: the null row is one line).
  - **Verify by hand:** start the errand at the giver, save, quit, load: the giver gives the reminder line, not the start; finish it, save, load: the thanks line. A save from before this slice: the list says it can't be read.

- **Done, with deviations:** as planned; `QuestLog` keeps the start order in a list beside its entries and exposes `Progress` for the saves. The world lists' null checks (`completedEvents`, `objectSwaps`, `quests`) moved into `HasNullWorldEntry` so `CheckNoNullEntries` stays under the complexity limit. The session exposes no quest log, so `GameSessionQuestSaveTests` starts the quest through a cell event and checks the restore by capturing again. `StoredSave` carries one quest, so every store round trip covers it. Tests 3255 → 3267 (the review added the completed quest at a vanished stage).
