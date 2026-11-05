# SZ2. The Tiles panel's sheet zooms

Builds on design.md, "What exists" (the sheet view and its hit tests), "Key decisions" 6-8; decisions 481-483, 485 and 487.

- **Done, with deviations:** `TileBrowserViewModel.Zoom` starts at `(int)MapDrawing.CellPixels` (a `double` constant; `CellZoom`'s steps are whole pixels); the wheel checks `CanExecute` before running a command, since the toolkit's `Execute` doesn't (SZ1 review); `TileBrowserZoomTests` builds its browser on the test world with every image 512 px square; `ZoomScrolling` reads both offsets before `UpdateLayout`, which clamps them to the new extent (the plan said the other order).

## Goal
Over the sheet in the Tiles panel, a zoom bar: −, the shown cell size ("48 px"; a click goes back to 48) and +, greyed at the bounds. Ctrl + wheel over the sheet zooms toward the mouse; the plain wheel still scrolls. Clicks, drags, Mark blocked and the Cut panel's grid stay right at every size, and the marks' lines stay one screen pixel wide ("one screen pixel" is one WPF unit, a DIP: exact at 100 % Windows scaling, not at 125 % or 150 %, as today). The zoom is the panel's for the session and stays when another sheet is chosen; the composer's picker, the same view over the same view model, shares it.

Fixes a bug on the way: with the Cut panel open on an image not set up, the marks' pens are sized `1 / DisplayScaleOf(previewSheet)` (a third of a pixel for a 16 px cut) while the view shows the image at 1; `ShownSheet` carries the view's own scale, so they are one pixel.

## New types
- `Editor/Viewport/CellZoomBar` (user control, view only): the −, size and + row bound to a `CellZoom` (`ZoomOutCommand`, `ResetCommand` with `CellPixelsText` as its content and the tool tip "A cell's size on screen; click for the map's 48 px", `ZoomInCommand`; the theme's buttons).
- `Editor/Viewport/ZoomScrolling` (static, view only): scrolls a scroll viewer so the point under an anchor stays put after a scale change. `static void KeepAnchor(ScrollViewer viewer, Point anchor, double oldScale, double newScale)`: `UpdateLayout`, then both offsets from `CellZoom.OffsetKeeping`.
- `Editor/Tiles/ShownSheet` (sealed record): a sheet as the view shows it: its cut, its image's size in its own pixels and the view's scale. `ShownSheet(TilesheetDefinition Cut, Size ImageSize, double Scale)`.

## Changes
- `Editor/Tiles/TileBrowserViewModel`: `public CellZoom Zoom { get; } = new(MapDrawing.CellPixels);` (the panel's zoom, decision 482; two lines, the file goes from 325 to about 327).
- `Editor/Tiles/TilesheetMarks`: `DisplayScaleOf(sheet)` becomes `static double ShownScaleOf(TilesheetDefinition? sheet, CellZoom zoom)` (`zoom.ScaleFor(sheet.TileWidth)`, or `zoom.ScaleFor(MapDrawing.CellPixels)` for an image not set up); `Draw(ShownSheet shown, IReadOnlyList<TileDefinition> tiles, SheetCellRectangle? selected)`, pens `1 / shown.Scale` wide. The summary says the browser shows cells at the zoom's size, not the map's.
- `Editor/Tiles/TileBrowserView.xaml`: a `viewport:CellZoomBar` docked on top of the scroll viewer, its `DataContext` bound to `Zoom`; the scroll viewer named `SheetScroller` with `PreviewMouseWheel="OnSheetMouseWheel"`.
- `Editor/Tiles/TileBrowserView.xaml.cs` (135 → about 185):
  - subscribes to the browser's `Zoom` in `OnDataContextChanged` and lets go there, as for `_cut`, so the composer's picker drops it on close;
  - `DrawSheet` takes the scale from `ShownScaleOf` and keeps it in a field; on `CellPixels` changing, redraws, then `ZoomScrolling.KeepAnchor` with the anchor the wheel left (else the viewer's center) and the old and new scales;
  - `OnSheetMouseWheel`: with Ctrl held, the mouse's position over `SheetScroller` becomes the anchor, `ZoomInCommand` or `ZoomOutCommand` runs when it can, and the event is handled (so the viewer doesn't scroll); without Ctrl it does nothing;
  - the comment over `DrawSheet` drops "the scale stays put while the cut is typed" only if it stops being true (the scale still comes from the chosen sheet, not the Cut panel's preview, so it stays).
- `Editor/Shell/ControlsHelpWindow.xaml`, Tiles: "Ctrl + mouse wheel over the sheet: zoom toward the mouse. − and + over the sheet: smaller or bigger cells; the size between them goes back to the map's 48 px. The composer's picker shares the zoom."

## Tests
- `TileBrowserZoomTests` (new, beside `TileBrowserViewModelTests`, which is 496 lines): `Zoom_New_ShowsTheMapCellSize`; `Zoom_ChoosingAnotherSheet_KeepsTheCellSize` (zoomed to 96 first, so a reset would fail it).
- `TilesheetMarksTests` (new): `ShownScaleOf_SixteenPixelSheetAtSixtyFour_IsFour`; `ShownScaleOf_ImageNotSetUpAtTheMapSize_IsOne`; `ShownScaleOf_ImageNotSetUpAtNinetySix_IsTwo`; `Draw_AtScaleSix_DrawsTheGridOneSixthOfASheetPixelWide` (the grid pen's thickness read from the drawing); `Draw_PreviewCutOnAnImageShownAtOne_DrawsTheGridOnePixelWide` (the bug above: a 16 px cut previewed at scale 1, pen 1, not a third).
- The browser's hit tests (`TileBrowserViewModelTests` on `CellAt`, `SelectCellAt`, `SelectCellsBetween`) pass unchanged: positions stay in sheet pixels (design.md, "What exists").

Size: about 75 lines of code and 25 of XAML, 7 tests (about 85): about 185.

## Map
`docs/map.md`, Editor `Viewport` row: `CellZoomBar`, `ZoomScrolling`; `Tiles, Objects` row: `TilesheetMarks` (at the zoom's size), `ShownSheet`; the purpose notes "the sheet view zooms (`TileBrowserViewModel.Zoom`), shared with the composer's picker".

## Hand check (yours)
1. Start the editor ("Game and editor" profile, F5); Tiles tab; choose a set-up 16 px sheet in the sheet box. The bar over the sheet shows "48 px"; the sheet looks as before.
2. Press **+** twice: "64 px", then "96 px"; the cells grow, the pixels stay sharp and square; the grid lines stay thin.
3. Hold Ctrl and turn the wheel up with the mouse over one tile near the middle of the sheet: the sheet grows around that tile, which stays under the mouse; turn it down: it shrinks around it. Without Ctrl, the wheel scrolls the sheet as before.
4. Zoom in until **+** greys (192 px), then out until **−** greys (16 px). Click the size between them: back to "48 px".
5. At "96 px", click a cell: it's outlined; drag over several: the dashed rectangle covers exactly the cells dragged over. Scroll to the far right and bottom corner and click the last cell: it's the one chosen (the status line under the sheet names it).
6. Tool bar **Mark blocked** on: click a tile: the red cross is on that tile. Click it again; **Mark blocked** off.
7. Choose another sheet in the box: the size stays "96 px".
8. Choose an image "(not set up)", press **Set up**: the Cut panel's grid lines up with the image at this size; press **Cancel** in the Cut panel.
9. Objects > **New object...**: the composer's picker shows the sheet at "96 px" with the same bar; zoom it to "64 px", press **Cancel**: the Tiles panel's sheet is at "64 px" too.
10. Drag the bar between the map and the side panel to widen the panel: the sheet uses the room.
11. Press F1: the Tiles section has the zoom line.
