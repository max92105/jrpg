# 0044. NPC facing and cutscene movement

Status: accepted
Date: 2026-11-12

Amends ADR 0015's decisions 127 (NPCs stand still) and 128 (always the standing frame, facing down).

## Context
An NPC is a placed object drawn from a field sprite (ADR 0015), standing still and facing down whatever happens.
With the bought character sheets set up per facing (ADR 0043), the game has every facing's frames and a walk per
facing, and the introduction the developer is writing needs more than a statue: an NPC that turns to the player when
spoken to, villagers placed facing their counter or window, and a cutscene in which an NPC walks across the room.
Saves hold the player's location and the world's switches, not where NPCs stand. Plan
`2026-11-12-character-sprites-and-npcs`, decisions 571-579.

## Options
### A. Keep NPCs still and facing down
- Pros: nothing changes.
- Cons: every scene reads as a set of statues; the introduction can't be staged.

### B. A draw-time facing rule and a cutscene step whose positions last for the visit (chosen)
- Pros: what an NPC faces is worked out from what already exists (the running event's source object, the player's
  facing, a facing on the placed object), nothing saved; a `moveNpc` event step walks a placed NPC along a path while
  the event waits, its runtime position kept in the map's active state for the current visit only; the save format
  stays as it is (ADR 0022).
- Cons: a cutscene that must leave an NPC elsewhere for good uses a swap or a once event; a save made after a walk
  puts the NPC back at its placed cell on load.

### C. Runtime facing and positions saved
- Pros: an NPC stays where a cutscene left it across saves.
- Cons: a format bump with its refusal test for a thing no scene needs yet; the editor would place objects the save
  then moves, two sources of truth for one position.

## Decision
**B.** What an NPC faces, the first rule that applies (decision 577): its walk's step while it walks; the player (the
opposite of the player's facing, decision 571) while an event of its own runs, whatever the trigger, the shop or the
Forge it opened included; its last walk's facing this visit; its placed facing (`PlacedObject.Facing`, down when
absent, decision 572). A `moveNpc` step names a placed NPC on the event's own map by uid and a path of one-cell steps
(decision 573); the NPC walks them at the player's walk speed through its walk frames, the event waiting until it
arrives; the walk ignores walls, objects and the player (decision 575); the runtime position and facing live in
`ActiveMap` for the visit (decision 574). The step is a presentation step: a skip pressed on a still runs past it with
the NPC at the path's end (decision 576). The path is checked at load from the placed cell (decision 578).

## Consequences
- **Easier:** a conversation looks like one; a room is staged by placing NPCs with a facing; an introduction walks an
  NPC in without new content types.
- **Harder:** content must know a walked NPC blocks where it stands until the player leaves the map (decision 574).
- **Not covered, each its own later plan:** wandering and routes (randomness through `IRandom`, collision with the
  player, a pause while an event runs); an NPC blocking the player while it walks; saved NPC positions; a moved NPC's
  draw order against the player, which still sorts by its placed row.
