---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice L3 (QuestDetailView: serif title, kind caps, wrapped texts, Objective band). Round 1.
Verdict: Ready to commit.

- Warning, open: QuestDetailView.cs:104 kind drawn upper-cased without characterSpacing, though slice and the type's doc say "spaced caps"; pass characterSpacing: SheetRowLook.LabelSpacing
- Warning, open: slice's "at least three lines each of description and stage text above the footer" unpinned; fit probe checks only the errand's texts; add a capacity test (fixed parts + 6 * text line height <= BodyHeight)
- Suggestion, open: Draw's `_description is null || _stage is null` and Show's `!` exist only for nullable flow; one nullable shown-layout record removes both
- Suggestion, open: width-change re-wrap branch untested
- Fixed (from L2): tabs narrowed to fit the row (commit 96b1b9c)
- Fixed (from L2): QuestsPage detail now wrapped (this slice)
- Follow-up: ResourceGaugeView.LabelSpacing (2) duplicates SheetRowLook.LabelSpacing
- Follow-up (carried): ShopListView.TabAt copy of PointedTabsView.TabAt (backlog)
- Follow-up (carried): Margin 28 / RowHeight 52 copied in several list views (backlog)
- Follow-up (carried from G2): Done note claims detail "at each of its stages"; only return_to_giver asserted
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
