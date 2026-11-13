---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice MV3 (moveNpc step in the editor: NpcChoice, StepChoices.NpcChoices,
EventStepViewModel.MoveNpc partial, Move NPC buttons in both event dialogs, EventStepEditor fields). 2026-11-13.
Round 1. Verdict: ready to commit.

- MV2 carried: the trap Warning is fixed (decision 593, MapCollision.CanMoveTo lets a body leave a polygon); MV2's
  three Suggestions not carried (committed slice, optional); MV2's Follow-up (From throws on moveNpc) fixed by MV3.
- Suggestion (open): NpcChoice.cs:3-4 summary says value equality makes an opened step select its entry; the combo
  selects by SelectedValuePath=Uid (string), equality only serves the test's Assert.Contains.
- Suggestion (open): "unsaved placements included" (EventEditingViewModel.NpcChoices) has no test placing an NPC on the
  open map and seeing it offered.
- Follow-up: deleting a placed NPC that another event's moveNpc step names is only caught by ContentWriter's
  validation on save; the editor could warn at delete time.
