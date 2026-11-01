# 0032. Quest progress is its own log

Status: accepted
Date: 2026-11-01

## Context
The game has no notion of story progress: events run on "once" marks, swapped objects and the leader (decision 185),
and nothing tracks what the party is doing. The quests plan adds quest progression that events advance and gate on, a
page that lists it, and saves that keep it (format 7). Story state that isn't a quest (a boss beaten for good, a door
the plot opened) has no home either. The developer answered the plan's questions on 2026-10-29 (decisions 386-398).

## Options
### A. A quest log: stage ids per quest, in `WorldState`, changed only by the three quest steps
- Pros: every state's rule lives in one Core type (decision 388); progress is named by stage id, so content inserting
  a stage before the current one moves nothing saved; the page reads one list; conditions read the same log.
- Cons: story state that isn't a quest has to be phrased as a quest stage until flags exist.

### B. General story flags (named booleans or numbers), quests built on top of them
- Pros: any state fits; a quest is a convention over flags.
- Cons: no rule for steps out of order, no stage order for "reached", no main-or-side kind; the page would need a
  second schema to know what a flag means; two places to keep in step once quests also exist.

### C. Progress as stage positions (indexes) per quest
- Pros: "reached" is a number compare; smaller saves.
- Cons: inserting a stage in content shifts every saved quest to another stage (`.claude/rules/content.md`: ids, never
  indexes); a renamed stage can't be reported.

## Decision
A.
- **`Quests/QuestLog`** holds each started quest's stage id and whether it is completed, and changes only through
  `Start`, `AdvanceTo` and `Complete`, with decision 388's rules for every state: a start on a started or completed
  quest does nothing; an advance on a quest not started starts it at that stage; an advance may move back to an earlier
  stage; an advance on a completed quest does nothing; a complete on a quest not started completes it at its last stage;
  a complete on a completed quest does nothing. The commands take the `QuestDefinition`, so the log never looks a quest
  up and `World` needs no catalog.
- **Stages are kept by id**; only "reached" (ADR 0033) compares positions, read from the quest's current stage order
  in content, so an advance back un-reaches the later stages (decision 389's flagged reading).
- **The log lives in `World/WorldState.Quests`**, beside the events done and the objects swapped, and saves keep it in
  format 7 (`SaveGame.Quests`, format 6 refused, ADR 0022). A saved quest at a stage content no longer has goes back to
  its first stage, and a saved quest content no longer has is forgotten, both reported by the repair (decision 396).
- **Story state that isn't a quest is a main-quest stage for now** (decision 398): a boss beaten for good or a door the
  plot opened advances the main quest; general flags get their own plan beside the log.

## Consequences
- Every quest rule has one home and one test file; events, the page and the saves read the log, never each other.
- Renaming a stage in content resets the saved quests at it to their first stage (reported), the price of ids over
  positions; the Quests window keeps ids fixed once typed (decision 394).
- A story beat that isn't a quest costs a main-quest stage until flags exist; the extension point is a sibling of the
  log in `WorldState`, not a change to it.
