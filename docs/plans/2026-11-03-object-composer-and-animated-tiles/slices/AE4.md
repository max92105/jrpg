# AE4. The editor's map view animates

Builds on design.md, "Key decisions" 6 and 8, and the README's pick 12.

## Goal
On the editor's map, animated tiles cycle as in the game, in step, on layers, in objects and in canopies, with no redraw of the map per tick: each animated tile is painted with one shared brush whose picture the view timer switches. The previews (the Objects panel, the placing ghost, the Shapes window, the composer's canvas) keep the first frame. A tile animated, changed or made still in the Animate panel shows the change at once.

## New types
- `Editor/Tiles/AnimatedTileBrushes` (`internal sealed class`): one brush per animated tile, switched to the frame a time calls for, which the map drawing paints those tiles with.
  - `AnimatedTileBrushes(CatalogDraft catalog, TileImages images)`.
  - `Brush? BrushOf(int tileId)` (null for a still tile; a new brush when the tile's record changed, so a re-applied animation shows; nearest-neighbor scaling set on the brush).
  - `void ShowFrameAt(TimeSpan elapsed)` (sets each brush's `ImageSource` to `images.CellImageOf` of the frame `FrameAt` gives).

## Changes
- `Editor/Maps/MapDrawing`: built with the brushes too (`MapDrawing(CatalogDraft catalog, TileImages tileImages, AnimatedTileBrushes brushes)`); a tile with a brush is painted with `DrawRectangle(brush, null, cell)`, others with `DrawImage` as now; `DrawObject` (the previews) keeps `TileImageOf`; `Draw` freezes the image only when it painted no brush (summary says why). About +25 lines (211).
- `Editor/Shell/MainWindow.TileAnimation.cs`: the timer's tick also calls `ShowFrameAt`.
- `Editor/Shell/MainWindow.xaml.cs`: the brushes built beside `TileImages` and passed to `MapDrawing` (the same line, no growth).

## Tests
- `Tiles/AnimatedTileBrushesTests` (new):
  - `BrushOf_StillTile_IsNull`.
  - `ShowFrameAt_AtTheFrameTime_ShowsTheSecondFramesCell` (the brush's source is `CellImageOf` of the second frame: the same cached instance).
  - `BrushOf_AfterTheAnimationChanged_ShowsTheNewFrames`.
- `Maps/MapDrawingTests` (new): `Draw_MapWithAnAnimatedTile_IsNotFrozen`; `Draw_MapWithStillTilesOnly_IsFrozen`.

Size: about 70 lines of code, 5 tests (about 180).

## Verify by hand
- Paint an animated tile over a stretch of a 60 x 40 map and inside a composed object: the cells cycle in step on the map, in the placed object and under its canopy (the game shows the canopy part still, pick 11). The 3x pixels stay sharp (no blur from the brush). Painting and scrolling don't stutter.
- Change the tile's time in the Animate panel and Apply: the map follows at once. Ctrl+Z: back to the old time.
- The Objects panel's preview and the ghost show the first frame.
