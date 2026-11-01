---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice G3 (Active/Completed categories: QuestJournal.Completed, QuestCategory, QuestList columns, QuestDetail note, QuestsPage Left/Right). Round 1.
Verdict: Ready to commit.

- Warning, open: QuestsPage.Update calls RefreshDetail every frame (new QuestDetail + two message lookups per frame); refresh only when a button changed the choice or category
- Suggestion, open: QuestsPageTests.WithTheErrand(stageId, bool isCompleted) is a flag argument; two named helpers (WithTheErrandAt / WithTheErrandCompletedAt)
- Suggestion, open: QuestDetailTests completed-note test's Assert.NotEqual("Deliver it.") is implied by the Equal above it
- Fixed (from G1/G2): Glyph test no longer asserts LeavesOnCancel (own test now)
- Follow-up (G2): Done note claims detail "at each of its stages"; only return_to_giver asserted
- Follow-up: QuestsPage detail column 340 px with untruncated description; L1-L3 should wrap
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
