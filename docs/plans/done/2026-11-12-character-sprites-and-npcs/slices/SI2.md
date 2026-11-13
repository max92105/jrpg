# SI2. The sprite setup's rules, the grid cut shared with the tile Cut panel

Editor, about 230 lines (115 of them tests). Design: "Tunables" (the guesses), key decisions 6 and 7. Decisions 562, 564, 565, 566, 568. Builds on SI1 (`CatalogProblems`).

## Goal
One sprite image's setup as the window will edit it: columns and rows typed, the frame size shown (the tile Cut panel's rule, now shared), the row each facing walks on, the walk's frame count and time, the scale and the feet line, the problem that greys OK, the record it gives, and the frame the preview shows for a facing at a time.

## New types
- `Editor/Tiles/GridCut` (internal static): the length a cut's cells get along one side of an image and why a count doesn't split it: `LengthAlong(imageLength, cells, margin, spacing)`, `ProblemAlong(imageLength, cells, margin, spacing, cellsName)`. Moved out of `SheetCutViewModel` (its private `SpanAlong`, `TileLengthAlong`, `ProblemAlong`) with no change in its messages; the sprite setup calls it with margin and spacing 0.
- `Editor/Sprites/FacingCycleSetup` (internal sealed partial, `ObservableObject`): one loop as typed: `DownRow`, `LeftRow`, `RightRow`, `UpRow` (0-based; the view shows them from 1, decision 568), `Frames`, `FrameMilliseconds`; `ProblemWithin(columns, rows)` (a row or the frames past the cut); `ToCycle()`; `CopyFrom(FacingCycle)` (SI3's Copy setup).
- `Editor/Sprites/FieldSpriteSetupViewModel` (internal sealed partial, `ObservableObject`): one image's setup as typed, built from a record (`From(sheet, imageSize)`) or for an image with none (`ForNew(file, imageSize, scaleGuess)`): `Columns`, `Rows`, `FrameWidth` and `FrameHeight` (read-only, from `GridCut`), `Walk` (`FacingCycleSetup`), `Scale`, `FeetPixels`, `RowChoices` (0 to `Rows - 1`, refreshed when `Rows` changes); `Problem`; `ToSheet()` (throws on a problem); `PreviewCellAt(Facing, TimeSpan elapsed)` (the record-to-be's `WalkCellAt`, null while there is a problem).
- `Editor/Sprites/FrameCutGuess` (internal static): a new image's columns and rows: 4 columns (tunable), and the rows that make square frames when the height splits evenly, else 1.
- `Editor/Sprites/SpriteScaleGuess` (internal static): 48 over the most common tile width among the set-up tilesheets when it divides evenly, else 1.

## Changes
- `Editor/Tiles/SheetCutViewModel`: reads `GridCut`; behavior unchanged (its existing tests pass untouched: the guard).
- `Problem`, in order: counts below 1; `GridCut.ProblemAlong` on each side; `Walk.ProblemWithin`; then `CatalogProblems.OfFieldSprites` on the catalog's records with this one in place. Every input it reads refreshes it, `ToSheet` and the preview (the editor rule on notes), a change inside `Walk` included.

## Tests
- `GridCutTests` (new): `LengthAlong_EvenSplit_IsTheCellLength`; `ProblemAlong_UnevenSplit_NamesPixelsAndCells`; `LengthAlong_MarginAndSpacing_LeavesThemOut` (the cases `SheetCutViewModelTests` pinned through the panel, now direct).
- `FieldSpriteSetupViewModelTests` (new): `ForNew_FourBySevenSquareSheet_GuessesFourColumnsSevenRows`; `ForNew_HeightNotSplittingSquare_GuessesOneRow`; `ForNew_ManaSeedSheetsSetUp_GuessesScaleThree`; `ForNew_NoSheetSetUp_GuessesScaleOne`; `ForNew_Defaults_WalkRowsDownLeftRightUp`; `From_PlayerRecord_ReadsItsNumbers`; `ToSheet_PlayerRecordUnchanged_EqualsTheRecord`; `Problem_ColumnsNotSplittingWidth_IsGridCutMessage`; `Problem_FacingRowPastRows_NamesFacing`; `Problem_WalkFramesPastColumns_SaysSo`; `Problem_ScaleZero_IsDataMessage`; `Problem_ChangingRowsBackIntoRange_Clears`; `Problem_WalkRowEditedInsideCycle_Refreshes`; `PreviewCellAt_WalkingLeft_StepsAlongLeftRow` (theory over three times); `PreviewCellAt_WithProblem_IsNull`; `RowChoices_RowsChanged_Follows`.
- `SheetCutViewModelTests`: unchanged, all passing.

## Verify by hand
1. Tiles > Set up sheet on any sheet: the Cut panel behaves as before (type a column count that doesn't split: the same message).

- **Done, with deviations:** `GridCut` is a readonly record struct of the margin and spacing (`GridCut.Plain` for a sprite sheet), so its two methods take three parameters, not five; the walk setup raises the parent's `Problem` through its own `PropertyChanged`; a new setup walks over as many frames as columns (4). Accepted (the review): an image with spare pixels past its record's frames (which the start allows) opens with a cut problem until the counts are retyped, since bought sheets are exact.
