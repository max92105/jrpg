---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice QE4 (Editor/Quests/QuestCatalogWindow, Shell/MainWindow.QuestEditing, Quests menu). Round 1.
Verdict: Ready to commit.

- Fixed (from QE3): QuestEditingViewModelTests pick placeholder_errand by id
- Suggestion, open: right-hand quest/stage fields stay enabled with no quest or stage chosen (typing goes nowhere); IsEnabled on a null check
- Suggestion, open: "Edit quests..." silently does nothing when QuestEditing is null (no content); bind IsEnabled
- Follow-up (from QE2 suggestion): ObjectIds' id problem says "(like tree_4)", shown in the Quests window too
- Follow-up (from QE1): CatalogDraft.SetQuests ToDictionary throws on duplicate ids (guarded by QuestCatalogViewModel.Problem)
- Follow-up (carried from G4): EventPresenter.IsShowingMessage summary omits the quest line
- Follow-up (carried from L3): capacity test for three lines each of description and stage text
- Follow-up: ResourceGaugeView.LabelSpacing (2) duplicates SheetRowLook.LabelSpacing
- Follow-up (carried): ShopListView.TabAt copy of PointedTabsView.TabAt (backlog)
- Follow-up (carried): Margin 28 / RowHeight 52 copied in several list views (backlog)
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
