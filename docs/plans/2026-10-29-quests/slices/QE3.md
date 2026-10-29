# QE3. The quest list, applied as one undo step

Part of [the quests plan](../README.md). After QE2. Decision 394. Design: [design.md](../design.md), "Key decisions" (quests are a catalog in the editor).

- **QE3. The Quests window's list: new quests, unique ids, applied to the draft as one undo step** (Editor, about 170 lines)
  - **Goal:** the window's state lists the draft's quests in id order, adds a new quest (chosen at once, its id empty for the developer to type), refuses a quest id used twice, and applies the whole session to the draft as one undo step, nothing when nothing changed.
  - **New types** (`Editor/Quests/`):
    - `QuestCatalogViewModel` (internal sealed, observable): every quest as the window edits it: `Quests`, `SelectedQuest`, `AddQuest`; `Problem` (a repeated id, or the first quest's problem naming it); `ToQuests()`.
    - `QuestEditingViewModel(CatalogDraft catalog)` (internal sealed): opens the window's state from the draft (`Start()`) and applies it (`Apply(QuestCatalogViewModel edited)`: `catalog.SetQuests`), as `EventEditingViewModel` does for events.
  - **Changes:** `Editor/Shell/MainViewModel.QuestEditing` (built beside `EventEditing`; the file is already over 300, one property and its construction).
  - **Tests:**
    - `QuestCatalogViewModelTests: New_DraftQuests_ListedInIdOrder`
    - `QuestCatalogViewModelTests: AddQuest_SelectsANewQuestWithAnEditableId`
    - `QuestCatalogViewModelTests: Problem_RepeatedQuestId_NamesIt`
    - `QuestCatalogViewModelTests: Problem_QuestWithAProblem_NamesTheQuest`
    - `QuestCatalogViewModelTests: ToQuests_AddedQuest_IncludedWithTheOthers`
    - `QuestEditingViewModelTests: Apply_ChangedQuests_OneUndoStepInTheDraft`
    - `QuestEditingViewModelTests: Apply_Unchanged_RecordsNoStep`
  - **Verify by hand:** nothing to see yet (QE4).
