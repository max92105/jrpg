# QE2. One quest as the Quests window edits it

Part of [the quests plan](../README.md). After QE1. Decision 394 (with its flagged reading: only what the session added can be removed). Design: [design.md](../design.md), "Structure and folder dependencies" (Editor `Quests`). The editor's `StartX`/`TryApplyX` and `Problem` conventions (`EventStepViewModel`).

- **QE2. The Quests window's rules: one quest, its stages, their ids and problems** (Editor, about 210 lines)
  - **Goal:** one quest can be edited as the window will show it: its id (typed while the quest is new, fixed once saved in the draft), title, description, kind, and its stages in order (add after the chosen one, move up and down, remove only a stage added in this session; a stage's id typed while new); a problem line names the first thing that would make the game refuse it.
  - **New types** (`Editor/Quests/`, a new feature folder):
    - `QuestViewModel` (internal sealed, observable): one quest as edited. `From(QuestDefinition quest)`, `New()`; `Id`, `IsNew`, `Title`, `Description`, `Kind`; `Stages`, `SelectedStage`; `AddStage`, `RemoveStage` (enabled for a new stage), `MoveStageUp`, `MoveStageDown` (`ListOrder`, as the event dialog's steps); `Problem`; `ToQuest()` (throws on a problem).
    - `QuestStageViewModel` (internal sealed, observable): one stage as edited: `Id`, `IsNew`, `Text`, `Problem`.
  - **Changes:** none outside the folder.
  - **Tests** (`QuestViewModelTests`):
    - `From_SavedQuest_IdsFixed`
    - `AddStage_StageChosen_AddsAnEditableStageAfterIt`
    - `RemoveStage_SavedStage_CannotExecute`
    - `RemoveStage_NewStage_RemovesIt`
    - `MoveStageDown_FirstStage_BecomesSecond`
    - `Problem_NoStage_AsksForOne`
    - `Problem_RepeatedStageId_NamesIt`
    - `Problem_BlankStageText_NamesTheStage`
    - `ToQuest_Edited_ReturnsTheDefinitionWithStagesInOrder`
  - **Verify by hand:** nothing to see yet (QE4).
- **Done, with deviations:** as planned; the id rule is `Objects/ObjectIds.ProblemOf` (the one content id rule; its rename to a shared `ContentIds` is a backlog line), a repeated stage id is reported on the later stage ("Stage 2 repeats the id ..."), and a new quest defaults to a side quest (placeholder default, yours to change). Tests 3323 → 3335. Review: the tests build their own two-stage quest instead of loading the repository's errand; stage subscriptions follow the collection's changes.
