# Object levels and sheet zoom: design

## Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| Highest level | `Core/Maps/ObjectTile.MaxLevel` | 7 (eight levels) | The content check's upper bound and the Level box's last choice (decisions 473, 478). Technical limit, not design. |
| Faded level | `Editor/Objects/Composing/ObjectCanvasDrawing.UpperLevelOpacity` | 0.4 | How the canvas draws the levels above the chosen one (decision 479). Placeholder look. |
| Zoom steps | `Editor/Viewport/CellZoom.Steps` | 16, 32, 48, 64, 96, 128, 192 px | A cell's shown size; whole factors 1x to 12x for 16 px sheets (decision 481). |
| Starting size | `MapDrawing.CellPixels`, passed to `CellZoom`'s constructor | 48 px | The map's cell: the sheet and the canvas open at it, as today. |

## Design overview

### What exists
- **The object format** (`Core/Maps/ObjectDefinition`, `ObjectTile(X, Y, TileId)`, a positional `readonly record struct`): one tile per cell (`Data/Maps/ObjectCatalogValidator.FindTileProblem`: "is listed more than once; an object has one tile per cell"), cells may be empty, at least one tile unless it is an NPC. `ContentWriter.WriteMapCatalogs` validates and writes `objects.json` through `ContentJsonOptions.Strict` (unmapped members refused, required constructor parameters respected; `AbsentWhenNullFilter` leaves later optional fields out while null). Content files have no format version; only saves do (`Data/Saving/SaveFile.CurrentFormatVersion`, ADR 0022). The repository's `content/objects.json` holds only the villager NPC today; the test world's frozen `objects.json` holds the old objects, all one tile per cell.
- **The game draws an object's tiles in list order** (`Game/Exploration/MapView.DrawObject`, an indexed loop, no allocation) and bakes canopies once (`Game/Rendering/ObjectSpriteBaker.CanopyPixels` → `CopyTile`, which writes every masked pixel, transparent ones included). Textures are premultiplied (`TextureLibrary`, `DefaultColorProcessors.PremultiplyAlpha`).
- **The editor draws an object's tiles in list order too** (`Maps/MapDrawing.DrawObjectAt`), for the map, its canopies (a clip), the Objects panel's preview, the placing ghost and the Shapes window (all through `DrawObject`). `PlacedObjectHits.CoversWithATile`, `CatalogUsage.ObjectIdsUsing`, `SheetPurge.ObjectsUsing` and `MapDrawing.IsAnimated` ask "any tile", so a stack changes none of them. `CatalogDraft.IsSameObject` compares tile lists by content (`SequenceEqual`), so a `Level` on the record struct takes part.
- **The composer** (`Objects/Composing/`): `ObjectCanvas` (immutable: size and one `SheetCell` per `GridCell` in `Cells`; `Stamp`, `Erase`, `Resize`, `Of(SheetCellRectangle)`, `Of(ObjectDefinition, tiles)` built with `ToDictionary` keyed by cell, which throws on two tiles on one cell, `TilesOf` row by row); `CanvasPainting` (strokes through `PaintAt`/`EraseAt`, a stack of canvases for its undo, `IsSamePicture` comparing the size and `Cells`); `ObjectComposerViewModel` (`Problem`, `ToObject`); `ObjectComposing` (`StartNew`, `StartEdit`, `TryApply`: `CatalogDraft.TileIdsOf([.. Canvas.Cells.Values])`); `ObjectCanvasDrawing.Draw(canvas, imageOf)` at `MapDrawing.CellPixels` with one-pixel grid lines; `ObjectComposerWindow` (the picker is a `TileBrowserView` over the panel's `TileBrowserViewModel`; `StrokeAt` turns `e.GetPosition(CanvasImage)` into a canvas cell; `CanvasImage` sets no bitmap scaling mode). `ObjectReskin`, `ObjectReskinning` and `ObjectFromCells` left in CO6 and CO1: nothing of them remains.
- **The sheet view** (`Tiles/TileBrowserView`): a `Grid` (`SheetView`) holding the sheet image and its marks, sized in the image's own pixels, scaled as a whole by `LayoutTransform = ScaleTransform(TilesheetMarks.DisplayScaleOf(sheet))` (`MapDrawing.CellPixels / sheet.TileWidth`, 3 for a 16 px sheet; 1 for an image not set up) inside a `ScrollViewer`, with nearest-neighbor scaling. Clicks and drags read `e.GetPosition(SheetView)`, which is in the grid's own coordinates (sheet pixels) whatever its layout transform, so the browser's hit tests (`CellAt`, `SelectCellAt`, `SelectCellsBetween`, `WalkabilityMarking.FlipCellAt`) need no change at any scale. `TilesheetMarks.Draw(sheet, imageSize, tiles, selected)` sizes its pens as `1 / DisplayScaleOf(sheet)` where `sheet` is the marked one: with the Cut panel open on an image not set up, that is the preview cut's scale (3 for a 16 px cut) while the view shows the image at 1, so the lines are a third of a pixel (a bug SZ2 fixes).
- **The map's zoom** (`Viewport/ViewportTransform`, pure: `ZoomAt(screen, zoom)` keeps the point under the mouse, 0.1 to 8) is applied by `Shell/MainWindow` on the plain wheel (1.15 a notch) as a render transform, not a scroll viewer; the Shapes window has its own wheel zoom in its code-behind. Neither has steps: a pixel-art sheet needs whole factors to stay crisp, so the sheet gets its own rule.
- **The side panel** (`MainWindow.xaml`) is 420 px wide by default, at least 300, beside a `GridSplitter`.

### Core changes
- `Core/Maps/ObjectTile` becomes `ObjectTile(int X, int Y, int TileId, int Level = 0)`: a fourth positional parameter with a default, so the about 25 test sites building `new ObjectTile(x, y, id)` keep compiling, and the strict serializer reads a missing `level` as 0 (an optional constructor parameter isn't required). `public const int MaxLevel = 7`. The summary says what a level is and that a higher one draws over a lower one (ADR 0040).
- `Core/Maps/ObjectDefinition.Tiles`' summary: each on its own cell and level, listed bottom level first, the order they are drawn in.

No rule moves into Core: the level is content shape; its checks are Data's (as every content check), its drawing is each shell's list order.

### Structure and folder dependencies

New files and types:

| Where | Type | Responsibility |
|---|---|---|
| `Data/Content` | `AbsentWhenZeroFilter` | Leaves a whole-number field added later out of a file while it is 0 (an object tile's `level`), so every file that predates it writes back byte for byte. |
| `Editor/Objects/Composing` | `CellOnLevel` | A canvas cell on one level: the key of the canvas's painted cells. |
| `Editor/Viewport` | `CellZoom` | A cell's shown size, stepped between bounds, and the scale and scroll offset it gives. |
| `Editor/Viewport` | `ZoomScrolling` | Scrolls a scroll viewer so the point under an anchor stays put after a scale change (view only). |
| `Editor/Viewport` | `CellZoomBar` | The −, size and + row bound to a `CellZoom` (a user control). |
| `Editor/Tiles` | `ShownSheet` | A sheet as the view shows it: its cut, its image's size and the view's scale (`TilesheetMarks.Draw`'s first argument). |

Changed: `Core/Maps/ObjectTile`, `ObjectDefinition` (summary), `Data/Maps/ObjectCatalogValidator` (the level rules), `Data/Content/ContentJsonOptions` (one modifier), `Game/Rendering/ObjectSpriteBaker` (`CopyTile` composes over; summary), `Game/Exploration/MapView` (a comment on the order), `Editor/Maps/MapDrawing` (a comment on the order), `Editor/Objects/Composing/ObjectCanvas` (cells by cell and level), `CanvasPainting` (the chosen level and its choices), `ObjectComposerViewModel` (`CanvasZoom`), `ObjectCanvasDrawing` (the chosen level, the line width), `ObjectComposerWindow` (Level box, canvas zoom, sharp pixels), `Editor/Tiles/TileBrowserViewModel` (`Zoom`), `TileBrowserView` (bar, wheel, scale), `TilesheetMarks` (`ShownScaleOf`, `Draw(ShownSheet, ...)`), `Editor/Shell/ControlsHelpWindow` (three lines).

Folder edges: Data `Content -> Core.Maps` exists. Game `Rendering -> Core.Maps` exists. Editor: `Viewport` stays a leaf (CommunityToolkit and WPF only; the starting size is passed in, so it never reads `Maps`); `Tiles -> Viewport` and `Objects/Composing -> Viewport` are new and one way (`Viewport` references no editor folder); `Objects/Composing -> Core.Maps` exists (`ObjectTile.MaxLevel`). No new project reference, no package.

### Key decisions
1. **A level on each tile, the list kept in level order** (decisions 472-474; ADR 0040). The level gives each stacked tile its identity (Edit reads a level with holes back as it was painted); the order rule lets every drawer keep its plain loop. The validator's checks run per tile in one pass: the tile id, the cell inside the object, the level in range, the level not below the previous tile's, and the (cell, level) pair not seen before. `FindTileProblem` splits into a per-tile check and the list's checks to stay under the complexity bound.
2. **Zero left out, as null is** (decision 472). `ObjectTile.Level` is an `int`, not an `int?` (no `?? 0` in every drawer); a sibling modifier to `AbsentWhenNullFilter`, `AbsentWhenZeroFilter`, sets `ShouldSerialize` to "not 0" for `(ObjectTile, "level")`, registered after it in `ContentJsonOptions`. The test world's frozen `objects.json` and the repository's then write back byte for byte (the identical-bytes tests are the guard).
3. **The canopy composes premultiplied "over"** (decision 476): `out = top + under × (1 − top.A / 255)` per channel, the formula `UiTheme.Over` uses (private to `Ui`, and `Rendering` doesn't reference `Ui`, so the bake writes its own one-line version with the same comment). The first tile on a transparent canopy gives what today's copy gives, so the existing bake tests pass unchanged.
4. **The canvas is one dictionary keyed by cell and level** (decisions 478, 480). `ObjectCanvas.Cells` becomes `IReadOnlyDictionary<CellOnLevel, SheetCell>`; `Stamp(level, at, cells)` and `Erase(level, at)` name their level (0 to `ObjectTile.MaxLevel`); `HighestLevel` is the highest painted level (−1 when blank); `Resize` keeps every level's cells inside; `Of(definition, tiles)` keys each tile by its cell and level; `TilesOf` writes level by level then row by row. There are no level objects to add or remove: a level exists while it holds a cell. `CanvasPainting.IsSamePicture` (the size and `Cells` compared by key) and `ObjectComposing.TryApply`'s `Cells.Values` keep working unchanged.
5. **The chosen level is the painting's** (decision 478). `CanvasPainting.Level` is what `PaintAt` and `EraseAt` act on. `LevelChoices` runs from 0 to `HighestLevel + 1`, at most `MaxLevel`, so one empty level always waits on top and an empty level is never saved. The setter ignores a value outside the choices (a combo box bound by `SelectedIndex` pushes -1 when its list is rebuilt, as `ObjectShapesWindow.OnZoneListSelectionChanged` guards); after the choices are rebuilt `Level` is raised again so the box shows it; when an undo or redo leaves the chosen level above the choices, the highest choice is chosen. `ObjectCanvasDrawing.Draw(canvas, chosenLevel, imageOf, gridLineWidth)` draws levels bottom first and pushes `UpperLevelOpacity` around those above the chosen one.
6. **The zoom is a cell's shown size** (decisions 481-484). `CellZoom` holds `CellPixels` (one of `Steps`), `ZoomIn`/`ZoomOut`/`Reset` commands greyed at the bounds, `ScaleFor(sourceCellPixels)` (`CellPixels / source`) and `static OffsetKeeping(offset, anchor, oldScale, newScale)` (`(offset + anchor) / oldScale × newScale − anchor`, never below 0; one axis at a time). The sheet's scale is `TilesheetMarks.ShownScaleOf(sheet, zoom)`: `zoom.ScaleFor(sheet.TileWidth)`, or `zoom.ScaleFor(MapDrawing.CellPixels)` for an image not set up. The canvas's is `CanvasZoom.ScaleFor(MapDrawing.CellPixels)`.
7. **Views apply the zoom as a layout transform** (inside their scroll viewer, as the sheet already is), so every click position read with `GetPosition(SheetView)` or `GetPosition(CanvasImage)` stays in sheet or canvas pixels: no hit test changes. On a size change the view redraws the marks or the grid (line width `1 / scale`: one WPF unit, a DIP, which is one device pixel at 100 % Windows scaling and not exactly one at 125 % or 150 %, as today), applies the transform, and `ZoomScrolling` keeps the anchor: the mouse for Ctrl + wheel, the viewer's center for the buttons. `ShownSheet` carries the view's own scale to the marks, so the pens match what is shown even while the Cut panel previews another cut.
8. **One zoom for the panel, shared with the picker** (decision 482). `TileBrowserViewModel.Zoom` is created with the view model (`new CellZoom(MapDrawing.CellPixels)`), so the panel's view and the composer's picker both show it. `TileBrowserView` subscribes to it in `OnDataContextChanged` and lets go there, as it does for the Cut panel, so a closed composer (whose picker's data context is set to null, decision 457) holds nothing.

### Extension points
- **A level shown or hidden in the composer:** a set of hidden levels on `CanvasPainting`, read by `ObjectCanvasDrawing`; no format change.
- **Zones or collision per level:** would need the format (a zone naming its level); the ADR's consequences say it adds, not replaces.
- **More levels:** raise `ObjectTile.MaxLevel`; the format holds any whole number.
- **The zoom remembered between runs:** the editor's settings (none exist yet) would store `CellZoom.CellPixels`.
- **The map's zoom in steps:** `ViewportTransform` could take `CellZoom`'s steps, if the developer wants the map crisp at every zoom.

### Replacement mapping

| Today | After this plan |
|---|---|
| `ObjectCatalogValidator`: one tile per cell | One tile per cell and level, levels 0 to 7, listed bottom level first (OL1) |
| `ObjectSpriteBaker.CopyTile` writes each masked pixel | Composes it over the pixel under it (OL2) |
| `ObjectCanvas.Cells` keyed by `GridCell` | Keyed by `CellOnLevel` (OL3) |
| `ObjectCanvas.Stamp(at, cells)`, `Erase(at)` | `Stamp(level, at, cells)`, `Erase(level, at)` (OL3) |
| `ObjectCanvasDrawing.Draw(canvas, imageOf)` | `Draw(canvas, chosenLevel, imageOf, gridLineWidth)` (OL4 the level, SZ3 the width) |
| `TilesheetMarks.DisplayScaleOf(sheet)` | `TilesheetMarks.ShownScaleOf(sheet, zoom)` (SZ2) |
| `TilesheetMarks.Draw(sheet, imageSize, tiles, selected)` | `Draw(ShownSheet shown, tiles, selected)` (SZ2) |

## Risks
- **A stacked object made before the composer reads levels:** between OL1 and OL3, a hand-edited stacked object makes Edit object throw (`ObjectCanvas.Of`'s `ToDictionary` keyed by cell). Nothing but a hand edit makes one; OL1's slice notes the gap, OL3 closes it.
- **Content an older build can't read:** the strict options refuse an unknown `level`, so an older game build stops on a new `objects.json` naming the file. Accepted: one developer, one build.
- **Hand-written order:** a hand-edited `objects.json` listing a level 1 tile before a level 0 one is refused at load, naming the tile, rather than drawn upside down; the editor always writes the order.
- **The Level box's -1:** a combo box bound by `SelectedIndex` pushes -1 when its items are replaced; unguarded, the next stroke would call `Stamp` with -1 and throw. The setter ignores it (decision 478) and a test pins it; OL4's hand check strokes right after a Ctrl+Z.
- **Canopy blending:** premultiplied math in the bake runs once per object at map load; no per-frame cost. Semi-transparent edges of the pack's overlays blend as the map's `SpriteBatch` would draw them. Only OL4's hand check sees it (the repository has no zoned object), so it draws a zone on purpose.
- **WPF scrolling after a zoom:** the extent changes only after layout, so `ZoomScrolling` calls `UpdateLayout` before scrolling; at the edges the viewer clamps the offset itself. The hand checks zoom at the corners.
- **Ctrl + wheel inside a scroll viewer:** handled in `PreviewMouseWheel` and marked handled, or the viewer scrolls as well.
- **Marks redrawn at each zoom step:** `TilesheetMarks.Draw` on a 64 x 64-cell sheet is about 4 000 rectangles, drawn once per step, not per frame.
- **The shared zoom:** zooming in the composer's picker zooms the Tiles panel too (expected; the hand check says so).
- **File sizes:** `TileBrowserViewModel` 325 (+2, already past 300, not crossing); `TileBrowserView.xaml.cs` 135 → about 185; `CanvasPainting` 165 → about 200; `ObjectCanvas` 103 → about 140; `ObjectComposerWindow.xaml.cs` 137 → about 175; `ObjectCatalogValidator` 112 → about 135. New test files beside the big ones: `ObjectCanvasLevelTests`, `CanvasPaintingLevelTests`, `ObjectComposingLevelTests` (`ObjectComposingTests` is 261), `TileBrowserZoomTests` (`TileBrowserViewModelTests` is 496), `TilesheetMarksTests`, `CellZoomTests`, `ContentWriterObjectLevelTests`.
- **Determinism:** no randomness, no time. **Save format:** untouched. **Content format:** one optional field, old files unchanged.
