# AE1. TileAnimationViewModel: the Animate panel's rules

Builds on design.md, "Key decisions" 5 and 9, and decisions 463 and 467.

## Goal
The Animate panel's rules, headless: it opens on the cell chosen in the Tiles panel (a cell of a set-up sheet with its image, a tile or not yet); its boxes start from the tile's animation, or from the defaults (4 frames, 150 ms); the frames to the right and the time are typed; what stops Apply is named, the frames running past the sheet's right edge included; the preview's cell for an elapsed time comes from Core's `FrameAt`. Applying comes in AE2. Kept apart from AE2 because the two together pass 300 lines.

## New types
- `Editor/Tiles/TileAnimationViewModel` (`internal sealed partial class : ObservableObject`): the Animate panel: the chosen cell's frames to the right and time, the preview's cell (AE2 adds Apply and Remove).
  - `TileAnimationViewModel(CatalogDraft catalog, TileBrowserViewModel tiles, SheetImages images)` (the images for the sheet's columns, as the browser reads them).
  - `bool CanOpen` (the browser's chosen cell is on a set-up sheet whose image is there; raised when the choice changes); `[RelayCommand] Open`, `[RelayCommand] Close`; `bool IsOpen`; `SheetCell? Cell` (the cell it opened on).
  - `[ObservableProperty] string FrameCountText`, `FrameMillisecondsText`; `string? Problem` ("Frames must be a whole number from 2.", "The time must be a whole number of milliseconds from 1.", "6 frames from column 9 run past the sheet's 12 columns.").
  - `SheetCell PreviewCellAt(TimeSpan elapsed)` (the cell `FrameAt` gives, the opened cell while there's a problem).

## Changes
- `Editor/Shell/MainViewModel`: `public TileAnimationViewModel? Animating { get; }`, built beside `Marking` (+3 lines).

## Tests (new `tests/Jrpg.Editor.Tests/Tiles/TileAnimationViewModelTests`, on the test world's catalog and image sizes)
- `TileAnimationViewModelTests: Open_AnimatedTile_StartsWithItsCountAndTime` (an animation set in code, not the defaults).
- `TileAnimationViewModelTests: Open_CellNotATileYet_StartsWithTheDefaults`.
- `TileAnimationViewModelTests: CanOpen_CellOfASheetWithoutItsImage_IsFalse` (and `CanOpen_NothingChosen_IsFalse`, `CanOpen_CellOfASetUpSheet_IsTrue`).
- `TileAnimationViewModelTests: Problem_FrameCounts` (theory: "2" accepted, "1", "x" refused).
- `TileAnimationViewModelTests: Problem_FrameTimes` (theory: "1" accepted, "0", "-5", "x" refused).
- `TileAnimationViewModelTests: Problem_LastFrameOnTheLastColumn_IsNull`; `Problem_LastFramePastTheLastColumn_SaysSo`.
- `TileAnimationViewModelTests: PreviewCellAt_AtTheFrameTime_IsTheNextCellToTheRight`.

Size: about 70 lines of code, 10 tests and rows (about 180).

## Verify by hand
Nothing to see yet (AE2).
