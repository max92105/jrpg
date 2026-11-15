# 0043. Field sprite sheets as content

Status: accepted
Date: 2026-11-12

Amended by ADR 0054: a facing may read a sequence of cells or mirror another facing, overriding its row.

Amends ADR 0015: its decisions 130 (one shared placeholder sprite) and 132 (one cell, 48 px frames in the player
sheet's layout), and its consequence that the game and the editor each read the frame layout from a named constant.

## Context
ADR 0015 made an NPC an object drawn from a field sprite, every sprite laid out like `player.png`: 48 px square
frames, 8 walk frames per row, one row per facing, a layout fixed in `Data/Art/FieldSpriteLayout` and read by the art
check, the game's `FieldSpriteFrame` and `WalkAnimation`, and the editor's `TileImages`. The developer bought character
sheets of another shape (decision 438: no agent opens one): square cells, about 4 columns, the top four rows a walk
cycle of 4 frames with one facing per row, extra pose rows under them. Frame size, columns, which row faces where, the
frame time and the drawn size now differ per sheet, and the developer wants to set them in the editor rather than
rename or repaint sheets. Plan `2026-11-12-character-sprites-and-npcs`, decisions 560-567.

## Options
### A. Keep the fixed layout
- Pros: nothing changes.
- Cons: every bought sheet would be repainted into the player's grid by hand; a 4-frame walk can't be expressed.

### B. Layout fields on each object (and on the new game)
- Pros: no new file.
- Cons: the layout belongs to the image, not to the object: two NPCs sharing a sheet would repeat it, and the new game
  would carry sprite fields of its own; the editor would edit the same numbers in two dialogs.

### C. Frames as tiles (ADR 0015's option C)
- Pros: the tile tools exist.
- Cons: frames don't fit tile cells, and facing and walk need a grammar tiles don't have.

### D. A per-image record in `field-sprites.json` (chosen)
- Pros: one record per image, keyed by its file name, so `objects.json` and `new-game.json` keep their
  `fieldSpriteFile` unchanged (decision 560); the player's sheet is one record among others with today's numbers
  (decision 561), so nothing moves on screen; the walk reuses ADR 0039's shape (decision 566), one rule that Core
  already has; the editor sets the record up like a tilesheet's cut.
- Cons: a second file to keep in step with the sprite folder; a sprite named without a record fails the load
  (decision 567), which is the point.

## Decision
**D.** `content/field-sprites.json` holds one `FieldSpriteSheet` per field sprite image: `file`, `frameWidth` and
`frameHeight` (decision 565), a whole-number `scale` (decision 562: a frame draws at `frame * scale` pixels, centered
on its cell, its feet line on the cell's bottom edge), `feetPixels` (decision 563: sheet pixels between the frame's
bottom and the feet, 0 and left out by default) and `walk`: the row each facing reads (`FacingRows`) and `frames`, a
`TileAnimation` running rightward from column 0 on that row (decision 566; a sheet stands on column 0, decision 564).
`ContentCatalog.FieldSprites` is keyed by file name, exact case; the loader refuses an object or a new game naming a
sprite without a record, naming it (decision 567). Core's `Sprites/` folder owns the record and the arithmetic (which
cell a facing shows at a time, the frames a sheet needs, where a sprite stands over its cell); the game, the editor and
the art check read it there.

## Consequences
- **Easier:** a bought sheet works as it is, set up once in the editor (the plan's SI slices); the player and NPCs
  share one drawing path; the art check reads each sheet's own frames.
- **Harder:** a new sprite needs its record before the game starts; the editor's New NPC offers only set-up sprites.
- **Watch:** `player.png`'s record must keep today's numbers (48 px, scale 1, rows down 0, left 1, right 2, up 3, 8
  frames at 100 ms) until the player's sheet is replaced in its own plan; the frozen test world carries its own copy of
  the file (ADR 0037), so a record changed in `content/` never moves a test.
