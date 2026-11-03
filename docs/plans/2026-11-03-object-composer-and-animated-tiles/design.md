# Object composer and animated tiles: design

## Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| New canvas size | `Editor/Objects/Composing/ObjectComposing.NewCanvasCells` | 3 x 3 | The empty canvas New object starts from when no rectangle is chosen in the Tiles panel. Placeholder. |
| Largest canvas | `Editor/Objects/Composing/ObjectCanvas.MaxCells` | 32 cells each way | The typed size's upper bound (an 80 x 112 Mana Seed tree is 5 x 7 cells). Technical limit, not design. |
| Canvas cell | `Editor/Objects/Composing/ObjectCanvasDrawing` | 48 px (`MapDrawing.CellPixels`) | The canvas shows cells at the map's size: 16 px art at 3x (decision 444). |
| Default frame time | `Editor/Tiles/TileAnimationViewModel.DefaultFrameMilliseconds` | 150 ms | The time the Animate panel offers for a tile with no animation yet. Placeholder. |
| Editor animation tick | `Editor/Shell/MainWindow.TileAnimation.cs` | 50 ms | How often the editor's view timer moves its time on and switches frames (map brushes and the panel's preview). View only. |
| Smallest frame time | `Data/Maps/TileCatalogValidator` | 1 ms | Below it the validator refuses; no upper bound. |
| Fewest frames | `Core/Maps/TileAnimation.MinFrames` | 2 | An animation of one frame is a still tile. |

## Design overview

### What exists
- **Objects are made from one rectangle.** Shift + drag in the Tiles panel sets `TileBrowserViewModel.SelectedCells` (a `SheetCellRectangle`); Objects > New object from selection (`MainWindow.ObjectEditing.OnNewObjectClicked`) opens `NewObjectWindow` over `MainViewModel.StartNewObject()` (`NewObjectViewModel`: id and category with `IdProblem` through `ObjectIds.ProblemOf`, `CategoryProblem`), and `MainViewModel.CreateObject` makes the cells tiles (`CatalogDraft.TileIdsOf(SheetCellRectangle)`, `TileAt ?? AddTile`, outside the history, decision 113), builds the object (`ObjectFromCells.Build`) and adds it as one step (`AddAndChoose`: `CatalogDraft.SetObject`, chosen in the Objects panel, Objects mode); the shell then opens the Shapes window (decision 124). `ObjectReskinning` (Objects > Replace cells, decision 449) gives the chosen object the marked cells through `ObjectReskin.Reskin` (`original with { Width, Height, Tiles }`), refusing an NPC and a sheet whose image is missing.
- **The object format** (`Core/Maps/ObjectDefinition`, `ObjectTile(X, Y, TileId)`): one tile per cell at most (`ObjectCatalogValidator`: "an object has one tile per cell"), cells may be empty, at least one tile unless it is an NPC; outline and zones in 0..1 of the object's size.
- **The Tiles panel's sheet view** `Tiles/TileBrowserView` (view-only code-behind: a click calls `SelectCellAt`, a drag `SelectCellsBetween`, or `WalkabilityMarking.FlipCellAt` while marking) over `TileBrowserViewModel` (330 lines: sheet choices, the Cut panel, `CellAt`, `SelectedCell`, `SelectedCells`, `MarkedCells`, `ChoiceText`). Its `Images` (`TileImages`) and `Marking` are set by the main window.
- **Editor drawing**: `Tiles/TileImages.TileImageOf(tileId)` crops and caches a tile by (sheet record, column, row) and returns `MissingArtMark.Picture` for a missing image or a cell past it; `Maps/MapDrawing.Draw` builds one frozen `DrawingImage` of the whole map each time the map or catalog changes (`MainWindow.xaml.cs` line 183), canopies being the object's tiles again under a clip; `DrawObject` makes the previews.
- **Game drawing**: `Rendering/TileAtlas` works out each tile's `TileImage(SheetFile, Source)` once; `Exploration/MapView` (built per map in `ExplorationScene.EnterCurrentMap`) draws layers and objects through it; `Rendering/CanopyTextures` bakes every canopy once through `ObjectSpriteBaker.CanopyPixels`, which reads `TileAtlas.ImageOf`. `ExplorationScene.Update(delta, input)` is the scene's time; `SceneStack` updates only the top scene. `WalkAnimation` is the one animation, driven by the delta.
- **Content**: `tiles.json` loads straight into `TileDefinition` (ADR 0002) through `ContentCatalogLoader`, checked by `TileCatalogValidator`; `ContentWriter.WriteMapCatalogs` writes it with `ContentFileWriter`, leaving out the optional fields listed in `AbsentWhenNullFilter`; `ArtReferenceCheck.FindProblems` checks each tile's cell against its image at game startup, and `FirstTileOutside` lets `ImportTilesheetImage` refuse an image too small for the tiles cut from it. The repository's `content/tiles.json` is empty since the reset (decision 453); the test world's frozen `tiles.json` holds the old tiles.
- **Catalog edits**: `Catalogs/CatalogDraft` (294 lines) turns each change into an undo step (`Do(CatalogEdit)`); `SetObject` skips a record equal by content (`IsSameObject`).

### Core changes
- `Core/Maps/TileAnimation` (sealed record): a tile's animation as content: the time each frame shows and the frames in order. `FrameMilliseconds` (int), `Frames` (`IReadOnlyList<AnimationFrame>`), `MinFrames` (2), `FrameDuration` (computed, `TimeSpan`), `FrameAt(TimeSpan elapsed)` (the index shown after that time: `(elapsed.Ticks / FrameDuration.Ticks) % Frames.Count`, in `long`, so years of play stay in range).
- `Core/Maps/AnimationFrame` (readonly record struct `(int Column, int Row)`): one frame's cell on the tile's sheet.
- `Core/Maps/TileDefinition.Animation` (`TileAnimation?`, init, null by default): no constructor churn.

Why Core: the game and the editor both turn a time into a frame and must agree; the editor never references the Game. `FrameAt` is a formula, not state: Core still never reads a clock.

### Structure and folder dependencies

New files and types:

| Where | Type | Responsibility |
|---|---|---|
| `Core/Maps` | `TileAnimation` | A tile's frames and the time each shows; the frame shown after a time. |
| `Core/Maps` | `AnimationFrame` | One frame's cell on the tile's sheet. |
| `Editor/Objects/Composing` | `ObjectCanvas` | An object's picture being composed: its size and the sheet cell on each of its cells. Immutable. |
| `Editor/Objects/Composing` | `CanvasPainting` | The composer's canvas as the developer paints it: strokes from the Tiles panel's chosen cells or erasing, the typed size, the strokes' own undo. |
| `Editor/Objects/Composing` | `ObjectComposerViewModel` | The composer window's state: the painting, the naming of a new object or the edited original, what stops Save, and the object it makes. |
| `Editor/Objects` | `ObjectNaming` | The id and category typed for a new object, with their problems (`NewObjectViewModel` without its cells). |
| `Editor/Objects/Composing` | `ObjectComposing` | Objects > New object and Edit object: starts the composer on the chosen cells or object, and applies it as one undo step. |
| `Editor/Objects/Composing` | `ObjectCanvasDrawing` | The canvas's picture: each painted cell's image at the map's cell size, with the grid. |
| `Editor/Objects/Composing` | `ObjectComposerWindow` | The window: the picker, the boxes and the canvas, mouse strokes to `CanvasPainting`. |
| `Editor/Tiles` | `TileAnimationViewModel` | The Animate panel: the chosen tile's frames clicked in order, the typed time, the preview's frame, applied or removed as one undo step. |
| `Editor/Tiles` | `AnimationFrameMarks` | The open panel's frames numbered over the sheet. |
| `Editor/Tiles` | `AnimatedTileBrushes` | One brush per animated tile, switched to the frame a time calls for, which the map drawing paints those tiles with. |
| `Editor/Catalogs` | `CatalogDraft.Removals.cs` | Partial file: `RemoveTilesheet`, `RemoveObject` moved out unchanged (AE2). |
| `Editor/Shell` | `MainWindow.TileAnimation.cs` | Partial file: the view timer moving the editor's animation time on (AE3, AE4). |

Changed: `Data/Maps/TileCatalogValidator` (the animation's rules), `Data/Art/ArtReferenceCheck` (frames inside the image, `FirstTileOutside` counting frames), `Data/Content/AbsentWhenNullFilter` (`animation` left out while null), `Game/Rendering/TileAtlas` (frames per tile, `ImageAt`), `Game/Exploration/MapView` (its animation time, `Advance`), `Game/Exploration/ExplorationScene` (advances the view), `Editor/Catalogs/CatalogDraft` (`TileIdsOf` over any cells; `SetTileAnimation`), `Editor/Tiles/TileImages` (`CellImageOf`), `Editor/Tiles/TileBrowserView` (routes clicks to the open Animate panel; a third image layer for the frame marks), `Editor/Maps/MapDrawing` (animated tiles painted with their brush; frozen only without one), `Editor/Shell/MainViewModel` (holds `Composing` and `Animating`; loses `StartNewObject`, `CreateObject`, `CanStartNewObject`, `Reskinning`), `Editor/Shell/MainWindow.ObjectEditing.cs` (New and Edit open the composer), `Editor/Shell/MainWindow.xaml` (menus, tool bar, Tiles tab), `Editor/Shell/ControlsHelpWindow` (the composer's and the panel's gestures).

Removed: `Objects/NewObjectViewModel`, `Objects/NewObjectWindow` (CO5), `Objects/ObjectFromCells`, `Objects/ObjectReskin`, `Objects/ObjectReskinning` (CO6), with their tests (their cases move to the composer's tests, named in each slice).

Folder edges: Editor `Objects/Composing -> Objects` (`ObjectNaming`, `ObjectIds`), `Objects/Composing -> Catalogs`, `Objects/Composing -> Tiles` (`TileBrowserViewModel`, `TileImages`, `TileBrowserView`), `Objects/Composing -> Maps` (`MapDrawing.CellPixels`, an existing edge kind: `Tiles -> Maps` exists through `TilesheetMarks`), `Shell -> Objects/Composing`. `Objects` never references `Objects/Composing` (no cycle). `Tiles -> Core.Maps` (`TileAnimation`) exists. Game `Exploration -> Rendering` exists; Data `Art -> Core.Maps` exists. No new project reference.

### Key decisions
1. **The composer paints sheet cells, not tiles** (picks 2, 3, 7). A canvas cell holds a `SheetCell`; Save turns each into a tile through `CatalogDraft.TileIdsOf` (`TileAt ?? AddTile`), so a cell not yet a tile is made one, and an animated tile's first cell paints that animated tile. Editing reads each `ObjectTile` back to its tile's cell; when two tiles share a cell the lowest id comes back (`TileAt`'s rule), which only matters for hand-made content.
2. **One window for New and Edit, its rules headless** (pick 1). `CanvasPainting` owns the strokes, `ObjectComposerViewModel` the naming and the result, `ObjectComposing` the start and the apply; the window only turns mouse positions into canvas cells. `ObjectComposerViewModel.ToObject(tileIdOf)` builds a new object (`ObjectNaming`'s id and trimmed category, no outline, no zones) or `Original with { Width, Height, Tiles, Category }`, so Edit keeps the id, outline and zones (stretched in 0..1 with a new size) and every placement and event, as decision 449 does today. `ObjectComposing.TryApply` follows the dialog pattern (`Start...` returns the view model, `TryApply` refuses on a problem).
3. **The picker is the Tiles panel's view model** (pick 3). The composer shows a second `TileBrowserView` bound to `MainViewModel.Painting.Browser`; choosing a sheet or cells in the composer chooses them in the panel too. No second view model to build or unsubscribe, and Set up and Cut work there. The window doesn't set the view's `Marking`, so a click always chooses.
4. **Strokes undo inside the composer** (pick 7). `ObjectCanvas` is immutable, so `CanvasPainting` keeps a stack of canvases: a stroke (press to release) or a resize is one step. The main editor's history sees only Save.
5. **The animation is the tile's, frames on its own sheet** (pick 9; the ADR). A cell's tile is what a brush and the composer paint, so an animated tile is painted and composed with no other change. Frames on the same sheet leave with it in `RemoveTilesheet` and `SheetPurge` and need no reference check of their own; the first frame being the tile's own cell keeps the sheet view, the previews and the canopy right without reading the animation.
6. **One formula, two clocks** (pick 10). `TileAnimation.FrameAt(elapsed)` in Core. The game: `MapView` holds its time and `Advance(delta)`; `ExplorationScene.Update` calls it first, whatever runs (an event, an encounter's pause); a new map's view starts at zero; screens over the map stop the scene's updates, so the water stands still under the menu, the Forge and a shop. The editor: one `DispatcherTimer` in the main window (view only) adds its interval to a `TimeSpan` and asks `AnimatedTileBrushes.ShowFrameAt` and the Animate panel's preview.
7. **No allocation per frame in the game.** `TileAtlas` keeps per tile an array of source rectangles (one for a still tile) and its animation; `ImageAt(tileId, elapsed)` returns a `TileImage` struct; `ImageOf(tileId)` stays, the first frame, for the canopy bake.
8. **The editor animates by brushes, not redraws** (pick 12). Redrawing the whole map image on every tick would rebuild thousands of draw calls 20 times a second. Instead `MapDrawing` paints an animated tile's cells with one shared, unfrozen `ImageBrush` per tile from `AnimatedTileBrushes`, and the timer swaps each brush's `ImageSource`: WPF re-renders without a new drawing. A map drawing holding such a brush is left unfrozen (a frozen drawing would freeze the brush). The previews (`DrawObject`, the ghost, the Shapes window, the composer) keep `TileImageOf`, the first frame.
9. **The Animate panel sits in the Tiles panel** (pick 13), as the Cut panel does, so the frames are clicked on the sheet the developer already sees; while it is open, a click on the sheet adds a frame instead of choosing (as Mark blocked flips). Its view model lives beside the browser (`MainViewModel.Animating`), not in it: `TileBrowserViewModel` is at 330 lines.

### Extension points
- **Several tiles per object cell** (Q1): `ObjectCanvas` would hold a list per cell and `ObjectTile` gain a level; the format change needs an ADR.
- **A time per frame** (Q2): `AnimationFrame` gains an optional `milliseconds`; `FrameAt` walks the cumulative times; the record and its tests change, the shells don't.
- **Offsets per cell** (Q3): an optional `isOffsetByCell` on `TileAnimation` and a `FrameAt(elapsed, GridCell)` that adds a fixed offset from the cell's position (a hash, no `IRandom`, nothing saved); the game's `MapView` and the editor's brushes would then need one brush or source per offset class.
- **Animated canopies** (pick 11): bake one canopy per frame for an object holding an animated cell under a zone.
- **Animation as a map-wide time** (a menu that keeps the water moving): move the time from `MapView` to a `TileClock` on `MapArt`, advanced by the flow.

### Replacement mapping

| Today | After this plan |
|---|---|
| Objects > New object from selection, `NewObjectViewModel`, `NewObjectWindow`, `MainViewModel.StartNewObject`, `CreateObject`, `CanStartNewObject`, the Tiles tab's "New object from selection..." button | Objects > New object..., `ObjectComposing.StartNew`, `ObjectComposerWindow`, `ObjectComposing.TryApply` (CO4, CO5) |
| `ObjectFromCells.Build`, `TilesOf` | `ObjectCanvas.Of(SheetCellRectangle)`, `ObjectCanvas.TilesOf` (CO1) |
| Objects > Replace cells of the chosen object, `ObjectReskinning`, `ObjectReskin` | Objects > Edit object..., `ObjectComposing.StartEdit`, `ObjectComposerViewModel.ToObject` on the original (CO6) |
| `NewObjectViewModel`'s id and category | `ObjectNaming` (CO3) |
| `CatalogDraft.TileIdsOf(SheetCellRectangle)` | `TileIdsOf(IReadOnlyCollection<SheetCell>)` (CO1) |
| `TileAtlas.ImageOf` in `MapView` | `TileAtlas.ImageAt(tileId, elapsed)`; `ImageOf` stays for the canopy bake (AN2) |

## Risks
- **Size of the window slice.** CO5 holds the window's XAML and mouse code, the canvas drawing and the switch from the old dialog (six existing tests move to the composer). If it passes about 250 lines at the slice step, `ObjectCanvasDrawing` and `TileImages.CellImageOf` move into their own slice before it.
- **Two views over one browser.** The composer's picker and the Tiles panel share `TileBrowserViewModel`: an open Cut panel shows in both, and a choice made in the composer stays in the panel after it closes (expected, and the hand check says so). The composer is modal, so the main window's view only redraws.
- **Unfrozen map drawings.** A map with animated tiles is drawn into an unfrozen `DrawingImage`; WPF renders it on the UI thread like any other, but a big map redrawn on each catalog change while brushes switch could stutter. The hand check paints a 60 x 40 map with animated water; if it stutters, the backlog gets a "draw animated cells as an overlay" line. `RenderOptions.BitmapScalingMode` must reach the brushes (set on the brush itself), or the 3x cells blur: AE4's hand check looks for it.
- **Canopies don't animate** (pick 11). A waterfall under a roof zone shows its first frame where the canopy covers it; the editor (which clips the tiles themselves) shows it moving, so the editor and the game differ there.
- **Re-cutting a sheet with animated tiles** can push a frame outside the grid: the editor draws the mark, the startup check names the tile and frame; the re-cut's warning doesn't count frames (out of scope).
- **The frozen test world** needs no change: `animation` is optional and left out while null. Tests with an animation build their catalogs in code (ADR 0037's rule on format changes concerns required fields).
- **Determinism:** no randomness; the game's time is the delta's sum; nothing saved. **Save format:** untouched. **Performance:** the game does one dictionary lookup and one division per drawn tile, no allocation; the editor switches one brush per animated tile per tick.
- **File sizes:** `MainViewModel` (503) shrinks over phase 1 before AE3 adds `Animating`; `CatalogDraft` (294) moves its removals out in AE2 before `SetTileAnimation`; `MainWindow.xaml.cs` (569) takes at most two lines (AE3 starts the timer); `TileBrowserViewModel` (330) changes one sentence of `ChoiceText` (CO5); `MapDrawing` (211) takes about 25 lines (AE4); `TileBrowserView.xaml.cs` (131) about 25 (AE3); `MainWindow.ObjectEditing.cs` (248) about 15 (CO5, CO6).
