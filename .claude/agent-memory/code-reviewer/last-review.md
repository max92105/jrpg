---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice E2 (QuestGateViewModel; EventViewModel keeps OnlyWhenQuest, label, problem). Round 1.
Verdict: Ready to commit.

- Suggestion, open: QuestGateViewModel.Label with a stage kind and no stage reads " when errand is at " (dangling); show "?" or similar until picked
- Follow-up (carried from E1, outside diff): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
