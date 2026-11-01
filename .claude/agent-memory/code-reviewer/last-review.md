---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice J1 (QuestJournal/QuestEntry, QuestLog.StartedQuests, GameSession.Quests). Round 1.
Verdict: Fix the Criticals, then commit.

- Critical, open: QuestJournalTests Active_MainAndSideQuests_MainFirst starts the side quest first, so newest-first alone puts the main one first; deleting OrderBy(Kind) keeps every test green - start the main quest first
- Fixed: SV FileSaveStore detached comment (now above HasNullStockEntry)
- Fixed: SV Done note's StartedQuests claim (J1 adds it)
- Declined/dropped: SV suggestion (completed quest at unknown stage repair test) - suggestion, not carried
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
