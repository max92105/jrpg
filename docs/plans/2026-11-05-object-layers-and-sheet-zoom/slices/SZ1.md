# SZ1. `CellZoom`: the zoom rule

Builds on design.md, "Key decisions" 6 and 7; decisions 481 and 483. The rule before the views (SZ2, SZ3), as `ViewportTransform` is for the map.

- **Done, with deviations:** `ZoomOutCommand_ReachingTheSmallestStep_RaisesCanExecuteChanged` and `CellPixelsText_AfterZoomingIn_IsRaised` added (the other side of the bound, and the text's notification).

## Goal
One pure, tested type says how big a cell is shown: a size in fixed steps, stepping in and out within the bounds, back to the starting size, the scale that gives for a source cell size, and the scroll offset that keeps the point under the mouse in place when the scale changes. No view uses it yet.

## New types
- `Editor/Viewport/CellZoom` (`internal sealed partial class`, `ObservableObject`): a cell's shown size, stepped between bounds, and the scale and scroll offset it gives.
  - `public static IReadOnlyList<int> Steps { get; } = [16, 32, 48, 64, 96, 128, 192]` (screen pixels per cell; whole factors for 16 px sheets, decision 444);
  - `CellZoom(int startCellPixels)` (`ArgumentOutOfRangeException` unless it is a step; callers pass `MapDrawing.CellPixels`, so `Viewport` stays a leaf);
  - `[ObservableProperty] int CellPixels { get; private set; }` and `string CellPixelsText` ("48 px"), raised with it;
  - `[RelayCommand(CanExecute = nameof(CanZoomIn))] ZoomIn()`, `[RelayCommand(CanExecute = nameof(CanZoomOut))] ZoomOut()` (the next step up or down), `[RelayCommand] Reset()` (back to the start); each `CanExecute` re-raised when `CellPixels` changes;
  - `double ScaleFor(double sourceCellPixels)`: `CellPixels / sourceCellPixels` (3 for a 16 px sheet at 48);
  - `static double OffsetKeeping(double offset, double anchor, double oldScale, double newScale)`: the scroll offset on one axis that keeps the content point under `anchor` (a position in the viewer) where it was, `(offset + anchor) / oldScale * newScale - anchor`, never below 0.

## Changes
None outside the new type.

## Tests
- `CellZoomTests` (new, `tests/Jrpg.Editor.Tests/Viewport/`):
  - `Constructor_StartNotAStep_Throws`;
  - `ZoomIn_AtTheMapSize_ShowsSixtyFour` and `ZoomOut_AtTheMapSize_ShowsThirtyTwo`;
  - `ZoomInCommand_AtTheLargestStep_CannotExecute` and `ZoomInCommand_OneStepBelowTheLargest_CanExecute` (both sides);
  - `ZoomOutCommand_AtTheSmallestStep_CannotExecute` and `ZoomOutCommand_OneStepAboveTheSmallest_CanExecute`;
  - `ZoomInCommand_ReachingTheLargestStep_RaisesCanExecuteChanged`;
  - `Reset_AfterZoomingIn_ShowsTheStartSize` (starts from 64, so a no-op can't pass);
  - `CellPixelsText_AtTheMapSize_IsFortyEightPixels`;
  - `ScaleFor_SixteenPixelSourceAtTheMapSize_IsThree`;
  - `ScaleFor_SixteenPixelSourceAtEveryStep_IsAWholeNumber` (theory over `Steps`);
  - `OffsetKeeping_DoublingTheScale_KeepsTheContentPointUnderTheAnchor` (offset 100, anchor 50, scale 3 to 6: 250);
  - `OffsetKeeping_ZoomingOutNearTheTopLeft_IsNeverNegative`.

Size: about 60 lines of code, 14 tests (about 120): about 180.

## Map
`docs/map.md`, Editor `Viewport` row: purpose "Zoom and pan: the map's, and a cell's shown size for the sheet and the composer's canvas"; key types add `CellZoom`.

## Hand check (yours)
Nothing to see yet (SZ2).
