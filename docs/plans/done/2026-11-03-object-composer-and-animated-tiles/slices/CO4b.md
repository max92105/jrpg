# CO4b. The canvas's picture

Builds on design.md, "Key decisions" 1, and decision 442 (missing art is marked, never a lockout). Split out of CO5 at approval, so the window's slice stays about 200 lines.

## Goal
The composer's canvas can be drawn before its window exists: each painted cell's image at the map's cell size (16 px art at 3x), a cell of a sheet whose image is missing drawn as the missing mark, the grid over the canvas so empty cells show. `TileImages` crops a sheet cell whether or not it is a tile yet, which the canvas needs (its cells aren't tiles until Save).

## New types
- `Editor/Objects/Composing/ObjectCanvasDrawing` (static): the canvas's picture: each painted cell's image at the map's cell size, with the grid. `static DrawingImage Draw(ObjectCanvas canvas, Func<SheetCell, ImageSource> imageOf)` (the window passes `TileImages.CellImageOf`; AE3 passes the frame at a time), clipped to the canvas, frozen.

## Changes
- `Editor/Tiles/TileImages`: `ImageSource CellImageOf(SheetCell cell)` (the crop by cell, cached by the sheet's record as today; `MissingArtMark.Picture` for a missing image or a cell past it); `TileImageOf` calls it with the tile's cell. The class summary names cells as well as tiles.

## Tests
- `TileImagesTests: CellImageOf_CellOfASheetWithoutItsImage_IsTheMark`.
- `TileImagesTests: CellImageOf_CellPastTheImage_IsTheMark`.
- `TileImagesTests: CellImageOf_CellNotATileYet_IsCropped`.
- `ObjectCanvasDrawingTests` (new): `Draw_TwoByThreeCanvas_IsTheCanvasSizeInMapPixels` (96 x 144, the cell size read from `MapDrawing.CellPixels`); `Draw_PaintedCell_AsksTheImageOfItsSheetCell` (the `imageOf` given records the cells asked).
- `TileImagesTests`' existing `TileImageOf` tests pass unchanged.

Size: about 45 lines of code, 5 tests (about 110).

## Verify by hand
Nothing to see yet (CO5).

- **Done, with deviations:** the canvas's grid shares the map's line color: `MapDrawing`'s private grid brush became `MapDrawing.GridBrush`, rather than a second copy of the color. From the review: the grid's lines are `MapDrawing.GridLines`, shared with the map's grid; a test checks where a cell is drawn.
