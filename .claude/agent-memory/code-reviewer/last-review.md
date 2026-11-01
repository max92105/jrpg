---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice G1 (QuestsPage, TabGlyph.Quests, first tab, texts). Round 1.
Verdict: Ready to commit.

- Fixed (from J1): QuestJournalTests Active_MainAndSideQuests_MainFirst now starts the main quest first
- Warning, open: ui.json menu_hint_quests "Your active quests, newest first." contradicts the journal order (main before side) - drop "newest first"
- Suggestion, open: QuestsPageTests Glyph_IsTheQuestsGlyph also asserts LeavesOnCancel; name doesn't say so
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
