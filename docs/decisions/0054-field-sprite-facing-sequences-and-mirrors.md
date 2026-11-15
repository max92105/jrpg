# 0054. A field sprite facing's sequence of cells or mirror, overriding its row

Status: accepted
Date: 2026-11-15

Amends ADR 0043: a facing no longer has to read a whole row from column 0.

## Context
ADR 0043 gave each field sprite image a record in `field-sprites.json`: frame size, scale, feet, and a walk of one
row per facing (`walk.rows`) with `walk.frames.frameCount` frames from column 0 at one frame time. Bought sheets don't
all follow that shape. `npc merchant A v00.png` lists row 2 for both Up and Right, since the record can't say "Right is
Left mirrored", and a sheet whose walk reads "0, 1, 2, 1" or starts mid-row can't be described at all. The developer
asked to choose any sequence of cells on the sheet to build a facing's animation (the editor push, roadmap row 5,
design 3.5). Constraints: every record in `content/` and the frozen test world loads unchanged; the record grows
additively so the work splits into small slices; Core stays free of nullable ripples; no save format change.

## Options
### A. A sequence overriding the row (chosen)
`walk.rows` keeps its four required ints in every record; an optional `walk.sequences` gives any facing
`{ "cells": [ { "column": c, "row": r }, ... ] }` or `{ "mirrorOf": "<facing>" }`, which overrides its row.
- Pros: every record loads as it is; the row form stays the short way for well-made sheets (`player.png`, most bought
  sheets); nothing nullable spreads through Core, Data or the editor; the work lands in small slices (Core, then Data,
  then the mirror, then the window).
- Cons: a row under a sequence, and `frameCount` when every facing has a sequence, are kept but unread (decision 767);
  the window keeps the row to restore it when Row is chosen again.

### B. One form for every facing, every record converted
Each facing is `{ "cells": [...] }` or `{ "mirrorOf" }`, nothing else.
- Pros: one reader and one form.
- Cons: in the writer's layout each cell takes four lines, so `player.png`'s record grows to about 140 lines and
  `field-sprites.json` to about 400; the frozen test world's file must change; the swap is one atomic slice across
  Core, Data, the editor and about 12 test sites, and the editor can't open a record its row boxes don't express until
  the picker lands.

### C. A start column and a count per facing
- Pros: small.
- Cons: covers a run starting mid-row, not "0, 1, 2, 1", a run crossing rows or a mirror: it misses the request.

### D. Repaint the sheets into the fixed layout
- Cons: ADR 0043 refused it; the developer asked for the opposite.

## Decision
Option A (decisions 761-770, 776). A facing shows its row (today's form), a sequence of cells picked anywhere on its
sheet in order, repeats allowed, or the mirror of another facing, flipped left to right in the same area. A sequence
holds at least one cell; a mirror names a facing that isn't itself or another mirror (one hop, so no loop). The
standing frame is a facing's first. One frame time, `walk.frames.frameMilliseconds`, paces every facing over its own
frame count. A cell's `column` and `row` are both required, through a contract modifier (the serializer builds a
struct through its parameterless constructor, so a missing member would read as 0). Core gives a facing's frame as a
`SpriteFrame` (its cell and whether it is mirrored): the game flips with `SpriteEffects.FlipHorizontally`, the editor
with a horizontal flip of the cropped frame. `FacingSequence` compares by content, so equal records stay equal for the
editor's changed-record check and frame cache.

## Consequences
- Old records load and write back byte for byte: `sequences` is left out while null.
- What a record needs of its image counts only what each facing reads: the rows of facings without a sequence, the
  frame count while one of them exists, and every sequence's cells; a mirror adds nothing.
- Kept but unread: a facing's row under its sequence, and `frameCount` once every facing has one (decision 767).
- The Character sprites window gains a "Shows" box per facing (Row, Cells, Mirror of) and picking cells on the image;
  a frame time per facing, inserting or reordering picked cells, and sequences for tiles, enemy idle loops or effect
  strips stay out (the plan's "Out of scope").
- `ObjectTile`'s and `GridCell`'s members have the same missing-member gap a cell had: they can join the contract
  modifier's list later (a backlog line).
