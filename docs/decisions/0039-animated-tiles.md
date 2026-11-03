# 0039. Animated tiles

Status: accepted
Date: 2026-11-03

## Context
The Mana Seed Forest Autumn pack has 13 animations, all Tiled `.tsx` animations whose frames run rightward from the
animated tile's cell with one duration each (the developer's count: waterfall rows 0-3, 8 frames at 70 ms; rows 4-9, 4
frames at 140 ms; the sparkles, 3 tiles of 4 frames at 600 ms). They are overlays painted on their own map layers over
water and cliffs. The tall grass effect sheet is a cutting effect with 32 px frames, not a tile animation, and stays out
(decision 471). Nothing but the player's walk animates today; Core never reads the clock; the game (`TileAtlas`,
`MapView`) and the editor (`TileImages`, `MapDrawing`) draw tiles each their own way. Plan
`2026-11-03-object-composer-and-animated-tiles`, decisions 463-471.

## Options
### A. An explicit `frames` list of cells
- Pros: any cell of the sheet; per-frame times fit beside it.
- Cons: null entries and cell references to check; no animation the pack has needs it. Kept as a later optional field.

### B. A different time for each frame
- Pros: Tiled allows it.
- Cons: none of the pack's animations uses it.

### C. Per-cell start offsets
- Pros: a field of water doesn't move in unison.
- Cons: a rule to define (a fixed offset from the cell's position, no randomness); nothing asks for it yet.

### D. An `animations.json` naming tile ids per frame
- Pros: tiles stay as they are.
- Cons: a second file to keep in step; frames become tiles of their own.

### E. Frames on other sheets, or animation on a map cell or layer
- Pros: freedom.
- Cons: a sheet's removal would orphan frames; an animated tile couldn't sit in an object.

### F. A frame count and a time on the tile (chosen)
- Pros: the shape of every animation the pack has; the tile's own cell stays the first frame, so the sheet view, the
  composer and the canopy bake are right without reading the animation; frames leave with their sheet.
- Cons: frames must sit to the right on the same row; a sheet of another shape needs option A.

## Decision
A tile in `tiles.json` may carry `animation`: `frameCount` (at least 2 frames, the tile's own cell then the cells to its
right on the same row) and `frameMilliseconds` (a whole number from 1, one time for every frame); it is left out of the
file when absent. `Core/Maps/TileAnimation.FrameAt(elapsed)` gives the frame, `(elapsed / frame time) mod frameCount`,
refusing a negative time; `TileDefinition.ColumnShownAt(elapsed)` is the column drawn; every copy is in step. The game's
elapsed time is the map view's, advanced by the update's delta and restarted on entering a map; the editor's moves on
with WPF's render tick, for its previews and the Animate panel (its map shows the first frame). Data validates the
shape; the startup art check (`ArtReferenceCheck`) checks the last frame's cell against the image, and an import refuses
an image too small for it. Canopies bake the first frame. The editor imports a Tiled `.tsx` file's animations when they
have this shape and refuses the file otherwise. Saves are untouched.

## Consequences
- An animated tile paints like any tile, in the game and the editor; frames leave with their sheet in a removal or a
  purge.
- A re-cut may push the last frame outside the grid: the editor draws the missing mark and the startup check names it.
- An explicit list, per-frame times or offsets would extend the record with optional fields, not replace it.
- A `.tsx` of another shape is refused rather than half-imported.
- The test world's frozen `tiles.json` needs no change: the field is optional.
