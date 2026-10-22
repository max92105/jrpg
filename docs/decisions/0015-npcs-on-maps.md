# 0015. NPCs on maps

Status: accepted
Date: 2026-10-22

## Context
The editor plan (`docs/plans/2026-10-21-editor/`, slices N1-N3) adds NPCs: characters the player sees on a map and talks to. Nothing models them today. A map already places objects (`PlacedObject`, drawn from sheet tiles, with a collision outline). Objects already carry events, and an `Interact` event with a `Dialogue` step is a conversation. The only field sprite is `content/art/field-sprites/player.png`: 48 px frames, 8 per row, one row per facing.

The question is **how an NPC is modelled in content**.

Constraints:
- solo developer;
- content only in `content/`, written through Data (ADR 0011);
- no save format change unless needed;
- no NPC art yet (decision 69): a placeholder now, real art later without reworking content.

## Options
### A. An object drawn from a field sprite
`ObjectDefinition` gains an optional `fieldSpriteFile`, a file in `content/art/field-sprites/` laid out like `player.png`. Such an object has no tiles and no above-player zones; the game and the editor draw its standing frame. An NPC is that object, placed on a map, with an `Interact` event holding a `Dialogue` step.
- Pros:
  - placing, collision outlines, interaction, events, "once" flags in saves and the shapes window all work unchanged;
  - one optional field; old content loads as it is, so no migration.
- Cons:
  - a placed object can't move. Movement (question 127) needs runtime positions, a save change and its own ADR.
  - "is an NPC" is implicit (has a field sprite), not a type.

### B. A separate NPC catalog and placed NPCs
`npcs.json` (name, sprite, dialogue) and `MapLayout.Npcs`.
- Pros: room for names, schedules and movement from the start.
- Cons: it duplicates placing, collision, interaction and events in Core, Data, Game and the editor, for features nobody has asked for yet.

### C. No format change: NPC art as a tilesheet, NPCs as tile objects
- Pros: no code outside the editor.
- Cons:
  - field-sprite frames don't fit tile cells, so real walk sheets wouldn't drop in;
  - facing (question 128) can't be added later without redoing it.

## Decision
**A**, chosen by the developer (2026-10-22). The content rules:
- An object has tiles or a field sprite, never both.
- A field-sprite object has no above-player zones.
- The loader and the art check validate the file like the player's sprite.

**Placeholder art (decision 69):** every NPC names one shared file, `npc_placeholder.png`. Real art later is then a new file and a one-field change per NPC object.

**The developer's answers (2026-10-22):**

| # | Question | Decision |
|---|---|---|
| 127 | Movement | NPCs stand still. Wandering or routes would be a later plan with its own ADR. |
| 128 | Facing | Always the standing frame, facing down. |
| 129 | Collision | A default outline over the lower half of the cell, editable in the shapes window. |
| 130 | Placeholder sprite | `npc_placeholder.png`, a plain silhouette in `player.png`'s frame layout (not a copy of the player). |
| 131 | Objects per character | One NPC object per character (`villager`, `guard`), each naming its sprite file. |
| 132 | Size and frames | One cell, 48 px frames in the player sheet's layout. |

## Consequences
- **Easier:**
  - An NPC needs only one new field plus drawing code in the game and the editor (N1, N2).
  - The shapes window, events and dialogue editors serve NPCs as they are.
- **Harder:**
  - Movement and turning to face the player don't fit a placed object; they need a new ADR when wanted.
  - The game and the editor each read the field sprite's frame layout, since the editor can't reference the game. A test or a named constant keeps them in step.
- **Watch:** the start map's NPC (N1) must sit off the paths that the scenario and content tests walk.
