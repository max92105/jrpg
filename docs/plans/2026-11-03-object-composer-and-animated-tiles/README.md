# Object composer and animated tiles
Status: approved

Drafted on 2026-11-03 from two requests the developer made while painting the first Mana Seed maps (the Mana Seed reset plan's C2):

> "I want to create object instead of the current way to be able to paint it in a small canvas also. These sheets have parts all over the place; to build a reusable object it will not be a single selection square, I'll need to paint it before."

> "Also there is animation sets I need to be able to use it and set it properly. I don't know how but I need an option in the editor to setup and place animated tiles."

Builds on the Mana Seed reset plan (`2026-11-03-mana-seed-reset`: decisions 442, 444, 449; 16 px sheets drawn 3x in the 48 px cell), ADR 0011 (content is the source of truth), ADR 0012 (editor architecture), ADR 0014 (editing shared catalogs) and ADR 0037 (the frozen test world).

- Numbers: decisions 455-471 and ADR 0039.
- Approved on 2026-11-03 with every recommendation taken (Q1-Q4 answered as the plan proposed) and the devil's advocate's findings applied: the animation format became a frame count running rightward from the tile's cell (what all 13 Mana Seed animations are), so explicit frames, their click routing and their marks left; an import of the pack's Tiled `.tsx` animations was added (AI1, AI2); the editor's map stays on the first frame (the animated map went to the backlog) while the previews and the Animate panel animate on the render tick; CO5 was split (CO4b) and its second sheet view lets go of the browser on close; CO6's hand check uses an object made in CO5's; `FrameAt` refuses a negative time.

## Goal
1. **An object is painted, not cut.** Objects > New object opens the object composer: a small canvas of cells (its size typed), the Tiles panel's sheet view beside it, and a click on the canvas stamps the chosen cell or rectangle there; parts come from anywhere on any set-up sheet. Save makes the object as one undo step, then the Shapes window opens. Objects > Edit object opens the same window on an existing object and keeps its id, outline, zones, placements and events (decision 449's promise, kept by a better tool).
2. **A tile can animate.** A tile may carry an animation: how many frames run rightward from its own cell on its sheet, and one time per frame. The game draws the frame the time calls for, every copy in step. An animated tile paints on any map layer like any tile, which is how the pack's animations are used: the sparkles and the waterfall are overlays on their own layers over the water and the cliff (decision 456), never parts of objects; an animated tile inside an object animates too, but the composer isn't built for it.
3. **The editor sets animations up.** Tiles > Import animations... reads a pack's Tiled `.tsx` file and gives every animated tile of the matching sheet its frame count and time; Tiles > Animate tile... opens a panel to set or change one tile by hand (frames to the right, time, a live preview). The Objects panel's preview and the composer's canvas animate; the editor's map shows each tile's first frame.
4. **Core rules and saves:** Core gains one content record and its frame formula; no state, event kind or save format changes. Animation is presentation.

## Order, and why
1. **Phase 1: the object composer (CO1-CO6).** The developer is painting maps now and needs it first. Rules before the window (canvas, painting, the composer's state, the save, the canvas's picture), then the window replacing New object from selection, then Edit replacing Replace cells.
2. **Phase 2: animated tiles in content and the game (ADR 0039, AN1, AN2).** The format lands with its validation and the game's drawing, so content the editor writes later is what the game reads. AN2's hand check edits `content/tiles.json` locally and reverts it, since the editor can't set an animation up yet.
3. **Phase 3: animated tiles in the editor (AI1, AI2, AE1, AE2, AE3).** The import first: every animation the pack has comes from its `.tsx` files, so one import sets them all up; it also brings the catalog step (`SetTileAnimations`) the panel reuses. Then the Animate panel for hand changes and sheets without a `.tsx`, then the animated previews.

Phases 2 and 3 don't depend on phase 1, nor phase 1 on them. If the developer wants animation sooner, phase 2 can run first.

## Guiding rules
- **Core never reads the clock** (CLAUDE.md, determinism): the game's animation time comes from the update's delta, the editor's from WPF's render tick; Core only turns an elapsed time into a frame (`TileAnimation.FrameAt`, `TileDefinition.ColumnShownAt`), so both shells show the same frame for the same time.
- **Undo covers every edit**: a composed object is one catalog step; canvas strokes are undone inside the composer with its own Ctrl+Z; an animation applied or removed, and a whole import, are one catalog step each.
- **No lockout on missing art** (decision 442): the composer and the previews draw a missing cell with `MissingArtMark`; the composer and the Animate panel refuse cells of a sheet whose image is missing, as Replace cells did.
- **Agents never open a Mana Seed image** (decision 438): hand checks are the developer's; tests use the test world's sheets, in-code catalogs and `.tsx` text written by the test. A pack's `.tsx` is XML, not an image: reading its structure for the import is allowed, but the importer is written from Tiled's documented format, and the numbers above are the developer's.
- **Slice sizes:** Editor slices about 200 lines (tests count about as many lines as the code), others under 300; no changed file crosses 300 lines. `Shell/MainViewModel` (503) shrinks over phase 1 (CO4a +3, CO5 about −35, CO6 about −6) before AI2, AE1 and AE2 add about 8. `Catalogs/CatalogDraft` (294) moves its removals into a partial file in AI2 before it grows. `Shell/MainWindow.xaml.cs` (569, a split in the backlog) takes at most two lines (AE2); new handlers go in `MainWindow.ObjectEditing.cs` (248) and a new `MainWindow.TileAnimation.cs`. `Tiles/TileBrowserViewModel` (330) changes one sentence only.

## Out of scope (follow-ups, in `docs/plans/backlog.md`, "From the object composer and animated tiles plan")
- Several tiles on one object cell (decision 456): a later plan if the half-timber house's doors or windows turn out to be transparent overlays on wall cells.
- The editor's map animating (the former AE4): one shared brush per animated tile switched by the render tick; until then the map shows each tile's first frame (decision 466).
- An explicit frame list (frames anywhere on the sheet, a time per frame) and per-cell start offsets: the ADR's options; the format leaves room for them as optional fields.
- The tall grass effect sheet: a cutting effect with 32 px frames, not a tile animation (decision 471).
- Picking a free pixel rectangle from an image without a cut (decision 457): the composer's picker is the Tiles panel's view, so Set up and Cut work there as in the panel.
- Canopies that animate: the game bakes a canopy once, from each tile's first frame (decision 465).
- An animated tile marked as such on the Tiles panel's sheet; the Animate panel shows a tile's animation when opened on it.
- The re-cut warning (`SheetUsage`) counting animation frames a re-cut would push outside the grid; the startup art check names them.
- Autotiles, lighting, characters (the reset plan's later plans).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the object composer
- [x] **CO1. `ObjectCanvas`: an object's picture as sheet cells on a grid: stamp, erase, resize, read from an object, made into tiles** (Editor, about 190 lines) → [slices/CO1.md](slices/CO1.md)
- [x] **CO2. `CanvasPainting`: strokes stamp the Tiles panel's chosen cells or erase, the size is typed, strokes undo inside the composer** (Editor, about 200 lines) → [slices/CO2.md](slices/CO2.md)
- [x] **CO3. `ObjectComposerViewModel` and `ObjectNaming`: the composer's id, category, problem and the object it makes, new or edited** (Editor, about 190 lines) → [slices/CO3.md](slices/CO3.md)
- [ ] **CO4a. `ObjectComposing`: New starts from the chosen rectangle or an empty canvas, Edit from the chosen object; Save makes the missing tiles and the object as one undo step** (Editor, about 200 lines) → [slices/CO4a.md](slices/CO4a.md)
- [ ] **CO4b. The canvas's picture: `ObjectCanvasDrawing` and `TileImages.CellImageOf`** (Editor, about 110 lines) → [slices/CO4b.md](slices/CO4b.md)
- [ ] **CO5. The composer window; Objects > New object opens it and then the Shapes window; New object from selection leaves** (Editor, about 200 lines) → [slices/CO5.md](slices/CO5.md)
- [ ] **CO6. Objects > Edit object opens the composer on the chosen object, the Shapes window after a size change; Replace cells leaves** (Editor, about 130 lines) → [slices/CO6.md](slices/CO6.md)

### Phase 2: animated tiles in content and the game
- [ ] ADR 0039: animated tiles. Write it from the entry under "ADRs" below (decisions 463-471).
- [ ] **AN1. `TileAnimation` on `TileDefinition`: frame count and frame time in `tiles.json`, validated, the last frame inside the image at startup, written back; `FrameAt` and `ColumnShownAt` in Core** (Core + Data, about 190 lines) → [slices/AN1.md](slices/AN1.md)
- [ ] **AN2. The game draws the frame the time calls for: `TileAtlas` keeps each tile's frames, `MapView` advances its time with the update; canopies keep the first frame** (Game, about 170 lines) → [slices/AN2.md](slices/AN2.md)

### Phase 3: animated tiles in the editor
- [ ] **AI1. `TiledTilesetReader`: a Tiled `.tsx` file's image, cut and animations, each turned into a frame count and time or refused naming the tile** (Data, about 200 lines) → [slices/AI1.md](slices/AI1.md)
- [ ] **AI2. Tiles > Import animations...: the `.tsx`'s sheet found by its image's file name, every animated tile set as one undo step; `CatalogDraft` moves its removals into a partial file first** (Editor, about 205 lines with a 30-line move) → [slices/AI2.md](slices/AI2.md)
- [ ] **AE1. `TileAnimationViewModel`: the Animate panel's frames to the right and time for the chosen cell, its problems and the preview's cell** (Editor, about 180 lines) → [slices/AE1.md](slices/AE1.md)
- [ ] **AE2. The Animate panel in the Tiles panel: Apply and Remove animation as one undo step, a live preview on the render tick (`TileAnimationTime`); Tiles > Animate tile** (Editor, about 205 lines) → [slices/AE2.md](slices/AE2.md)
- [ ] **AE3. The Objects panel's preview and the composer's canvas animate** (Editor, about 130 lines) → [slices/AE3.md](slices/AE3.md)

## Decisions
- **455. One composer for New and Edit**, a modal window; it replaces Objects > New object from selection (`NewObjectViewModel`, `NewObjectWindow`) and Objects > Replace cells of the chosen object (`ObjectReskinning`, `ObjectReskin`). Decision 449 still holds: Edit keeps the id, category, outline, zones, placements and events. Replace cells' one-click path (a rectangle chosen, one menu entry, picture and size replaced) becomes Edit, a size typed, the rectangle stamped, Save: a re-skin happens once per object after an art change, and the composer does it with parts from anywhere.
- **456. One tile per object cell** (Q1): stamping over a painted cell replaces it; the object format is unchanged. The pack's own animations (the sparkles over water, the waterfall over the cliff) are overlays painted on their own map layers, never inside objects, so the composer doesn't carry them. If the half-timber house's doors or windows turn out to be transparent overlays on wall cells, stacking becomes its own later plan (backlog).
- **457. The composer's picker is the Tiles panel's own sheet view** (`TileBrowserView` over the same `TileBrowserViewModel`), and only set-up sheets give cells (Q4); Set up and Cut work inside it as in the panel. Parts come from any set-up sheet. The composer's view lets go of the shared view model when the window closes.
- **458. A click stamps the chosen cell or rectangle with its top-left on the clicked canvas cell**, clipped to the canvas; dragging paints on; the right button, or the Erase toggle, erases.
- **459. New starts from the Tiles panel's chosen rectangle** when there is one (sized to it, painted with it), else from an empty 3 x 3 canvas; the id and category are typed in the composer itself (no second dialog). Edit shows the id fixed and the category editable.
- **460. Canvas size: 1 to 32 cells each way**, typed; a resize keeps cells at their place from the top-left and drops those outside.
- **461. Strokes undo inside the composer** (its Ctrl+Z and Ctrl+Y, buttons too); Save is one catalog undo step; cells not tiles yet become walkable tiles first (decision 113), outside the history as today.
- **462. The Shapes window opens after Save for a new object, and for an edited one whose size changed** (its outline and zones stretched with it).
- **463. An animation belongs to a tile: `animation: { frameCount, frameMilliseconds }`** (ADR 0039). The frames run rightward along the row from the tile's own cell on its sheet (what all 13 Mana Seed animations are), at least 2, so they leave with their sheet in a removal or a purge; one time for every frame (Q2), a whole number of milliseconds from 1; every copy in step (Q3). An explicit frame list is a later option.
- **464. The game's animation time is the map view's**, advanced by each update's delta, restarting on entering a map and still while a screen is over the map (the scene stack updates only the top scene). The frame shown is Core's `TileAnimation.FrameAt(elapsed)` (a negative time is refused), shared with the editor.
- **465. Canopies bake each tile's first frame**: an animated cell under an above-player zone shows still in the canopy part.
- **466. The editor's map shows each tile's first frame**; the Objects panel's preview, the composer's canvas and the Animate panel's preview animate. Animating the map is a backlog line.
- **467. Animate is a panel in the Tiles panel** (as the Cut panel), opened on the chosen cell of a set-up sheet with its image: a "frames to the right" box, the time, a live preview, Apply, Remove animation; Apply makes the cell a tile first when it isn't one.
- **468. The time field is `frameMilliseconds`**, not `frameTimeMs` (no abbreviations), as `StillStep.Seconds` names its unit.
- **469. Tiles > Import animations...** reads a Tiled `.tsx` file chosen in a dialog: its sheet is the set-up sheet whose image has the `.tsx`'s image file name, its cut must match the sheet's, and every `<tile>` with an `<animation>` becomes that cell's frame count and time, as one undo step (cells not tiles yet made tiles first). A `.tsx` with an animation whose frames don't run rightward from the tile's own cell on its row, or whose frames differ in duration, is refused whole, naming the tile.
- **470. The editor's animated pictures follow WPF's render tick** (`CompositionTarget.Rendering`, about 16 ms): the editor's animation time moves on by the rendering time, and a picture is redrawn only when `FrameAt` gives a new frame for one of its tiles.
- **471. The tall grass effect sheet is out of scope**: a cutting effect drawn with 32 px frames, not a tile animation.

## Open design questions (developer decides)
None open.

## ADRs
- **ADR 0039: animated tiles** (written before AN1 from this entry; decisions 463-471):
  - **Context:** the Mana Seed Forest Autumn pack has 13 animations, all Tiled `.tsx` animations whose frames run rightward from the animated tile's cell with one duration each (the developer's count: waterfall B rows 0-3, 8 frames at 70 ms; rows 4-9, 4 frames at 140 ms; the sparkles, 3 tiles of 4 frames at 600 ms); they are overlays painted on their own map layers over water and cliffs. The tall grass effect sheet is a cutting effect with 32 px frames, not a tile animation, and stays out. Nothing but the player's walk animates today; Core never reads the clock; the game (`TileAtlas`, `MapView`) and the editor (`TileImages`, `MapDrawing`) draw tiles each their own way.
  - **Decision:** a tile in `tiles.json` may carry `animation`: `frameCount` (at least 2 frames, the tile's own cell then the cells to its right on the same row) and `frameMilliseconds` (a whole number from 1, one time for every frame); left out of the file when absent. `Core/Maps/TileAnimation.FrameAt(elapsed)` gives the frame, `(elapsed / frame time) mod frameCount`, refusing a negative time; `TileDefinition.ColumnShownAt(elapsed)` is the column drawn; every copy is in step. The game's elapsed is the map view's, advanced by the update's delta and restarted on entering a map; the editor's moves on with WPF's render tick, for its previews and the Animate panel (its map shows the first frame). Data validates the shape; the startup art check (`ArtReferenceCheck`) checks the last frame's cell against the image, and an import refuses an image too small for it. Canopies bake the first frame. The editor imports a Tiled `.tsx` file's animations when they have this shape and refuses the file otherwise. Saves are untouched.
  - **Options:** an explicit `frames` list of cells (any cell of the sheet; kept as a later option, an optional field beside `frameCount`, if a sheet needs it); a time per frame (Tiled allows it; none of the pack's animations uses it); per-cell start offsets so a field doesn't move in unison (a fixed offset from the cell's position, no randomness); an `animations.json` naming tile ids per frame (a second file to keep in step, frames that are tiles of their own); frames on other sheets (a sheet's removal would orphan them); animation on a map cell or layer (no animated tile inside an object); the chosen: a frame count and a time on the tile.
  - **Consequences:** an animated tile paints like any tile, in the game and the editor; frames leave with their sheet in a removal or a purge; a re-cut may push the last frame outside the grid, which the editor draws as the missing mark and the startup check names; an explicit list, per-frame times or offsets would extend the record with optional fields, not replace it; a `.tsx` of another shape is refused rather than half-imported; the test world's frozen `tiles.json` needs no change (the field is optional).
