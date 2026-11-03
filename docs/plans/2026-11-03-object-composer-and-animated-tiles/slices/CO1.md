# CO1. ObjectCanvas: an object's picture as sheet cells on a grid

Builds on design.md, "Key decisions" 1 and the README's picks 2, 4 and 6.

## Goal
The composer's rules for a picture, with no window yet: a canvas of a size, each cell empty or showing a sheet cell; a rectangle of sheet cells stamped with its top-left on a canvas cell (clipped, replacing what was there); a cell erased; the canvas resized (cells keep their place from the top-left, those outside drop); a canvas read from a rectangle (New object from the chosen cells) or from an existing object; the painted cells made into object tiles. `CatalogDraft.TileIdsOf` takes any cells, so the composer can make tiles of scattered cells.

## New types
- `Editor/Objects/Composing/ObjectCanvas` (`internal sealed class`, immutable): an object's picture being composed: its size and the sheet cell on each of its cells.
  - `public const int MaxCells = 32;`
  - `int Width`, `int Height`; `IReadOnlyDictionary<GridCell, SheetCell> Cells`; `bool IsBlank`.
  - `static ObjectCanvas Empty(int width, int height)`; `static ObjectCanvas Of(SheetCellRectangle cells)`; `static ObjectCanvas Of(ObjectDefinition definition, IReadOnlyDictionary<int, TileDefinition> tiles)` (each `ObjectTile` back to its tile's sheet cell).
  - `ObjectCanvas Stamp(GridCell at, SheetCellRectangle cells)`; `ObjectCanvas Erase(GridCell at)`; `ObjectCanvas Resize(int width, int height)` (throws `ArgumentOutOfRangeException` outside 1..`MaxCells`: the typed size is checked first, CO2).
  - `IReadOnlyList<ObjectTile> TilesOf(Func<SheetCell, int> tileIdOf)` (row by row from the top-left, as `ObjectFromCells.TilesOf` orders them, so a re-saved object compares equal in `CatalogDraft.IsSameObject`).

## Changes
- `Editor/Catalogs/CatalogDraft.TileIdsOf(SheetCellRectangle)` becomes `TileIdsOf(IReadOnlyCollection<SheetCell> cells)` (same body, distinct cells); its two callers pass `cells.Cells` (`MainViewModel.CreateObject`, `ObjectReskinning.Reskin`). No line growth (294).

## Tests (new `tests/Jrpg.Editor.Tests/Objects/Composing/ObjectCanvasTests`)
- `ObjectCanvasTests: Of_Rectangle_PaintsEachCellAtItsPlace`.
- `ObjectCanvasTests: Of_Object_ReadsEachTileBackToItsSheetCell`.
- `ObjectCanvasTests: Stamp_RectangleAtACell_PaintsItsCellsFromThere`.
- `ObjectCanvasTests: Stamp_PastTheRightAndBottomEdges_KeepsOnlyTheCellsInside`.
- `ObjectCanvasTests: Stamp_OverAPaintedCell_ReplacesIt`.
- `ObjectCanvasTests: Erase_PaintedCell_LeavesItEmpty`.
- `ObjectCanvasTests: Resize_Smaller_DropsTheCellsOutside`.
- `ObjectCanvasTests: Resize_Larger_KeepsTheCellsWhereTheyWere`.
- `ObjectCanvasTests: IsBlank_OneCellPainted_IsFalse` (and `IsBlank_Empty_IsTrue`).
- `ObjectCanvasTests: TilesOf_ScatteredCells_OneTilePerPaintedCellRowByRow`.
- Existing `MainViewModelTests.CreateObject_*` and `ObjectReskinningTests` pass unchanged (the callers' switch).

Size: about 80 lines of code, 11 tests (about 190).

## Verify by hand
Nothing to see yet: the composer's window comes in CO5. New object from selection and Replace cells still work (the `TileIdsOf` switch).
