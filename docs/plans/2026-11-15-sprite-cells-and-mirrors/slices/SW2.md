# SW2. Picking cells and the boxes' members: the rules

Editor, about 260 lines (about 170 of them tests). Decisions 766, 767, 771, 772. Builds on SW1.

## Goal
Everything the window's new boxes and the image's clicks need, headless:
- **The boxes' members**: each facing's Shows choices, its mirror choices (the three other facings), its cells as
  text ("4 cells: (1, 3) (2, 3) (3, 3) (2, 3)", column then row, from 1) and the flags that show the row box, the
  mirror box or the cells line; whether any facing reads its row (for greying "Frames per row"). Switching a facing
  from Row to Cells with no cell yet starts its sequence with that row's run (decision 771); switching back to Row
  reads its kept row; a facing's cells are kept while the window is open.
- **Picking** (decision 772): one picking facing at most, `FacingCycleSetup.PickingFacing`; switching a facing to
  Cells makes it the picking one, its line's Pick toggles it, leaving Cells clears it. A point on the image, in image
  pixels, adds the cell under it at the end of the picking facing's sequence, the same cell again adding it again; a
  point outside the whole frames (past the last whole column or row of an image that doesn't split evenly, or
  negative) adds nothing; Remove last and Clear edit the sequence.

## New types
- `Editor/Sprites/FacingSourceChoice` (internal sealed record): one entry of the Shows box: a `FacingSource` and its
  label; `All` the fixed three ("Row", "Cells", "Mirror of"), bound by `SelectedValue` (a fixed list, no rebuild).

## Changes
- `Editor/Sprites/FacingSetup`: `IsOnRow`, `IsOnCells`, `IsMirror` (notified with `Source`); `MirrorChoices` (the three
  other facings, fixed per instance); `CellsText` (notified with `Cells`); `AddCell(SpriteCell)`; `[RelayCommand]`s
  `RemoveLastCell` and `ClearCells`, each greyed with no cell, each replacing `Cells` whole.
- `Editor/Sprites/FacingCycleSetup`: on a facing's `Source` turning from Row to Cells with no cell, its `Cells` become
  the row's run (columns 0 to `Frames - 1` on its row) and it becomes `PickingFacing`; a facing leaving Cells clears
  `PickingFacing` when it was the one; `PickingFacing` (`FacingSetup?`, observable); `[RelayCommand] TogglePicking(
  FacingSetup facing)` (picks it, or clears when it already picks; greyed unless the facing is on Cells);
  `IsAnyFacingOnRow` (notified when a source changes).
- `Editor/Sprites/FieldSpriteSetupViewModel.PickCellAt(double x, double y)`: the cell at `(x / FrameWidth, y /
  FrameHeight)` within `Columns` and `Rows`, added to `Walk.PickingFacing`; nothing without a picking facing, outside
  the whole frames, or while the cut has a problem (a frame size below 1). A command, never a throw (the editor rule).

## Split point
The boxes' members (`FacingSourceChoice`, the flags, `MirrorChoices`, `CellsText`, `IsAnyFacingOnRow`, the seeding and
their `FacingSetupTests`/`FacingCycleSetupTests` lines, about 110 lines) and the picking (the rest, about 150) can be
two slices, SW2a and SW2b, if the review prefers each near 200.

## Tests
`FacingSetupTests` (SW1's, extended): `MirrorChoices_Left_ListsTheOtherThree`; `CellsText_ThreeCells_NumbersFromOne`;
`Source_ChangedToMirror_RaisesIsMirror` (the flag relay); `RemoveLastCell_ThreeCells_KeepsTheFirstTwo`;
`RemoveLastCellCanExecute_CellAdded_IsRaised` (the greyed button's relay); `ClearCells_ThenAddCell_StartsANewSequence`.

`FacingCycleSetupTests` (SW1's, extended): `Source_RowToCells_StartsWithTheRowsRun` (Frames 4 on row 2: (0,2) to
(3,2)); `Source_RowToCellsAgain_KeepsThePickedCells` (cells kept, not refilled); `ToCycle_CellsBackToRow_GivesTheRowKept`;
`IsAnyFacingOnRow_EveryFacingOffRow_IsFalse`; `IsAnyFacingOnRow_OneFacingOnRow_IsTrue` (the rule's two sides).

`FieldSpriteCellPickingTests` (new, `Editor.Tests/Sprites/`; a setup made with `FieldSpriteSetupViewModel.ForNew` on
an image size given in the test, 100 x 130 px cut in 4 x 4 frames of 25 x 32 px, so the bottom 2 px aren't a frame):
- `PickCellAt_InsideAFrame_AddsItsCellToThePickingFacing` ((60, 70) → (2, 2) on Up while Up picks);
- `PickCellAt_SameFrameTwice_AddsItTwice`;
- `PickCellAt_LastWholeFrame_AddsIt` ((99, 127) → (3, 3), the bound);
- `PickCellAt_BelowTheWholeRows_AddsNothing` ((10, 129));
- `PickCellAt_NegativePoint_AddsNothing`;
- `PickCellAt_NoFacingPicking_AddsNothing`;
- `Source_RowToCells_MakesItThePickingFacing`;
- `Source_LeavingCells_ClearsThePickingFacing`;
- `TogglePicking_ThePickingFacing_ClearsIt`.

## Verify by hand
None: the window's boxes, clicks and buttons are SW3.

- **Done, with deviations:** run whole (no SW2a/SW2b split), plus `TogglePickingCommand_FacingOnRow_CannotExecute` (the
  greyed side). `CopyFrom` clears the picking facing, so opening a record whose facing reads cells picks nothing until
  its Pick or its Shows box is used; `CellsText` says "1 cell" for one and "0 cells" for none. Review: the relays pass
  only typed fields to `Problem` (a facing's `Source`, `Row`, `Cells`, `MirrorOf`; not `PickingFacing` nor
  `IsAnyFacingOnRow`), so a Pick click keeps a refused copy's line; tests added for that, the column and top bounds,
  and the Pick and `CellsText` relays.
