---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice G4 (quest notices: QuestChange, waiting quest step, QuestNoticeText). Round 1.
Verdict: Ready to commit.

- Suggestion, open: EventPresenter.IsShowingMessage summary "(a dialogue page, a found item, a rest)" omits the quest line; StartReading's "Core sets a receipt..." comment doesn't say why CurrentQuestChange! holds (only a changed step waits)
- Suggestion, open: EventPresenterQuestTests builds its expected line from QuestNoticeText.Of(change) without pinning change.Kind == Started; name promises "the started line"
- Suggestion, open: QuestLogTests don't assert AdvanceTo on a completed quest returns null, nor Complete on a not-started quest returns Completed
- Fixed (from L3): kind caps now drawn with characterSpacing: SheetRowLook.LabelSpacing
- Follow-up (carried from L3): capacity test for three lines each of description and stage text
- Follow-up: ResourceGaugeView.LabelSpacing (2) duplicates SheetRowLook.LabelSpacing
- Follow-up (carried): ShopListView.TabAt copy of PointedTabsView.TabAt (backlog)
- Follow-up (carried): Margin 28 / RowHeight 52 copied in several list views (backlog)
- Follow-up (carried from G2): Done note claims detail "at each of its stages"; only return_to_giver asserted
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
