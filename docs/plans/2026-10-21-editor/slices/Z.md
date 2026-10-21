# Z. Editor session check

Part of [the editor plan](../README.md). Design: [design.md](../design.md).

- **Z. Editor session check** (manual): one working session in the editor, from a fresh sheet to a walked map with an NPC, then the game. Run it on the repository's content, committing nothing until the end, so `git diff` shows exactly what the session wrote.
  - [ ] **Window (E1, E2, E13a, E13b):** no empty strip at the top; the map list shows names and switches in one click (unsaved prompt still works); the layer list chooses a layer and adds one at a new Z; every menu entry works and F1 lists every gesture used below; the dark theme, mode tool bars and splitters behave.
  - [ ] **Sheets (E5a-E7):** import a PNG from outside `content/`; it is listed "(not set up)"; set it up by columns and rows; the grid lines up with the art. Re-cut a used sheet on a `--content` copy: the warning names maps and objects; a cut leaving a tile outside is refused; a too-small replacement image is refused.
  - [ ] **Painting and layers (E3a-E3d, E1):** paint from cells that were never tiles, with brush, rectangle, fill and fill layer, on the ground, a detail layer and the above-player layer; Alt + click picks a tile back.
  - [ ] **Walkability (E4a):** mark a few tiles blocked with Mark blocked; the crosses show; "Unsaved changes" shows; switching maps keeps them.
  - [ ] **Undo (E4b):** Ctrl+Z and Ctrl+Y walk back and forth through a mix of paint strokes, blocked marks, a cut, a shape edit and a new object, newest first; an undo that would remove something in use is refused with its name.
  - [ ] **Overlay (E8):** Show collision shows the blocked cells, green outlines and blue zones, and follows every change and undo made in this session.
  - [ ] **Shapes (E9-E11b):** edit an existing house's collision outline and a tree's canopy zone with every tool (add, insert, drag, remove, rectangle, whole object, snap, zoom), opened from the panel and from a placed object; Cancel leaves them unchanged; OK updates the map.
  - [ ] **New object (E12a, E12b):** make an object from a rectangle of sheet cells, draw its shapes, place it.
  - [ ] **NPCs (N1-N3):** make an NPC with the placeholder sprite, place two, give each a Talk event with its own lines, adjust one's outline.
  - [ ] **Save:** Ctrl+S once; `git diff` shows only the session's changes, in CRLF, and every tile and object id the maps use is in the catalogs; a deliberately broken shape or cut can't be saved and names the problem.
  - [ ] **Close and reopen the editor:** everything is as saved; closing with unsaved catalog changes asks.
  - [ ] **Game (F5):** the new sheet's tiles draw; the above-player layer covers the player; blocked tiles block; outlines block where drawn; canopies cover the player where drawn and nowhere else; both NPCs stand, block at their feet and say their own lines.
  - [ ] Revert the session's content changes (or keep the useful ones as their own commit).
  - [ ] Fill in "Done, with deviations" on each slice.
