---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice N1 (placeholder errand on start.json: quest_giver + innkeeper advance event; RepositoryQuestTests, StartMapQuestTests). Round 1.
Verdict: Ready to commit.

- Warning, open: giver's event order untested (completing event's ReachedStage gate and its place before the Active reminder); moving the reminder first makes the errand uncompletable with all tests green
- Suggestion, open: Interact_InnkeeperWithNoQuest_StillRunsTheRestEvent repeats StartMapVillagerTests.Interact_FacingInnkeeper_ReadsALineThenRests and asserts less (TryInteract unchecked)
- Suggestion, open: ContentWriterQuestTests string slicing by "gated_cell".."steps" - reading the loaded CellEvent or JsonNode would not depend on key order
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1, outside diff): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
