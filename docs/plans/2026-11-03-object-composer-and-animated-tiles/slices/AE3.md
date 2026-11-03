# AE3. The Objects panel's preview and the composer's canvas animate

Builds on design.md, "Key decisions" 6 and 8, and decisions 466 and 470.

## Goal
The small pictures animate on the render tick: the Objects panel's preview of the chosen object, and the composer's canvas while it is open, show each animated tile's frame for the editor's animation time, redrawn only when `TileAnimationTime.FramesChanged` says a frame changed. The editor's map, the placing ghost and the Shapes window keep the first frame (decision 466; the animated map is a backlog line).

## New types
None.

## Changes
- `Editor/Tiles/TileImages`: `ImageSource CellImageAt(SheetCell cell, TimeSpan elapsed)` (the cell's tile's frame when the cell is an animated tile's own, through `TileDefinition.ColumnShownAt`; the cell itself otherwise) and `TileImageAt(int tileId, TimeSpan elapsed)`; both reuse `CellImageOf`'s cache.
- `Editor/Maps/MapDrawing.DrawObject(ObjectDefinition definition, TimeSpan elapsed)`: the previews' overload drawing each tile with `TileImageAt`; the existing `DrawObject(definition)` (the ghost, the Shapes window) keeps the first frame (about +5 lines, 211).
- `Editor/Objects/ObjectBrowserView.xaml.cs`: redraws `Preview` on `FramesChanged` while the chosen object has an animated tile (the window gives it the `TileAnimationTime`, as it gives `Drawing`).
- `Editor/Objects/Composing/ObjectComposerWindow`: built with the `TileAnimationTime` too; redraws the canvas with `cell => images.CellImageAt(cell, time.Elapsed)` on `FramesChanged`, unsubscribing on `Closed` beside the picker's release (CO5).
- `Editor/Shell/MainWindow.ObjectEditing.cs`: passes `AnimationTime` to the composer.

## Tests
- `TileImagesTests: CellImageAt_AnimatedTilesCellAtTheFrameTime_IsTheNextCellsImage` (the same cached instance as `CellImageOf` of the cell to the right).
- `TileImagesTests: CellImageAt_StillCell_IsItsOwnImage`.
- `TileImagesTests: CellImageAt_AnimationFrameOfAnotherTile_IsItsOwnImage` (the second frame's cell isn't animated itself: it stays still where painted).
- `MapDrawingTests` (new): `DrawObject_AtTheFrameTime_DrawsTheSecondFrame` (an object of one animated tile; the drawing's image source is `CellImageOf` of the next cell).

Size: about 50 lines of code, 4 tests (about 130).

## Verify by hand
- Compose an object with an animated cell (a waterfall tile among cliff parts) and Save: the Objects panel's preview cycles; the composer's canvas cycled while painting. The placing ghost and the map show the first frame; the game animates it (AN2).
- Choose a still object: the preview doesn't redraw (no flicker, no CPU in Task Manager beyond idle).
- Close the composer and choose another object: no lag (the closed window let go of the tick).

- **Done, with deviations:** the cell tests sit in a new `Tiles/TileImagesAnimationTests`; `MapDrawing.IsAnimated(definition)` tells the Objects panel whether its preview moves, so a still object is never redrawn; the composer's canvas redraws on every `FramesChanged` (a small picture); `MainWindow.xaml.cs` took one line (`ObjectBrowser.AnimationTime`), 571 lines. The pictures moving are your hand check. From the review: `IsAnimated` has its two tests; `TileImages` finds animated cells through a lookup built once per catalog change (the canvas asks for every cell on each redraw); the Objects panel's time setter lets go of an earlier time.
