# SC1. A facing reads a sequence of cells: the record and its arithmetic

Core, about 220 lines (about 140 of them tests). Decisions 761-765, 767, 769. Builds on ADR 0054; design.md, "Core
changes" and "The record".

## Goal
A field sprite's facing may have a sequence of cells, picked anywhere on its sheet and in any order, which overrides
its row: `FieldSpriteSheet.StandingCell` gives the sequence's first cell, `WalkCellAt` plays its cells in order at the
walk's frame time and loops over its own count; the other facings read their rows as before. `ColumnsNeeded` and
`RowsNeeded` count only what each facing reads, so the art check and the import (both through
`ArtReferenceCheck.HoldsFrames`) refuse an image too small for a sequence, and no longer ask for a row nothing reads.

No caller changes: the game and the editor already draw through `StandingCell` and `WalkCellAt`, and `FacingRows` is
unchanged.

## New types
- `Core/Sprites/FacingSequence` (public sealed record): one facing's frames read off the sheet in order, in place of
  its row. `Cells` (`IReadOnlyList<SpriteCell>`, init, empty by default). Equality and hash by its cells in order
  (decision 769, as `Loot/DropTable` compares its entries, decision 604).
- `Core/Sprites/FacingSequences` (public sealed record): the facings whose sequence overrides their row; `Down`, `Up`,
  `Left`, `Right` (each `FacingSequence?`); `Of(Facing)` returns that facing's or null (an out-of-enum value throws,
  as `FacingRows.RowOf`).

## Changes
- `Core/Sprites/FacingCycle`: `Sequences` (`FacingSequences?`, init, null by default); the summary says a facing reads
  its sequence when it has one, else its row with `Frames.FrameCount` frames, every facing at `Frames`' frame time
  (decision 765), and that a row or a frame count nothing reads is kept as written (decision 767).
- `Core/Maps/TileAnimation`: `public static int LoopFrameAt(TimeSpan elapsed, TimeSpan frameTime, int frameCount)`,
  the loop formula moved out of `FrameAt` (which now calls it), same negative-time guard, same `long` ticks. The
  summary names its second user.
- `Core/Sprites/FieldSpriteSheet`:
  - private `FrameCountOf(Facing)` (the sequence's cell count, else `Walk.Frames.FrameCount`) and private
    `CellOf(Facing, int index)` (the sequence's cell, else `(index, row)`);
  - `StandingCell` is `CellOf(facing, 0)`; `WalkCellAt` is `CellOf(facing, TileAnimation.LoopFrameAt(walked,
    Walk.Frames.FrameDuration, FrameCountOf(facing)))`. No allocation (a property read, a switch, a division);
  - `ColumnsNeeded` becomes `long`: the farthest column read + 1: `Frames.FrameCount` only while some facing has no
    sequence, and every sequence cell's column; `RowsNeeded` the highest row read + 1, over the facings with no
    sequence and every sequence cell. Both in `long`, so a cell at `int.MaxValue` never wraps. Their summaries say
    "read".
- `Data/Art/ArtReferenceCheck.AddSpritesTooSmall`: drop the `(long)` cast on `ColumnsNeeded` the build now calls
  redundant (one line; no behavior change).

## Tests
`FieldSpriteSheetSequenceTests` (new, `Core.Tests/Sprites/`; the sheet built in the test with `with` over a row
sheet whose facings are on rows 0-3, 4 frames at 100 ms):
- `StandingCell_FacingWithSequence_IsItsFirstCell` (a sequence starting at (2, 5), so neither column 0 nor its row);
- `WalkCellAt_Sequence_PlaysItsCellsInOrder` (at 0, 100 and 200 ms);
- `WalkCellAt_SequenceRepeatingACell_ShowsItAgainInTurn` ((0,4), (1,4), (2,4), (1,4) at 300 ms);
- `WalkCellAt_AfterTheSequencesLastCell_LoopsToItsFirst` (a 3-cell sequence on 4-frame rows: at 300 ms the sequence
  is back at its first cell, a row facing at its fourth);
- `WalkCellAt_FacingWithoutSequence_StillReadsItsRow` (Up on a sequence, Left on row 1);
- `ColumnsNeeded_SequenceCellPastTheRowRun_CountsIt` (a cell at column 6 with 4-frame rows: 7);
- `ColumnsNeeded_EveryFacingOnASequence_IgnoresFrameCount` (frame count 8, cells up to column 2: 3);
- `RowsNeeded_SequenceCellBelowTheRows_CountsIt`;
- `RowsNeeded_RowUnderASequence_IsNotCounted` (Up's row 9 overridden, the others on rows 0-2, cells on row 1: 3);
- `ColumnsNeeded_CellAtIntMaxColumn_DoesNotWrap` and `RowsNeeded_CellAtIntMaxRow_DoesNotWrap`.

`FacingSequenceTests` (new): `Equals_SameCellsInAnotherList_IsEqual`; `GetHashCode_SameCellsInAnotherList_IsEqual`;
`Equals_SameCellsInAnotherOrder_Differs`; `FieldSpriteSheet_Equals_SameSequencesBuiltApart_IsEqual` and
`FieldSpriteSheet_GetHashCode_SameSequencesBuiltApart_IsEqual` (two whole sheets, each with its own lists: the
equality the window's `ToSheets` and `TileImages`' cache rely on).

`TileAnimationTests` (extended, 33 lines): `LoopFrameAt_CountOtherThanTheRecords_LoopsOverIt`. The existing `FrameAt`
tests are the guard that the move changed nothing.

Existing tests: `FieldSpriteSheetTests` (72) and `FacingRowsTests` (20) compile and pass unchanged
(`Assert.Equal(8, sheet.ColumnsNeeded)` infers `long`): the guard for row records.

## Verify by hand
None: no record names a sequence, and Data doesn't check one until SC2 (design.md, "Risks").
