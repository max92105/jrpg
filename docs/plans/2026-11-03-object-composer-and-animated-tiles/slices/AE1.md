# AE1. TileAnimationViewModel: the Animate panel's frames and time

Builds on design.md, "Key decisions" 5 and 9, and the README's picks 9 and 13.

## Goal
The Animate panel's rules, headless: it opens on the cell chosen in the Tiles panel (a cell of a set-up sheet with its image, a tile or not yet); its frames start from the tile's animation, or from the cell alone; a cell clicked on the same sheet is added as the next frame (another sheet's is refused with a line saying why); the last frame can be taken off, never the first (the tile's own cell); the time is typed in whole milliseconds; what stops Apply is named; the preview's cell for an elapsed time comes from Core's `FrameAt`. Applying comes in AE2.

## New types
- `Editor/Tiles/TileAnimationViewModel` (`internal sealed partial class : ObservableObject`): the Animate panel: the chosen tile's frames clicked in order, the typed time, the preview's frame (AE2 adds Apply and Remove).
  - `TileAnimationViewModel(CatalogDraft catalog, TileBrowserViewModel tiles)`.
  - `public const int DefaultFrameMilliseconds = 150;`
  - `bool CanOpen` (the browser's chosen cell is on a set-up sheet whose image is there; raised when the choice changes); `[RelayCommand] Open`, `[RelayCommand] Close`; `bool IsOpen`.
  - `IReadOnlyList<SheetCell> Frames`; `void AddFrameAt(SheetCell cell)`; `[RelayCommand] RemoveLastFrame` (enabled with two frames or more).
  - `[ObservableProperty] string FrameMillisecondsText`; `string? Problem` ("The time must be a whole number of milliseconds from 1.", "Click at least one more frame on the sheet.").
  - `string Status` (the frames count, or why a click was refused: "Frames must be on the tile's own sheet.").
  - `SheetCell PreviewCellAt(TimeSpan elapsed)` (the frame `TileAnimation.FrameAt` gives, the first while there's a problem).

## Changes
None outside the new type.

## Tests (new `tests/Jrpg.Editor.Tests/Tiles/TileAnimationViewModelTests`, on the test world's catalog)
- `TileAnimationViewModelTests: Open_AnimatedTile_StartsWithItsFramesAndTime`.
- `TileAnimationViewModelTests: Open_CellNotATileYet_StartsWithTheCellAndTheDefaultTime`.
- `TileAnimationViewModelTests: CanOpen_CellOfASheetWithoutItsImage_IsFalse` (and `CanOpen_NothingChosen_IsFalse`).
- `TileAnimationViewModelTests: AddFrameAt_CellOfTheSameSheet_AppendsIt`.
- `TileAnimationViewModelTests: AddFrameAt_CellOfAnotherSheet_KeepsTheFramesAndSaysWhy`.
- `TileAnimationViewModelTests: RemoveLastFrame_OnlyTheFirstLeft_CannotExecute`.
- `TileAnimationViewModelTests: Problem_OneFrame_AsksForAnother`.
- `TileAnimationViewModelTests: Problem_FrameTimes` (theory: "1" accepted, "0", "-5", "x" refused).
- `TileAnimationViewModelTests: PreviewCellAt_AtTheFrameTime_IsTheSecondFrame`.

Size: about 80 lines of code, 10 tests and rows (about 190).

## Verify by hand
Nothing to see yet (AE3).
