---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice QE2 (Editor/Quests: QuestViewModel, QuestStageViewModel and their rules). Round 1.
Verdict: Ready to commit.

- Warning, open: QuestViewModelTests build on content's placeholder_errand and assert its whole stage list / count 3 (G2); build a QuestDefinition in the test
- Suggestion, open: the id problem says "(like tree_4)" (ObjectIds' object example) in the Quests window
- Suggestion, open: a removed stage keeps its PropertyChanged subscription to OnStageChanged
- Fixed (from QE1): CatalogUsage events copy -> Core MapLayout.AllEvents
- Fixed (from QE1): CombinedHistory.RefusalOfCatalogUndo comment order
- Follow-up (from QE1): CatalogDraft.SetQuests ToDictionary throws on duplicate ids before the validator names them
- Follow-up (carried from G4): EventPresenter.IsShowingMessage summary omits the quest line
- Follow-up (carried from L3): capacity test for three lines each of description and stage text
- Follow-up: ResourceGaugeView.LabelSpacing (2) duplicates SheetRowLook.LabelSpacing
- Follow-up (carried): ShopListView.TabAt copy of PointedTabsView.TabAt (backlog)
- Follow-up (carried): Margin 28 / RowHeight 52 copied in several list views (backlog)
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
