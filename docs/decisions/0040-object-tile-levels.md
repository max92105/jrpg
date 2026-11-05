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
