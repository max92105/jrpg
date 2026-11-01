---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice QE1 (quests in the editor's catalog draft: SetQuests, undo refusal, quests.json in WriteMapCatalogs). Round 1.
Verdict: Ready to commit.

- Warning, open: CatalogUsage.EventsOf (Editor/Catalogs/CatalogUsage.cs:73) is the 4th copy of "a layout's object + cell events" (MapEventValidator.AllEvents, MapEventSteps.Of, WorldStateRepair:85); propose Core MapLayout.AllEvents
- Suggestion, open: CombinedHistory.RefusalOfCatalogUndo comment lists quests last ("or the first map whose event names a quest...") but RefusalOfQuestUndo runs first
- Suggestion, open: SetQuests' quests.ToDictionary throws on duplicate ids before the validator can name them (matters once QE2-QE4 feed it)
- Follow-up (carried from G4): EventPresenter.IsShowingMessage summary omits the quest line
- Follow-up (carried from L3): capacity test for three lines each of description and stage text
- Follow-up: ResourceGaugeView.LabelSpacing (2) duplicates SheetRowLook.LabelSpacing
- Follow-up (carried): ShopListView.TabAt copy of PointedTabsView.TabAt (backlog)
- Follow-up (carried): Margin 28 / RowHeight 52 copied in several list views (backlog)
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
