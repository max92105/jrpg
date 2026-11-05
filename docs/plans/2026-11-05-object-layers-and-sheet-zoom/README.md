# Object levels and sheet zoom
Status: approved

Drafted on 2026-11-05 from two requests the developer made while painting objects with the composer:

> "i need to be able to layer on object also"

> "i need to be able to better interact with the tile page. the same side bar to make it bigger smaller"

Continues the object composer and animated tiles plan (`done/2026-11-03-object-composer-and-animated-tiles`): decision 456 kept one tile per object cell and left stacking to "its own later plan" (backlog, "From the object composer and animated tiles plan"); decisions 444 (16 px sheets drawn 3x in the 48 px cell), 457 (the composer's picker is the Tiles panel's own sheet view) and 461 (strokes undo inside the composer) still hold. Builds on ADR 0011 (content is the source of truth), ADR 0012 (editor architecture), ADR 0014 (editing shared catalogs) and ADR 0037 (the frozen test world).

- Numbers: decisions 472-487 and ADR 0040.
- Approved on 2026-11-05 with every recommendation taken and Q1 answered as the plan read it (decision 487: zoom the sheet's picture; the side panel's splitter stays). The devil's advocate's findings were applied: the levels model became one dictionary keyed by cell and level, with the Level box offering one empty level above the highest painted one, so Add level, its undo step and its rules left and the former OL3 and OL4 became one slice (OL3; the window slice is now OL4); the level setter ignores a value outside the choices (a combo box pushes -1 when its list is rebuilt); the canopy is hand-checked through an above-player zone in OL4; the composer's canvas draws pixels sharp (SZ3); the new tests go in new files beside test files already past 250 lines; OL2's two pins that passed on day one left.

## Goal
1. **An object cell can hold a stack of tiles.** Each tile of an object has a level on its cell: 0 the base, each higher level drawn over the lower ones. A window or a door from the half-timber sheet is painted on level 1 over a wall cell on level 0, and the wall shows through its transparent pixels. The game, its canopies and every editor picture draw the stack bottom first.
2. **The composer paints levels.** A Level box over the canvas chooses the level strokes paint and erase; it always offers one empty level above the highest painted one; the levels above the chosen one are drawn faded. Edit object reads a stacked object back level by level.
3. **The sheet view zooms.** The Tiles panel's sheet (and the composer's picker, the same view) gets a zoom bar (−, the cell size, +) and Ctrl + wheel toward the mouse; the composer's canvas zooms the same way. Clicks, drags and marks stay right at every size.
4. **Saves are untouched.** Only `objects.json` gains an optional field; content without it loads and writes back byte for byte.

## Order, and why
1. **Phase 1: levels inside an object (ADR 0040, OL1-OL4).** The format first, with its validation and its writer (OL1), so the content the composer writes later is what the game reads. Then the drawing (OL2): the game draws the list in order already, so only the canopy bake changes (it overwrote pixels); the editor's pictures are pinned by a test. Then the composer: the canvas's levels and the painting's chosen level (OL3), then the window's Level box and the faded levels (OL4), rules before the window as every editor panel is split. The phase's full hand check is OL4's: it makes the first stacked object, gives it an above-player zone, and looks at it in the editor and the game.
2. **Phase 2: the sheet zoom (SZ1-SZ3).** The zoom rule as a pure type first (SZ1), then the Tiles panel's sheet view (SZ2, which the composer's picker gets for free), then the composer's canvas (SZ3).

The phases don't depend on each other. If the developer wants the zoom sooner, phase 2 can run first: SZ3 then adds its grid-line parameter to `ObjectCanvasDrawing.Draw` before OL4 adds the level, and OL4's slice file says so.

## Guiding rules
- **Agents never open a Mana Seed image** (decision 438, `.claude/rules/content.md`): hand checks are the developer's; tests use the test world's sheets and catalogs built in code.
- **Undo covers every edit:** strokes on any level are the composer's own undo steps (decision 461); Save stays one catalog undo step.
- **No lockout on missing art** (decision 442): a level's cell of a sheet whose image is missing draws the missing mark, as today.
- **The zoom rule is pure and tested** (`Viewport/CellZoom`), the views only apply it: the same split as `ViewportTransform` for the map.
- **Slice sizes:** Editor slices about 200 lines, tests counted at about the code's size, never past 300; no changed file crosses 300 lines. `Tiles/TileBrowserViewModel` (325) takes two lines (SZ2); `Shell/MainWindow.xaml` (296) and `Catalogs/CatalogDraft` (298) are not touched; `Objects/Composing/CanvasPainting` (165) ends about 200, `ObjectCanvas` (103) about 140. Test files past 250 lines get a sibling instead of growing: `ObjectComposingTests` (261) → `ObjectComposingLevelTests`, `TileBrowserViewModelTests` (496) → `TileBrowserZoomTests`.

## Out of scope (follow-ups, in `docs/plans/backlog.md`, "From the object levels and sheet zoom plan")
- Showing or hiding a level, reordering levels, moving painted cells from one level to another, adding a level above an empty one (the Level box offers one empty level on top; an empty top level is never saved).
- A level with its own above-player zone or collision: zones and the outline stay the object's, over the whole stack.
- Levels on the map: the map keeps its own layers (z); an object stays drawn as one sprite by depth (`DrawOrderKey`).
- The zoom remembered between editor runs, keyboard shortcuts for the sheet's zoom, zoom steps below the sheet's own pixels (an overview that drops pixel rows).
- The Animate panel's preview following the zoom (decision 486).
- The map's own zoom (wheel, 1.15 per notch) and the Shapes window's zoom: unchanged.
- Exact device pixels for the marks and the grid at 125 % or 150 % Windows scaling: "one screen pixel" is one WPF unit (DIP), as today.
- The editor's map animating (backlog, decision 466).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: levels inside an object
- [x] ADR 0040: object tile levels. Write it from the entry under "ADRs" below (decisions 472-476).
- [x] **OL1. `ObjectTile.Level`: a tile's level on its cell in `objects.json`, left out at 0; one tile per cell and level, listed bottom level first, levels 0 to 7; written back** (Core + Data, about 175 lines) → [slices/OL1.md](slices/OL1.md)
- [x] **OL2. The stack drawn bottom first: the game's canopy bake lays each tile over the one under it; the editor's object pictures pinned on a stacked object** (Game + Editor, about 110 lines) → [slices/OL2.md](slices/OL2.md)
- [x] **OL3. The composer's levels: `ObjectCanvas` keyed by cell and level, `CanvasPainting.Level` and its choices (one empty level on top), Save and Edit with a stack** (Editor, about 235 lines, 25 of them one-token call-site churn) → [slices/OL3.md](slices/OL3.md)
- [x] **OL4. The composer window's Level box; the levels above the chosen one faded; the first stacked object made, zoned and seen in the game** (Editor, about 90 lines) → [slices/OL4.md](slices/OL4.md)

### Phase 2: the sheet zoom
- [ ] **SZ1. `CellZoom`: a cell's shown size in steps (16 to 192 px, 48 the map's), in and out within the bounds, the scale for a source cell size, the scroll offset that keeps the point under the mouse** (Editor, about 180 lines) → [slices/SZ1.md](slices/SZ1.md)
- [ ] **SZ2. The Tiles panel's sheet zooms: the zoom bar over it, Ctrl + wheel toward the mouse, marks one screen pixel wide at every size; the composer's picker shares it** (Editor, about 185 lines) → [slices/SZ2.md](slices/SZ2.md)
- [ ] **SZ3. The composer's canvas zooms: its own zoom and bar, Ctrl + wheel toward the mouse, sharp pixels, grid lines one screen pixel wide, strokes on the right cell at every size** (Editor, about 85 lines) → [slices/SZ3.md](slices/SZ3.md)

## Decisions
- **472. An object tile has a level** (ADR 0040): `ObjectTile.Level`, a whole number on the tile's cell, 0 the base; a higher level is drawn over a lower one. `objects.json` writes it as `level` on a tile, left out while 0, so every object written before writes back byte for byte.
- **473. Levels run from 0 to 7** (`ObjectTile.MaxLevel`, a technical limit: eight levels is far past a wall, a window and a sign). A level may be empty on a cell, the base included: an overlay may stand alone on a cell.
- **474. One tile per cell and level, listed bottom level first.** `objects.json` lists an object's tiles by level, lowest first (row by row within a level, as the composer writes them); the content check refuses two tiles on one cell and level, a level outside 0 to 7, and a lower level listed after a higher one, naming the tile. Drawing the list in order is drawing the stack bottom first, so the game's `MapView` and the editor's `MapDrawing` need no sort and no allocation.
- **475. No version for content files.** Content files carry no `formatVersion` (only saves do, ADR 0022): an `objects.json` without `level` is every tile on level 0, which is today's meaning. Saves are untouched (they name objects by id).
- **476. The canopy bake lays each tile over the one under it**, premultiplied "over" as the game draws: a transparent pixel of an upper level keeps the pixel under it, a half-transparent one blends. Today's bake overwrote each pixel, which only worked with one tile per cell.
- **477. The word is "level"**, in the content, the code and the composer; "layer" stays the map's (z), so the two never mix.
- **478. The composer's Level box offers the levels from 0 to one above the highest painted level** ("0 (base)", "1", "2"...), at most 7: there is always one empty level on top to paint on, and no Add level button. The chosen level is painted and erased by every stroke, the right button included. When an undo leaves the chosen level above the choices, the highest choice is chosen; a value outside the choices (a combo box's -1 while its list is rebuilt) is ignored. New object from a rectangle paints the base level.
- **479. The canvas draws every level, the levels above the chosen one faded** (40 % opacity), the chosen and lower ones whole: what is painted on stays plain, what will cover it stays visible.
- **480. Edit object reads the stack back:** each tile on its level, the Level box offering one empty level above the highest; Save writes only the painted cells, so an empty level is never saved.
- **481. The zoom is a cell's shown size**, in steps of 16, 32, 48, 64, 96, 128 and 192 screen pixels: whole factors (1x to 12x) for the 16 px sheets every Mana Seed sheet is (decision 444); 48, the map's cell, by default. A sheet's scale is the shown size over its tile width, which at 48 is today's `DisplayScaleOf`; an image not set up yet scales by the shown size over 48 (its own size at the default, as today).
- **482. One zoom for the Tiles panel, for the session**: it lives on `TileBrowserViewModel` and stays when another sheet is chosen. The composer's picker binds the same view model (decision 457), so it shares the zoom.
- **483. Ctrl + wheel over the sheet zooms toward the mouse**; the plain wheel keeps scrolling it. A bar over the sheet: −, the shown cell size ("48 px"; a click goes back to 48) and +, greyed at the bounds.
- **484. The composer's canvas has its own zoom**, same steps, bar and Ctrl + wheel, starting at 48 each time the composer opens; its pixels are drawn sharp (nearest neighbor), as the sheet's are.
- **485. The side panel stays as it is:** it already resizes with its splitter, and its minimum width (300) leaves room for the zoom bar.
- **486. The Animate panel's preview stays at the map's cell size (48 px)**: it shows the tile as the map draws it.
- **487. "Bigger and smaller" is the sheet's picture** (Q1): the developer's request means zooming the sheet in the Tiles panel, not resizing the panel, whose splitter stays as it is (decision 485).

## Open design questions (developer decides)
None open.

## ADRs
- **ADR 0040: object tile levels** (written before OL1 from this entry; decisions 472-476):
  - **Context:** an object is one tile per cell (decision 456; `ObjectCatalogValidator`: "an object has one tile per cell"), drawn in its list's order by the game (`MapView.DrawObject`) and the editor (`MapDrawing.DrawObjectAt`); its canopy is baked once by copying each tile's pixels (`ObjectSpriteBaker.CanopyPixels`), overwriting what was there. The Mana Seed half-timber sheet draws doors and windows as transparent overlays meant to sit on wall cells, so an object needs several tiles on one cell. Content files have no format version; saves name objects by id and never hold their tiles.
  - **Decision:** each object tile carries `level` (0 to 7, 0 the base, left out of the file while 0). An object has at most one tile per cell and level, and lists its tiles bottom level first; Data refuses anything else, naming the tile. Every drawer draws the list in order, so the stack is drawn bottom first with no sort; the canopy bake composes each tile over the pixels under it. The editor's composer paints one level at a time.
  - **Options:** the list's order alone as the draw order, several tiles allowed per cell (no new field, but a level painted with a hole reads back shifted down, and nothing tells a stacked cell from a duplicate entry); a separate `overlays` list on the object (two fixed levels, every drawer reading two lists); the tiles as a list of levels, each a list of tiles (rewrites every object in every file, the frozen test world's included); the chosen: a level on each tile, with the list kept in level order.
  - **Consequences:** old content is level 0 throughout and writes back unchanged; the test world needs no change; a hand-edited file listing a higher level first is refused with the tile named rather than drawn wrong; the composer's canvas holds cells by cell and level; a later wish (a level hidden in the editor, zones per level) adds to the composer, not the format. Saves are untouched.
