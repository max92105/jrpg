---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice QE3 (Editor/Quests: QuestCatalogViewModel, QuestEditingViewModel, MainViewModel.QuestEditing). Round 1.
Verdict: Ready to commit.

- Warning, open: QuestEditingViewModelTests.Apply_ChangedQuests edits `edited.Quests[0]` assuming content's first quest by id is placeholder_errand; pick it by id
- Fixed (from QE2): QuestViewModelTests now build their own QuestDefinition
- Fixed (from QE2): a removed stage unsubscribes OnStageChanged (QuestViewModel line 113)
- Follow-up (from QE2 suggestion): ObjectIds' id problem says "(like tree_4)", shown in the Quests window too
- Follow-up (from QE1): CatalogDraft.SetQuests ToDictionary throws on duplicate ids (now guarded by QuestCatalogViewModel.Problem for the window)
- Follow-up (carried from G4): EventPresenter.IsShowingMessage summary omits the quest line
- Follow-up (carried from L3): capacity test for three lines each of description and stage text
- Follow-up: ResourceGaugeView.LabelSpacing (2) duplicates SheetRowLook.LabelSpacing
- Follow-up (carried): ShopListView.TabAt copy of PointedTabsView.TabAt (backlog)
- Follow-up (carried): Margin 28 / RowHeight 52 copied in several list views (backlog)
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
