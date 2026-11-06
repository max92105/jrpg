# OM1. Select a rectangle of map cells

Builds on design.md, "Structure" (`MapCellRectangle`) and the replacement mapping; decision 514 (Ctrl + drag in Tiles mode). Needs OM0 (`MainWindow.TilePainting.cs`).

## Goal
In Tiles mode, Ctrl + drag on the map selects a rectangle of cells (clipped to the map), drawn as a dashed cyan outline that stays after the release; Ctrl + click selects one cell; Esc clears it; showing another map clears it. Painting, filling and picking work as before (a plain drag still paints; Ctrl + drag no longer does). The selection is what OM2 and OM3 make an object of; nothing else uses it yet, so its menu entry comes in OM3 and this slice's visible control is the outline itself and its F1 line.

## New types
- `Editor/Maps/MapCellRectangle` (`internal readonly record struct MapCellRectangle(GridCell TopLeft, int Columns, int Rows)`): a rectangle of map cells: its top-left, columns and rows, and its cells.
  - `public static MapCellRectangle Between(GridCell corner, GridCell oppositeCorner)` (any order of the corners).
  - `public MapCellRectangle? ClippedTo(MapLayout layout)` (null when it lies wholly outside).
  - `public IEnumerable<GridCell> Cells` (row by row, `BrushCells.Rectangle`).

## Changes
- `Tiles/TilePaintingViewModel` (134, ends about 160): `[ObservableProperty] public partial MapCellRectangle? SelectedMapCells { get; private set; }`; `public void SelectMapCells(GridCell corner, GridCell oppositeCorner)` (clipped to the open map); `public void ClearMapSelection()`; `OpenMap` becomes `[ObservableProperty] public partial MapViewModel? OpenMap { get; set; }` with `OnOpenMapChanged` clearing the selection (same public shape).
- `Shell/MainWindow.TilePainting.cs` (OM0's; its summary adds "and the cell selection"):
  - `HandlePaintButton`: a left press with `Keyboard.Modifiers == ModifierKeys.Control` sets `_isSelectingCells` and starts the drag on the rectangle tool's path (`_rectangleStart`, captured), so `OnViewportMouseMove` and `OnViewportMouseUp` in `MainWindow.xaml.cs` need no change; `FinishRectangle` calls `SelectMapCells(start, end)` while `_isSelectingCells`, else paints or erases as today, and resets the flag (as `OnViewportLostMouseCapture`'s reset of `_rectangleStart` ends a drag, `ShowBrushAt` with no drag clears it).
  - `ShowBrushAt` hides the brush outline while selecting and draws the dashed outline from the start to the mouse, then the kept selection (`DrawMapSelection`, also run by the existing `Painting.PropertyChanged` subscription, which calls `ShowBrushAt`).
  - `HandleToolKey`: Escape clears the map selection when in Tiles mode with one (otherwise not handled, so Objects mode's Escape (MO1) still reaches `HandleObjectKey`).
- `Shell/MainWindow.xaml`: `<Rectangle x:Name="MapSelectionOutline" Stroke="Cyan" StrokeDashArray="4 2" Visibility="Collapsed" />` in the overlay canvas (+1, ends about 291).
- `Shell/ControlsHelpWindow.xaml`, Tiles: "Ctrl + drag: select a rectangle of map cells (dashed); Ctrl + click: one cell; Esc clears it." (OM3 adds what uses it.)

## Tests
New `Maps/MapCellRectangleTests`:
- `MapCellRectangleTests: Between_CornersInAnyOrder_GivesTheSameRectangle` (one test per order: top-left first and bottom-right first).
- `MapCellRectangleTests: ClippedTo_RectanglePastTheRightAndBottom_KeepsTheInsidePart`.
- `MapCellRectangleTests: ClippedTo_RectanglePastTheLeftAndTop_KeepsTheInsidePart` (corners at -2).
- `MapCellRectangleTests: ClippedTo_WhollyOutside_IsNull`.
- `MapCellRectangleTests: Cells_TwoByThree_ListsSixCellsRowByRow`.

In `Tiles/TilePaintingViewModelTests` (237 lines; these go in a new `Tiles/TilePaintingSelectionTests`):
- `TilePaintingSelectionTests: SelectMapCells_TwoCorners_SelectsTheRectangle`.
- `TilePaintingSelectionTests: SelectMapCells_PastTheMap_IsClipped`.
- `TilePaintingSelectionTests: ClearMapSelection_Selected_EmptiesIt`.
- `TilePaintingSelectionTests: OpenMap_Changed_ClearsTheSelection`.
- `TilePaintingSelectionTests: SelectMapCells_PaintsNothing` (the map stays clean: no step recorded).

Size: about 30 lines for the record, 20 in the view model, 35 in the window partial, 3 in XAML and F1; 11 tests (about 110): about 200.

## Map
`docs/map.md`, Editor `Maps` row: `MapCellRectangle` (a rectangle of map cells); `Tiles` row: `TilePaintingViewModel.SelectedMapCells` (Ctrl + drag on the map in Tiles mode).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5); Tiles tab; choose a tile.
2. Drag on the map: it paints as before. Ctrl + drag: a dashed cyan rectangle follows and stays after you let go; nothing is painted (Ctrl+Z has no new step).
3. Ctrl + drag past the map's edge: the rectangle stops at the edge.
4. Press Esc: the rectangle goes. Ctrl + click: one cell is outlined. Show another map: it goes.
5. Shift + click still fills, Alt + click still picks, R and F still change the tool.
