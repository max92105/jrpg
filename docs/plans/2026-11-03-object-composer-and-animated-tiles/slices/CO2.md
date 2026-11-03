# CO2. CanvasPainting: strokes, the typed size, the composer's own undo

Builds on design.md, "Key decisions" 3 and 4, and the README's picks 4, 6 and 7.

## Goal
The composer's painting, headless: a stroke (press, moves, release) stamps the Tiles panel's chosen cell or rectangle at each canvas cell it passes, or erases them (the right button, or the Erase toggle); the size is typed in two boxes and applied while valid; each stroke and each resize is one step of the composer's own undo and redo. Cells of a sheet whose image is missing are refused, as Replace cells refused them (decision 442).

## New types
- `Editor/Objects/Composing/CanvasPainting` (`internal sealed partial class : ObservableObject`): the composer's canvas as the developer paints it: strokes from the Tiles panel's chosen cells or erasing, the typed size, the strokes' own undo.
  - `CanvasPainting(ObjectCanvas start, TileBrowserViewModel picker)`.
  - `[ObservableProperty] ObjectCanvas Canvas` (private set); `TileBrowserViewModel Picker` (the window binds its sheet view to it).
  - `[ObservableProperty] bool IsErasing` (the visible Erase toggle; the right button erases whatever it says).
  - `[ObservableProperty] string ColumnsText`, `RowsText`; `string? SizeProblem` ("Columns must be a whole number from 1 to 32."); a valid change resizes as one step.
  - `void BeginStroke()`, `void PaintAt(GridCell at)` (stamps `Picker.MarkedCells`, or erases while `IsErasing`), `void EraseAt(GridCell at)`, `void EndStroke()`.
  - `[RelayCommand] Undo`, `Redo` with `CanUndo`, `CanRedo`.

## Changes
None outside the new type.

## Tests (new `tests/Jrpg.Editor.Tests/Objects/Composing/CanvasPaintingTests`, on the test world's catalog)
- `CanvasPaintingTests: PaintAt_RectangleChosenInThePicker_StampsItAtTheCell`.
- `CanvasPaintingTests: PaintAt_NothingChosen_ChangesNothing`.
- `CanvasPaintingTests: PaintAt_CellsOfASheetWithoutItsImage_ChangesNothing`.
- `CanvasPaintingTests: PaintAt_EraseToggleOn_EmptiesTheCell`.
- `CanvasPaintingTests: EraseAt_PaintedCell_EmptiesIt`.
- `CanvasPaintingTests: Undo_AfterAStrokeOverThreeCells_RestoresTheCanvasBeforeTheStroke`.
- `CanvasPaintingTests: Redo_AfterUndo_PaintsTheStrokeAgain`.
- `CanvasPaintingTests: Undo_AfterAResize_BringsTheDroppedCellsBack`.
- `CanvasPaintingTests: ColumnsText_ValidSizes_Resize` (theory rows 1 and 32, the bounds accepted).
- `CanvasPaintingTests: ColumnsText_InvalidSizes_SetSizeProblemAndKeepTheSize` (theory rows "0", "33", "x").

Size: about 90 lines of code, 10 tests (about 200).

## Verify by hand
Nothing to see yet (CO5).
