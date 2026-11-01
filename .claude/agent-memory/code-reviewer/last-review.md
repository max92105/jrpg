---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice QC (events gated on a quest; ActiveMap.CanStart one gate; ADR 0033). Round 1.
Verdict: Ready to commit.

- Warning, open: QuestCondition.IsMetBy AtStage arm doesn't use NamedStage(); AtStage with null stage on a quest not started returns true (acts as NotStarted) where ReachedStage throws
- Suggestion, open: MapEventStarter summary writes `ActiveMap.CanStart` in backticks; XML doc uses <c> elsewhere
- Suggestion, open: IsMetBy_CompletedWhileActive_NotMet also asserts the met side; name it IsMetBy_Completed_MetOnlyOnceCompleted
- Follow-up: QuestLog.Entry now holds QuestDefinition; the save/restore slice must rebuild entries from the catalog
- Follow-up (carried from R1): MapEventStarter.Running() noun-named method
- Follow-up (carried): Trade/ShopScene 296 lines
