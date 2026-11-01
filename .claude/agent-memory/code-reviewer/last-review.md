---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice L2 (PointedTabsView, QuestListView tabs, L1 leftovers). Round 1.
Verdict: Fix the Criticals, then commit.

- Critical, open: two 380 px tabs + 24 gap = 784 px, but the Quests list row is 584 px (ColumnWidth 640 less margins); the Completed tab runs to column.X+812, past the detail column's left (column.X+680), and the detail title (same TopMargin row) draws over it. TabAt ignores Row.Width; fit tabs to the row (or narrow tab shape) and pin "last tab's Right <= row.Right" in a test
- Fixed (from L1): QuestList fitting test (Show_LongTitle_RowHoldsTheFittedTitle); ellipsis longest-start assert; loop names now slot/row
- Follow-up: ShopListView.TabAt is a verbatim copy of PointedTabsView.TabAt (backlog line 113 covers it)
- Follow-up: Margin 28 / RowHeight 52 still copied in ForgeListView, InventoryListView, EquipmentChoiceView and others (backlog line)
- Follow-up: QuestsPage detail description untruncated/unwrapped; L3 should wrap
- Follow-up (carried from G2): Done note claims detail "at each of its stages"; only return_to_giver asserted
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
