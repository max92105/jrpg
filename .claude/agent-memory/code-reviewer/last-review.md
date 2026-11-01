---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice SV (quests in saves, format 7: QuestProgress, QuestLog.Progress/Restore, WorldStateRepair.KeepKnownQuests). Round 1.
Verdict: Fix the Criticals, then commit.

- Critical, open: FileSaveStore.cs:132 detached comment - "A shop's stock is a list of pieces..." now sits above HasNullWorldEntry; move it down to HasNullStockEntry
- Warning, open: SV.md Done note says QuestLog exposes `StartedQuests` for J1; no such member exists in the tree (grep) - drop the claim or add it in J1's file
- Suggestion, open: no repair test for a completed quest at an unknown stage (stays completed, back at first stage) - pins decision 396 for completed quests
- Follow-up (carried from N1): giver's event order untested
- Follow-up (carried from E3): kind-to-label pairing untested in E3's view model tests
- Follow-up (carried from E1): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
