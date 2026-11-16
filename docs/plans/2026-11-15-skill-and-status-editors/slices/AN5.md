# AN5. Strip from sheet cells: the cut and the picks as typed

Editor, about 210 lines (about 110 of them tests). Decision 990. Design: design.md, sections 1 ("Strip tool's pace")
and 2.4. Headless; needs no earlier AN slice.

## Goal
The rules of "Strip from sheet cells" before its window: a sheet's size, its cut as columns and rows (`GridCut.Plain`,
the sprite setup's rule), cells picked in order by a point on the image (a cell may repeat), Remove last and Clear, a
line of the picks, the strip's file name and pace, and the problem line that greys its OK; what OK will write as one
value. The tool holds no catalog session: what it writes is a file and a typed row of the Effect strips page (AN7).

## New types
- `Editor/Animation/CellStrip` (internal sealed record): the strip the tool writes: `FileName`, `(int Width, int
  Height) CellSize`, `IReadOnlyList<SpriteCell> Cells`, `int FrameMilliseconds`.
- `Editor/Animation/StripFromCellsViewModel` (internal sealed partial, `ObservableObject`): the tool's state.
  - `TakeSheet(string fileName, (int Width, int Height) size)`: the picks cleared, the columns and rows guessed as one
    row of square cells when the width splits into squares of the height (an effects strip), else 1 by 1; the strip's
    name the sheet's when empty. No file read here: the window reads the image.
  - `ColumnsText`, `RowsText`; `CellSize` (`(int, int)?`, null while the cut is a problem); `CutLine` ("6 x 2 cells of
    192 x 192 px").
  - `IsPicking` (on after `TakeSheet`); `PickCellAt(double x, double y)` (image pixels; outside the sheet or with no
    cut, nothing); `PickRow(int row)` (the review's nitpick: every cell of that row, from 0, left to right; the window
    calls it for a click on the row's number); `Picks`; `RemoveLastCommand`, `ClearCommand`; `PicksLine` ("4 cells:
    (1, 1) (2, 1) (3, 1) (2, 1)", numbered from 1 as `SequenceMarks` shows them).
  - `StripNameText` (a `.png` added when typed without one), `FrameMillisecondsText`
    (`EffectStripDefinition.UnlistedFrameTime`'s 80 to start) and `FrameMilliseconds` (`int?`, that box as a number,
    for the preview).
  - `Problem`: no sheet ("Open a sheet."), the cut (`GridCut.ProblemAlong`, each side), no pick ("Pick the cells the
    strip plays, in order."), a name with a path character or empty, a frame time that isn't a whole number from 1;
    `CanWrite`; `ToStrip()` throws on a `Problem`.
  - `PicksChanged` event: the picks or the cut changed, for the window's preview and marks.

## Tests
- `Editor.Tests/Animation/StripFromCellsViewModelTests` (new; sizes given in the test):
  - `TakeSheet_SixSquaresInARow_GuessesSixByOne`; `TakeSheet_NotSquares_GuessesOneByOne`.
  - `PickCellAt_PointInSecondColumn_AddsColumnTwo`; `PickCellAt_OutsideTheSheet_AddsNothing`.
  - `PickCellAt_SameCellTwice_KeepsBoth`.
  - `PickRow_SecondRowOfSix_AddsItsSixCellsLeftToRight`; `PickRow_PastTheCut_AddsNothing`; `PickRow_NotPicking_AddsNothing`.
  - `ColumnsChanged_PicksPastTheNewCut_Cleared` (a cut that no longer holds a picked cell clears the picks).
  - `PicksLine_FourPicks_ListsThemFromOne`.
  - `Problem_NoPick_AsksForThem`; `Problem_NameWithASlash_SaysSo`; `Problem_ColumnsNotSplitting_IsTheCutMessage`.
  - `ToStrip_TypedNameWithoutExtension_AddsPng`.
  - `RemoveLast_TwoPicks_LeavesOne`.

## Verify by hand
None: the window comes in AN6.
- **Done, with deviations:** code 271 lines (CellStrip 16, StripFromCellsViewModel 217, Sprites/CellPicking 38, the
  sprite setup's PickCellAt and FacingSetup.CellsText now calling it: the cell under a point and the picks' line, not a
  second copy), tests 543 in two files (StripFromCellsViewModelTests, StripFromCellsProblemTests); map rows named.
