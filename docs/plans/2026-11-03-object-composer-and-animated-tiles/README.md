# Object composer and animated tiles
Status: draft

Drafted on 2026-11-03 from two requests the developer made while painting the first Mana Seed maps (the Mana Seed reset plan's C2):

> "I want to create object instead of the current way to be able to paint it in a small canvas also. These sheets have parts all over the place; to build a reusable object it will not be a single selection square, I'll need to paint it before."

> "Also there is animation sets I need to be able to use it and set it properly. I don't know how but I need an option in the editor to setup and place animated tiles."

Builds on the Mana Seed reset plan (`2026-11-03-mana-seed-reset`: decisions 442, 444, 449; 16 px sheets drawn 3x in the 48 px cell), ADR 0011 (content is the source of truth), ADR 0012 (editor architecture), ADR 0014 (editing shared catalogs) and ADR 0037 (the frozen test world). Numbers: none yet; decisions and the ADR are numbered at approval from `docs/plans/README.md` (next free: decision 455, ADR 0039).

## Goal
1. **An object is painted, not cut.** Objects > New object opens the object composer: a small canvas of cells (its size typed), the Tiles panel's sheet view beside it, and a click on the canvas stamps the chosen cell or rectangle there; parts come from anywhere on any set-up sheet. Save makes the object as one undo step, then the Shapes window opens. Objects > Edit object opens the same window on an existing object and keeps its id, outline, zones, placements and events (decision 449's promise, kept by a better tool).
2. **A tile can animate.** A tile may carry an animation: its frames (cells of its own sheet, in order, the first its own cell) and one time per frame. The game draws the frame the time calls for, every copy in step; an animated tile paints on maps and inside objects like any tile, so a waterfall or a torch built in the composer animates.
3. **The editor sets animations up and shows them.** Tiles > Animate tile opens a panel in the Tiles panel: click the frames on the sheet in order, type the time, watch the preview, Apply as one undo step. The editor's map view animates too.
4. **Core rules and saves:** Core gains one content record and its frame formula; no state, event kind or save format changes. Animation is presentation.

## Order, and why
1. **Phase 1: the object composer (CO1-CO6).** The developer is painting maps now and needs it first. Rules before the window (canvas, painting, the composer's state, the save), then the window replacing New object from selection, then Edit replacing Replace cells.
2. **Phase 2: animated tiles in content and the game (ADR, AN1, AN2).** The format lands with its validation and the game's drawing, so content the editor writes later is already what the game reads. AN2's hand check edits `content/tiles.json` locally and reverts it, since the editor can't set an animation up yet.
3. **Phase 3: animated tiles in the editor (AE1-AE4).** The panel's rules, its catalog step, the panel, then the map view animating. Last, since the game must draw what the editor sets up.

Phases 2 and 3 don't depend on phase 1, nor phase 1 on them: an animated tile works inside a composed object because the composer paints sheet cells, and a cell's tile is the animated one. If the developer wants animation sooner, phase 2 can run first.

## Guiding rules
- **Core never reads the clock** (CLAUDE.md, determinism): the game's animation time comes from the update's delta, the editor's from a view timer; Core only turns an elapsed time into a frame (`TileAnimation.FrameAt`), so both shells show the same frame for the same time.
- **Undo covers every edit**: a composed object is one catalog step; canvas strokes are undone inside the composer with its own Ctrl+Z; an animation applied or removed is one catalog step.
- **No lockout on missing art** (decision 442): the composer and the Animate panel draw a missing cell with `MissingArtMark`, and refuse new cells from a sheet whose image is missing, as Replace cells did.
- **Agents never open a Mana Seed image** (decision 438): hand checks are the developer's; tests use the test world's sheets and in-code catalogs.
- **Slice sizes:** Editor slices about 200 lines (tests count about as many lines as the code), others under 300; no changed file crosses 300 lines. `Shell/MainViewModel` (503) shrinks over phase 1 (CO4 +3, CO5 about −35, CO6 about −6) before AE3 adds 3. `Catalogs/CatalogDraft` (294) moves its removals into a partial file in AE2 before it grows. `Shell/MainWindow.xaml.cs` (569, a split in the backlog) takes at most two lines (AE3); new handlers go in `MainWindow.ObjectEditing.cs` (248) and a new `MainWindow.TileAnimation.cs`. `Tiles/TileBrowserViewModel` (330) changes one sentence only.

## Out of scope (follow-ups for `docs/plans/backlog.md` at approval)
- Several tiles on one object cell (question Q1): the object format stays one tile per cell.
- Per-frame times and per-cell start offsets (questions Q2, Q3): the format leaves room for both (design.md, "Extension points").
- Picking a free pixel rectangle from an image without a cut (question Q4): the composer's picker is the Tiles panel's view, so Set up and Cut work there as in the panel.
- Canopies that animate: the game bakes a canopy once, from each tile's first frame (planner's pick 9).
- An animated tile marked as such on the Tiles panel's sheet (a corner mark); the Animate panel shows a tile's animation when opened on it.
- The re-cut warning (`SheetUsage`) counting animation frames a re-cut would push outside the grid; the startup art check names them.
- Autotiles, lighting, characters (the reset plan's later plans).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the object composer
- [ ] **CO1. `ObjectCanvas`: an object's picture as sheet cells on a grid: stamp, erase, resize, read from an object, made into tiles** (Editor, about 190 lines) → [slices/CO1.md](slices/CO1.md)
- [ ] **CO2. `CanvasPainting`: strokes stamp the Tiles panel's chosen cells or erase, the size is typed, strokes undo inside the composer** (Editor, about 200 lines) → [slices/CO2.md](slices/CO2.md)
- [ ] **CO3. `ObjectComposerViewModel` and `ObjectNaming`: the composer's id, category, problem and the object it makes, new or edited** (Editor, about 190 lines) → [slices/CO3.md](slices/CO3.md)
- [ ] **CO4. `ObjectComposing`: New starts from the chosen rectangle or an empty canvas, Edit from the chosen object; Save makes the missing tiles and the object as one undo step** (Editor, about 200 lines) → [slices/CO4.md](slices/CO4.md)
- [ ] **CO5. The composer window; Objects > New object opens it and then the Shapes window; New object from selection leaves** (Editor, about 230 lines) → [slices/CO5.md](slices/CO5.md)
- [ ] **CO6. Objects > Edit object opens the composer on the chosen object, the Shapes window after a size change; Replace cells leaves** (Editor, about 130 lines) → [slices/CO6.md](slices/CO6.md)

### Phase 2: animated tiles in content and the game
- [ ] ADR (number at approval): animated tiles. Write it from the entry under "ADRs needed" below once Q2 and Q3 are answered (the entry holds the planner's picks; an answer that differs changes its Decision and AN1's record).
- [ ] **AN1. `TileAnimation` on `TileDefinition`: frames and frame time in `tiles.json`, validated, frames inside the image at startup, written back; `FrameAt` in Core** (Core + Data, about 230 lines) → [slices/AN1.md](slices/AN1.md)
- [ ] **AN2. The game draws the frame the time calls for: `TileAtlas` keeps each tile's frames, `MapView` advances its time with the update; canopies keep the first frame** (Game, about 170 lines) → [slices/AN2.md](slices/AN2.md)

### Phase 3: animated tiles in the editor
- [ ] **AE1. `TileAnimationViewModel`: the Animate panel's frames clicked in order on the tile's sheet, the typed time, problems and the preview's frame** (Editor, about 190 lines) → [slices/AE1.md](slices/AE1.md)
- [ ] **AE2. Apply and Remove animation as one undo step; `CatalogDraft` moves its removals into a partial file first** (Editor, about 160 lines with a 30-line move) → [slices/AE2.md](slices/AE2.md)
- [ ] **AE3. The Animate panel in the Tiles panel: frames numbered on the sheet, clicks add frames, a live preview; Tiles > Animate tile** (Editor, about 200 lines) → [slices/AE3.md](slices/AE3.md)
- [ ] **AE4. The editor's map view animates: one brush per animated tile, switched by the view timer** (Editor, about 180 lines) → [slices/AE4.md](slices/AE4.md)

## Decisions
None yet: the developer decides at approval. The developer said "your picks" is acceptable; the planner's picks below are proposed and numbered at approval, and the real forks are the open questions.

Planner's picks (proposed, numbered at approval; design.md, "Key decisions", gives the reasons):
1. **One composer for New and Edit**, a modal window; it replaces Objects > New object from selection (`NewObjectViewModel`, `NewObjectWindow`) and Objects > Replace cells of the chosen object (`ObjectReskinning`, `ObjectReskin`). Decision 449 still holds: Edit keeps the id, category, outline, zones, placements and events.
2. **One tile per canvas cell** (Q1's default): stamping over a painted cell replaces it; the object format is unchanged.
3. **The composer's picker is the Tiles panel's own sheet view** (`TileBrowserView` over the same `TileBrowserViewModel`): one choice of sheet and cells, shown in both places; parts come from any set-up sheet; Set up and Cut work inside it.
4. **A click stamps the chosen cell or rectangle with its top-left on the clicked canvas cell**, clipped to the canvas; dragging paints on; the right button, or the Erase toggle, erases.
5. **New starts from the Tiles panel's chosen rectangle** when there is one (sized to it, painted with it), else from an empty 3 x 3 canvas; the id and category are typed in the composer itself (no second dialog). Edit shows the id fixed and the category editable.
6. **Canvas size: 1 to 32 cells each way**, typed; a resize keeps cells at their place from the top-left and drops those outside.
7. **Strokes undo inside the composer** (its Ctrl+Z and Ctrl+Y, buttons too); Save is one catalog undo step; cells not tiles yet become walkable tiles first (decision 113), outside the history as today.
8. **The Shapes window opens after Save for a new object, and for an edited one whose size changed** (its outline and zones stretched with it).
9. **An animation belongs to a tile**: frames are cells of the tile's own sheet (so they leave with it in a removal or a purge), in order, the first its own cell; at least 2 frames; one time per frame for the whole animation, a whole number of milliseconds from 1; all copies in step.
10. **The game's animation time is the map view's**, advanced by each update's delta, restarting on entering a map and still while a screen is over the map (the scene stack updates only the top scene). The frame shown is Core's `TileAnimation.FrameAt(elapsed)`, shared with the editor.
11. **Canopies bake each tile's first frame**: an animated cell under an above-player zone shows still in the canopy part.
12. **The editor's map view always animates** (no toggle); the Objects panel preview, the placing ghost, the Shapes window and the composer's canvas show the first frame.
13. **Animate is a panel in the Tiles panel** (as the Cut panel), opened on the chosen cell of a set-up sheet with its image; Apply makes the cell a tile first when it isn't one.

## Open design questions (developer decides)
- **Q1. Stacking in the composer:** one tile per object cell (the plan), or several per cell (a door over a wall)? Several would change the object format (`ObjectTile` per cell becomes a list per cell, or layers), the game's and editor's drawing and the canopy bake: its own ADR and about two more slices.
- **Q2. Animation speed:** one time for every frame (the plan), or a time per frame (a flame that holds one frame longer)? Per frame would make `frames` entries carry an optional time; `FrameAt` would walk a cumulative table.
- **Q3. In step, or offset per cell:** every copy on the same frame (the plan), or each map cell starting at its own offset so a field of grass doesn't sway in unison? An offset would come from the cell's position (no randomness, no save), as an optional flag on the animation.
- **Q4. Picking from an image not set up:** only set-up sheets (the plan; Set up works from inside the composer's picker), or a free pixel rectangle cut on the fly from any image?

## ADRs needed
- **ADR (number at approval): animated tiles** (written before AN1 from this entry, adjusted to Q2 and Q3's answers):
  - **Context:** Mana Seed's Forest Autumn pack has animation sheets (water sparkles, a waterfall, tall grass) whose frames sit side by side; nothing but the player's walk animates today; Core never reads the clock; the game (`TileAtlas`, `MapView`) and the editor (`TileImages`, `MapDrawing`) draw tiles each their own way.
  - **Decision:** a tile in `tiles.json` may carry `animation`: `frameMilliseconds` (a whole number from 1, one time for every frame) and `frames` (at least 2 `{ "column", "row" }` cells of the tile's own sheet, in order, the first the tile's own cell); left out of the file when absent. `Core/Maps/TileAnimation.FrameAt(elapsed)` gives the frame: `(elapsed / frame time) mod frames`, so every copy is in step. The game's elapsed is the map view's, advanced by the update's delta and restarted on entering a map; the editor's comes from a view timer. Data validates the shape; the startup art check (`ArtReferenceCheck`) checks every frame's cell against the image, and an import refuses an image too small for them. Canopies bake the first frame. Saves are untouched.
  - **Options:** an `animations.json` naming tile ids per frame (a second file to keep in step, and frames that are tiles of their own); frames as tile ids (frames on other sheets, which a sheet's removal would orphan); animation on a map cell or layer (no animated tile inside objects); a time per frame (Q2); per-cell offsets (Q3); the chosen: an optional field on the tile.
  - **Consequences:** an animated tile paints and builds objects like any tile, in the game and the editor; frames leave with their sheet in a removal or a purge; a re-cut may push a frame outside the grid, which the editor draws as the missing mark and the startup check names; a time per frame or an offset would extend the record with optional fields, not replace it; the test world's frozen `tiles.json` needs no change (the field is optional).
- **Only if Q1 is answered "several per cell":** an ADR on the object tile format, before a re-planned CO1.
