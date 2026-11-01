---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice QS (startQuest/advanceQuest/completeQuest steps, Core + Data). Round 1. Verdict: Ready to commit.
verify.ps1 all pass (3186 tests).

- Suggestion, open: the two-stage `_errand` QuestDefinition is now written out in QuestLogTests, EventRunQuestTests and MapEventValidatorQuestTests (~165-column line each); one shared test fixture, one stage per line
- Suggestion, open: Start_AdvanceQuestStep_QuestAtThatStage / Start_CompleteQuestStep_QuestCompleted don't assert run.IsFinished (only the start test pins "doesn't wait")
- QL suggestions (long line, AdvanceTo unknown-stage on a completed quest): not in this diff, left to the developer
- Follow-up (carried): Trade/ShopScene 296 lines
