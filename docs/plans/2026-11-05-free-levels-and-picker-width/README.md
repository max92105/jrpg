# Free levels and the picker's width
Status: retro pending

Drafted on 2026-11-05 from what the developer asked after trying the object levels and the sheet zoom:

> "i want to be able to just decide the level not be locked be incremental 0-1-2-3. i want to be able to go -1."

> "the sheet side i want to be able to make it bigger by sliding. i hate the horizontal scroll bar. i want to be able to make this part of the screen larger."

Adjusts the object levels and sheet zoom plan (`done/2026-11-05-object-layers-and-sheet-zoom`): decisions 473 (levels 0 to 7) and 478 (the Level box offers one empty level above the highest painted one, clamps on undo, ignores -1) are replaced by 488 and 490; decisions 472, 474-477 and 479-486 still hold. ADR 0040 gets an amendment (below), not a new ADR. Builds on ADR 0011 (content is the source of truth) and ADR 0012 (editor architecture).

- Numbers: decisions 488-495; no new ADR (ADR 0040 gets an amendment).
- Approved on 2026-11-05 as drafted, with the three departures from the coordinator's recommendations: the content check's order check starts at `ObjectTile.MinLevel` (a bug fix), the picker's minimum is 300 and the canvas side's 400 (decision 494).

The developer's answers in the planning session, taken as decided:
1. A level stays a number **inside the object only**: it orders the object's tiles (lower drawn first); the object is still one sprite on one map layer, not tied to the map's z. The developer picks any level freely, negative ones included (-1 under the base).
2. The part to widen by sliding is **the object composer's picker** (`ObjectComposerWindow.xaml`, a fixed 420 px column today): a drag bar between the picker and the canvas.
3. The zoom stays as built (steps, − and +); no fit-to-width. Widening the picker shows more columns; the horizontal bar shows only while the sheet is wider than the picker.

## Goal
1. **Any level, -7 to 7.** An object tile's level runs from -7 to 7; 0 stays the base and the default, left out of `objects.json` while 0. A level under the base is drawn under it, in the game, its canopies and every editor picture, since every drawer already draws the list lowest level first.
2. **The composer's Level box lists every level.** Fifteen entries, 7 at the top and -7 at the bottom, "0 (base)" chosen when the composer opens; any level can be chosen at any time, empty or not; an undo never changes the choice. Each level holding a painted cell is marked ("2 •").
3. **The picker widens by sliding.** A drag bar between the composer's picker and its canvas; the picker starts at 420 px as today.
4. **Saves are untouched**; `objects.json` keeps its shape (a `level` may now be negative), and every file written before writes back byte for byte.

## Order, and why
1. **FL1 (Core + Data)**: the range and its check first, with the ADR amendment, so what the composer writes in FL2 is what the game reads. It also fixes the order check's start (a tile on -1 listed first would be refused as "listed after a tile on level 0").
2. **FL2 (Editor)**: the canvas and the painting take any level; the Level box becomes the fixed list of fifteen, bound by value. This is where the developer can paint on -1 and on 5 over an empty 4.
3. **FL3 (Editor)**: the marks on painted levels, and the picker's drag bar (merged in: a XAML-only change has nothing headless to test, and the composer window is open anyway for the marks' hand check).

## Guiding rules
- **Agents never open a Mana Seed image** (decision 438, `.claude/rules/content.md`): hand checks are the developer's; tests use the test world and canvases built in code.
- **Undo covers every edit:** strokes on any level stay the composer's own undo steps (decision 461); an undo now never touches the chosen level.
- **The list never rebuilds:** the Level box's fifteen entries are made once per composer, and only their marks change, so the box never clears its selection under the painting (the reason for decision 478's -1 guard goes with it).
- **Slice sizes:** Editor slices about 200 lines, tests counted at about the code's size. Files stay well under 300: `CanvasPainting` 208 → about 195, `ObjectCanvas` 124 → about 126, `ObjectComposerWindow.xaml` 99 → about 106, `ObjectCatalogValidatorTests` 232 → about 255.

## Out of scope
- Levels on the map: the map keeps its own layers (z); an object stays drawn as one sprite by depth (`DrawOrderKey`).
- Levels beyond -7 to 7 (`ObjectTile.MinLevel`/`MaxLevel` are a technical limit; raising them is one constant each).
- The backlog's other composer level wishes: showing or hiding a level, reordering levels, moving painted cells from one level to another, a zone or collision per level (backlog, "From the object levels and sheet zoom plan"). FL2 strikes "a level above an empty one" from that line: this plan delivers it.
- The picker's width remembered between openings or editor runs; a drag bar inside the Tiles panel itself (the main window's side panel already has one, decision 485).
- A fit-to-width zoom or a zoom that follows the picker's width (answer 3).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Free levels
- [x] ADR 0040 amendment: free levels from -7 to 7. Write it from the entry under "ADRs" below (no new ADR).
- [x] **FL1. Levels from -7 to 7 in `objects.json`: `ObjectTile.MinLevel`, the check's range and its order check starting at the lowest level** (Core + Data, about 60 lines) → [slices/FL1.md](slices/FL1.md)
- [x] **FL2. The composer paints any level: the canvas takes -7 to 7, the Level box lists all fifteen (bound by value, never rebuilt), no clamp on undo** (Editor, about 175 lines) → [slices/FL2.md](slices/FL2.md)
- [x] **FL3. The Level box marks the painted levels; the composer's picker widens with a drag bar** (Editor, about 120 lines) → [slices/FL3.md](slices/FL3.md)

## Decisions
- **488. Levels run from -7 to 7** (`ObjectTile.MinLevel = -7`, `MaxLevel = 7`, a technical limit: fifteen levels is far past a wall, a window and a shadow under it). 0 stays the base and the default, left out of `objects.json` while 0. Replaces decision 473's range; a level may still be empty on a cell, the base included.
- **489. A level orders the object's own tiles only:** lower drawn first, a negative level under the base; the object stays one sprite on one map layer (answer 1). Tiles are still listed lowest level first (decision 474), negative ones first; Data refuses a level outside -7 to 7, naming the tile.
- **490. The Level box lists every level, 7 at the top and -7 at the bottom** (it reads like the stack: what is drawn over sits above), "0 (base)" chosen when the composer opens, New object and Edit object alike. Any level can be chosen at any time; an undo or redo never changes the choice. Replaces decision 478's "one empty level above the highest painted", its clamp on undo and its -1 guard (a -1 is a level now, and the list never rebuilds). New object from a rectangle still paints the base.
- **491. A level holding a painted cell is marked "•" in the Level box** ("2 •", "0 (base) •"); the list is fixed per composer and only the marks change.
- **492. The fading follows the chosen level, negative ones included** (decision 479 unchanged): the levels above the chosen one are faded, so with -1 chosen the base and everything over it are faded.
- **493. Edit object reads every level back, negative ones included; Save writes the tiles lowest level first** (decision 480 unchanged otherwise: an empty level is never saved).
- **494. The composer's picker widens with a drag bar** between it and the canvas side: it starts at 420 px, at least 300 (the side panel's own minimum for the same view, decision 485: below it the Cut panel's grid clips); the canvas side at least 400 (the width of its Columns, Rows, Erase, Undo and Redo row). Not remembered between openings.
- **495. The zoom stays as built** (decisions 481-484): widening the picker shows more of the sheet at the same cell size; its horizontal bar shows only while the sheet is wider than the picker (the scroll viewer's `Auto`, unchanged).

## Open design questions (developer decides)
None open.

## ADRs
- **ADR 0040 amendment, free levels** (decisions 488 and 489; an "Amendment (2026-11-05): free levels" section appended to `docs/decisions/0040-object-tile-levels.md`, its Decision paragraph left as written; the README's 0040 line updated to "-7 to 7"; written in FL1 from this entry):
  - **What changes:** a level runs from -7 to 7 (`ObjectTile.MinLevel`, `MaxLevel`), 0 the base and the default, still left out while 0; a negative level is drawn under the base. The tiles are still listed lowest level first, so every drawer and the canopy bake keep drawing the list in order; Data refuses a level outside the range and a lower level listed after a higher one, naming the tile. The composer chooses any level freely.
  - **Why an amendment, not a new ADR:** the decision's shape (a level on each tile, the list kept in level order, absent at 0) and its options are unchanged; only the range moves, and every file valid before stays valid.
  - **Consequences:** an older build refuses an `objects.json` with a negative level, naming the file (one developer, one build: accepted, as for the field itself); the test world and the repository's files are unchanged.
