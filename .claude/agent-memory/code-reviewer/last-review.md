---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice G2 (QuestList, QuestDetail, QuestsPage selection + detail column). Round 1.
Verdict: Ready to commit.

- Fixed (from G1): ui.json menu_hint_quests no longer says "newest first"
- Suggestion, open (carried from G1, now merged further): QuestsPageTests Glyph_IsTheQuestsGlyph also asserts LeavesOnCancel
- Suggestion, open: G2 Done note says page tests check the detail "at each of its stages"; only return_to_giver's detail is asserted
- Follow-up: QuestsPage detail column is 340 px (1100 - 720 - 40) with untruncated description; L1-L3 should wrap
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
