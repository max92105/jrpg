# 0040. Object tile levels

Status: accepted
Date: 2026-11-05

## Context
An object is one tile per cell (decision 456; `ObjectCatalogValidator`: "an object has one tile per cell"), drawn in its
list's order by the game (`MapView.DrawObject`) and the editor (`MapDrawing.DrawObjectAt`); its canopy is baked once by
copying each tile's pixels (`ObjectSpriteBaker.CanopyPixels`), overwriting what was there. The Mana Seed half-timber
sheet draws doors and windows as transparent overlays meant to sit on wall cells, so an object needs several tiles on
one cell. Content files have no format version (only saves do, ADR 0022); saves name objects by id and never hold their
tiles. Plan `2026-11-05-object-layers-and-sheet-zoom`, decisions 472-476.

## Options
### A. The list's order alone, several tiles allowed per cell
- Pros: no new field.
- Cons: a level painted with a hole under it reads back shifted down; nothing tells a stacked cell from a duplicate
  entry.

### B. A separate `overlays` list on the object
- Pros: the base list stays as it is.
- Cons: two fixed levels; every drawer reads two lists.

### C. The tiles as a list of levels, each a list of tiles
- Pros: the stack is the file's shape.
- Cons: rewrites every object in every file, the frozen test world's included.

### D. A level on each tile, the list kept in level order (chosen)
- Pros: old content means level 0 throughout and writes back unchanged; drawing the list in order is drawing the stack
  bottom first, with no sort.
- Cons: the list's order becomes a rule the content check enforces.

## Decision
Each object tile carries `level` (0 to 7, `ObjectTile.MaxLevel`; 0 the base), left out of `objects.json` while 0. An
object has at most one tile per cell and level and lists its tiles bottom level first; Data refuses anything else,
naming the tile. A level may be empty on a cell, the base included. Every drawer draws the list in order, so the stack
is drawn bottom first; the canopy bake composes each tile over the pixels under it (premultiplied "over", as the game
draws). The editor's composer paints one level at a time. Saves are untouched.

## Consequences
- Old content is level 0 throughout and writes back byte for byte; the test world needs no change.
- A hand-edited file listing a higher level first is refused with the tile named rather than drawn wrong.
- The composer's canvas holds cells by cell and level.
- A later wish (a level hidden in the editor, zones per level) adds to the composer, not the format.

## Amendment (2026-11-05): free levels
Plan `2026-11-05-free-levels-and-picker-width`, decisions 488 and 489. After painting with the composer, the developer
wanted to choose any level, including one under the base. A level now runs from -7 to 7 (`ObjectTile.MinLevel`,
`MaxLevel`), 0 still the base and the default, left out of the file while 0; a negative level is drawn under the base.
Tiles are still listed lowest level first, so every drawer and the canopy bake keep drawing the list in order; Data
refuses a level outside the range and a lower level listed after a higher one, naming the tile. A level only orders the
object's own tiles: the object stays one sprite on one map layer. The composer chooses any level freely.

The decision's shape and its options are unchanged, only the range moves, so this is an amendment and not a new ADR:
every file valid before stays valid. An older build refuses an `objects.json` with a negative level, naming the file.
