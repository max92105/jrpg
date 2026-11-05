# SZ3. The composer's canvas zooms

Builds on design.md, "Key decisions" 6 and 7; decision 484. Reuses SZ2's `CellZoomBar` and `ZoomScrolling`.

- **Done, with deviations:** `CanvasZoom` starts at `(int)MapDrawing.CellPixels`, as SZ2's zoom; the wheel checks `CanExecute` before running a command (SZ1 and SZ2 reviews).

## Goal
The composer's canvas gets its own zoom bar over it and Ctrl + wheel toward the mouse, starting at the map's 48 px each time the composer opens. Its pixels are drawn sharp and square at every size (today `CanvasImage` sets no scaling mode, so WPF would smooth them once scaled). Strokes land on the cell under the mouse at every size; the grid lines stay one screen pixel wide (one WPF unit, a DIP: exact at 100 % Windows scaling, not at 125 % or 150 %, as today).

## New types
None.

## Changes
- `Editor/Objects/Composing/ObjectComposerViewModel`: `public CellZoom CanvasZoom { get; } = new(MapDrawing.CellPixels);` (the canvas's own zoom; the summary names it).
- `Editor/Objects/Composing/ObjectCanvasDrawing.Draw`: a last parameter `double gridLineWidth` (in canvas pixels; the window passes `1 / scale`), replacing the fixed `GridLineWidth` constant; four parameters with OL4's `chosenLevel`.
- `Editor/Objects/Composing/ObjectComposerWindow.xaml`: a `viewport:CellZoomBar` over the canvas's scroll viewer (`DataContext` `CanvasZoom`); the scroll viewer named `CanvasScroller` with `PreviewMouseWheel="OnCanvasMouseWheel"`; `CanvasImage` keeps `Stretch="None"`, gets `RenderOptions.BitmapScalingMode="NearestNeighbor"` (as the sheet view's `SheetView` has) and its scale as a layout transform.
- `ObjectComposerWindow.xaml.cs` (137, about 175 after OL4):
  - `DrawCanvas` passes `1 / scale` as the line width and sets `CanvasImage.LayoutTransform` from `CanvasZoom.ScaleFor(MapDrawing.CellPixels)`;
  - on `CanvasZoom.CellPixels` changing: redraw, then `ZoomScrolling.KeepAnchor` (the wheel's anchor, else the viewer's center); unsubscribed in `OnClosed`;
  - `OnCanvasMouseWheel`: with Ctrl held, the anchor is the mouse over `CanvasScroller`, the zoom steps, and the event is handled;
  - `StrokeAt` is unchanged: `e.GetPosition(CanvasImage)` is in the canvas's own pixels whatever its layout transform (a comment says so).
- `Editor/Shell/ControlsHelpWindow.xaml`, Objects: "In the composer, Ctrl + mouse wheel over the canvas, or − and + over it: zoom the canvas."

## Tests
- `ObjectComposerViewModelTests: CanvasZoom_New_ShowsTheMapCellSize`.
- `ObjectCanvasDrawingTests: Draw_GridLineWidth_IsTheWidthGiven` (the grid pen's thickness, e.g. 0.5).
- The existing `ObjectCanvasDrawingTests` pass the width (3 lines).

Size: about 40 lines of code and 12 of XAML, 2 tests (about 30): about 85, under the aim; nothing to fold in without crossing into another slice's file.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `Composing/ObjectComposerWindow` gains "its canvas zooms (`ObjectComposerViewModel.CanvasZoom`)".

## Hand check (yours)
1. Start the editor ("Game and editor" profile, F5); Objects > **New object...**. The canvas's bar shows "48 px".
2. Press **+** over the canvas: "64 px"; the cells grow, their pixels sharp and square (no blur), the grid stays thin. The picker's own bar (left) doesn't change.
3. Hold Ctrl and turn the wheel over the bottom-right canvas cell: the canvas grows around it, which stays under the mouse.
4. At "128 px", choose a cell in the picker and click the bottom-right canvas cell: the stamp lands on that cell. Right-click it: it's erased. Drag across a row: every cell crossed is painted.
5. Zoom out until **−** greys (16 px): the canvas is small; a click still stamps the cell under the mouse.
6. Press **Cancel** and open Objects > **New object...** again: the canvas is back at "48 px".
7. Press F1: the Objects section has the canvas zoom line.
