---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice L1 (list column look: PageListLayout, TextEllipsis, QuestRow, QuestListView, QuestList(fitTitle)). Round 1.
Verdict: Ready to commit.

- Warning, open: QuestList's new title fitting (Column.Show applies fitTitle) untested; QuestListTests only pass the identity; add Show_FitsEachTitle with a marking function
- Suggestion, open: QuestListView.Draw loop names a QuestRow `entry` (entry.Entry.Quest) and the slot index `row`; rename to `row`/`slot`
- Suggestion, open: TextEllipsisTests don't pin "longest start that fits" (one more character must not fit)
- Fixed (from G3): per-frame RefreshDetail; WithTheErrand flag argument split; redundant NotEqual in QuestDetailTests
- Follow-up: Margin 28 / RowHeight 52 still copied in ForgeListView, InventoryListView, EquipmentChoiceView and others (backlog line)
- Follow-up (G2): Done note claims detail "at each of its stages"; only return_to_giver asserted
- Follow-up: QuestsPage detail description untruncated/unwrapped; L3 should wrap
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
