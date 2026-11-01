---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice E3 (quest/kind/stage boxes in both event dialogs; QuestConditionKindChoice). Round 1.
Verdict: Ready to commit.

- Warning, open: kind-to-label pairing untested (tests check labels only; CellEvent test asserts ConditionKinds == All); pin the Kind sequence too
- Suggestion, open: stage box stays enabled after the quest is cleared with a stage kind left (NeedsStage ignores HasQuest)
- Suggestion, open: QuestChoices_..InIdOrderAfterNone uses one quest; order is never exercised (and the VM doesn't sort)
- Fixed: E2 suggestion (dangling " is at " label) - StageOrQuestionMark shows "?"
- Follow-up (carried from E1, outside diff): EventStepViewModel.QuestStep() `_ =>` arm; EventEditingViewModel QuestStages one-liner
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data
- Follow-up (carried): Trade/ShopScene 296 lines
