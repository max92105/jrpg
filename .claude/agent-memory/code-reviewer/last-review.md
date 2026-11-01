---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice E1 (event dialogs show and add start/advance/complete quest steps). Round 1.
Verdict: Ready to commit.

- Suggestion, open: EventStepViewModel.QuestStep() `_ =>` arm silently builds CompleteQuestStep for any kind; name CompleteQuest and keep `_ => throw new UnreachableException()`
- Suggestion, open: EventEditingViewModel.Choices QuestStages one-liner (~200 cols); split into a private QuestStageChoices() method
- Follow-up (carried from QV): enum-to-camelCase name written in five places in Data; one shared helper type
- Follow-up (carried): Trade/ShopScene 296 lines
- QV suggestions (CamelCase<T> name, FindShadowedEvent guard order) not re-raised: outside this diff
