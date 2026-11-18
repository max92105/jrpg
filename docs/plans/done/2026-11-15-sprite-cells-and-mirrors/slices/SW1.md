# SW1. Each facing's setup as typed: row, cells or mirror

Editor, about 240 lines (about 140 of them tests). Decisions 761, 767, 778. Builds on SC3; design.md, "Replacement
mapping".

## Goal
The Character sprites window's setup holds each facing as typed, losslessly: its row (always kept, and saved: the
record's row), and what overrides it: nothing, a sequence of cells or a mirror of another facing; read from a record
and written back as one, Copy setup from included. Its problem line adds the window's own checks before Data's
message (decision 778), counting only what a facing reads: a Cells facing with no cell, a cell past the cut, a row
past the rows for a facing on its row, and the frames past the columns only while some facing reads its row. The
window still shows only the row boxes (rebound); SW3 adds the Shows boxes.

## New types
- `Editor/Sprites/FacingSource` (internal enum): what a facing reads in the setup: `Row`, `Cells`, `Mirror`.
- `Editor/Sprites/FacingSetup` (internal sealed partial, `ObservableObject`): one facing as typed: `Facing` (fixed),
  `Source`, `Row` (from 0, kept whatever the source), `Cells` (`IReadOnlyList<SpriteCell>`, replaced whole on a change
  so it notifies), `MirrorOf` (`Facing`, the first other facing by default). `CopyFrom(int row, FacingSequence?
  sequence)` (the source from what overrides the row); `ToSequence()` (`FacingSequence?`: the cells or the mirror,
  null on Row); `ProblemWithin(int columns, int rows)`: on Row, a row past the rows (today's message, moved here); on
  Cells, `Up has no cells: pick them on the image.` or `Up's cell 3 (5, 2) is outside the 4 x 4 frames.` (place,
  column and row counted from 1 as the image numbers them); on Mirror, nothing (Data's message).

## Changes
- `Editor/Sprites/FacingCycleSetup`: `DownRow` .. `RightRow` give way to `Down`, `Up`, `Left`, `Right` (`FacingSetup`,
  fixed instances); a change in any of them is raised as that property's name, so the setup's `OnWalkChanged` relay
  keeps feeding `Problem` and the preview; `CopyFrom(FacingCycle)` and `ToCycle()` go through the four (the rows from
  each `Row`, `Sequences` null when no facing has one, so a row record writes back unchanged); `ProblemWithin` asks
  each facing in the order down, up, left, right, then checks `Frames` against the columns only while some facing's
  source is Row (decision 767). Summary rewritten.
- `Editor/Sprites/FieldSpriteSetupViewModel.ForNew`: the four facings on rows 0-3 as today, through `CopyFrom`.
- `Editor/Sprites/FieldSpriteWindow.xaml`: the four row combo boxes bind `Walk.Down.Row` .. `Walk.Right.Row` (four
  bindings; no new control). Until SW3 a facing with a sequence keeps it, and its row box edits the kept row; no
  record has one before SW3.

## Tests
`FacingSetupTests` (new, `Editor.Tests/Sprites/`): `ToSequence_CellsSource_GivesItsCellsInOrder`;
`ToSequence_MirrorSource_NamesTheFacing`; `ToSequence_RowSource_IsNull`;
`ProblemWithin_CellsWithoutCell_NamesTheFacing`; `ProblemWithin_CellPastTheCut_NamesItsPlaceFromOne`;
`ProblemWithin_CellOnTheLastColumnAndRow_IsNull` (the bound); `ProblemWithin_RowPastTheRowsUnderASequence_IsNull`
(only what is read counts).

`FacingCycleSetupTests` (new): `CopyFrom_RecordWithSequenceAndMirror_ToCycleGivesItBack` (the record built in the
test: Down on row 0, Up a sequence over row 5, Left on row 1, Right mirroring Left over row 2; rows and sequences both
come back); `ToCycle_RowRecord_HasNoSequences`; `ProblemWithin_NoFacingOnRow_IgnoresFrames` (8 frames, 4 columns,
every facing on Cells or Mirror: no problem); `ProblemWithin_OneFacingOnRow_ChecksFrames` (the same with Down on its
row: the frames problem); `Changed_FacingCells_RaisesItsFacing` (the relay).

Existing: `FieldSpriteSetupViewModelTests` (6 `DownRow`-style uses), `FieldSpriteCopySetupTests` (2),
`FieldSpriteCatalogViewModelTests` (1) move to `Walk.Down.Row`; `FieldSpriteCopySetupTests` (107 lines) gains
`CopySetupFrom_SourceWithMirror_CopiesIt` (decision 570's "walk" copied whole).

## Verify by hand
The window shows the sheets, so the visual step is the developer's; the implementer runs the script.
1. (Developer) `dotnet run --project src/Jrpg.Editor`, NPCs > Character sprites..., choose `player.png`: the boxes show
   rows 1-4 as before; change Down's row and back; OK is lit; Cancel.
2. (Implementer) Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line. Read only its lines.

- **Done, with deviations:** as planned, plus `ProblemWithin_RowPastTheRowsOnRow_NamesIt` (the other side of the
  read-only rule). The copy test's fixture record gains Right mirroring Left for `CopySetupFrom_SourceWithMirror_CopiesIt`;
  `FieldSpriteCatalogViewModelTests` named no row property, so only `FieldSpriteSetupViewModelTests` moved to
  `Walk.Down.Row`. `ForNew` already went through `CopyFrom`, unchanged. Hand check step 1 is the developer's.
