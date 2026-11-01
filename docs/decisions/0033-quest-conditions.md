# 0033. Quest conditions gate whole events

Status: accepted
Date: 2026-11-01

## Context
An NPC's event may already be gated on the object's look (`onlyWhenObjectId`), the leader (`onlyWhenLeaderId`,
decision 185) and a "once" mark (ADR 0006), and the first event of an object that may start wins. Quests (ADR 0032)
need an NPC's lines to change as a quest advances: before it is taken, while it is at a stage, once it is done. The
developer answered the plan's questions on 2026-10-29 (decisions 386 and 389).

## Options
### A. A quest condition on the whole event, beside the leader and object gates
- Pros: one event per quest situation, read as the dialogs already read the other two gates; the "first event that may
  start wins" rule needs no change; every gate check lives in one place.
- Cons: an NPC with many situations holds many events; a condition can't branch inside one event.

### B. Per-step conditions or a branch step inside one event
- Pros: one event per NPC, with its lines chosen inside.
- Cons: a new step kind that reads state and skips steps, which ADR 0006 and ADR 0016's skipping would both have to
  learn; the dialogs would need a tree, not a list.

### C. A separate list of quest events apart from the map's
- Pros: quest content in one file.
- Cons: a second way to attach an event to an object or a cell, and a second validator for the same step list.

## Decision
A.
- **`MapEvent.OnlyWhenQuest`** is a `QuestCondition`: a quest id, one of five kinds (decision 389) and, for the two
  stage kinds, a stage id. **Not started**: never started. **Active**: started and not completed, at any stage. **At a
  stage**: active at that stage. **Reached a stage**: at it or past it in the quest's current stage order, or completed;
  an advance back (decision 388) un-reaches the later stages. **Completed**.
- **One gate**: whether an event may start now is `ActiveMap.CanStart(mapEvent, leaderId)`: the leader, a "once" event
  not already done, and the quest condition met by the world's log. The arrival, cell, touch and interact paths all ask
  it; the object's look (`MapEvent.CanRunOn`) stays apart, since cell and arrival events have no object.
- **The first event that may start wins**, as before, so an NPC holds one event per quest situation in the order the
  author lists them; content refuses an object event that can always start and would hide every later event of its
  activation on that object (the no-shadowing rule, decision 389's review).
- The condition is content: validated against `quests.json` with the map, edited as three boxes in the event dialogs
  (the quest, the kind, the stage), and left out of a file while null so every existing map writes back byte for byte.

## Consequences
- An NPC's lines follow a quest without a new step kind; the interaction prompt is refreshed after an event ends, so a
  "Talk" that just advanced the quest already points at the next event.
- A condition names a stage by id and reads its position only when checked, so inserting a stage in content changes no
  saved state and no condition; a renamed stage is caught by the map validator.
- Branching inside one event, failed quests and per-step conditions are later work that reuses `QuestCondition.IsMetBy`.
