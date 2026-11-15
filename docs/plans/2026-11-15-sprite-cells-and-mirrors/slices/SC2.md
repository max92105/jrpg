# SC2. Sequences checked on load and written back

Data, about 150 lines (about 100 of them tests). Decisions 762, 763, 768. Builds on SC1; design.md, "The record" and
"Risks" (the struct trap; nothing has a sequence between the slices).

## Goal
`field-sprites.json` may give a facing a sequence: the loader refuses an empty sequence, a cell with a negative column
or row, and a cell missing its column or row, naming the record and the facing; the writer leaves `sequences` out
while null, so today's files write back byte for byte; the startup art check refuses an image too small for a
sequence's cells. No editor change: the editor makes no sequence before SW3, and SW1 carries one losslessly.

## New types
- `Data/Content/RequiredStructMembers` (internal static, a contract modifier as the `AbsentWhen*Filter`s are): marks
  members of content structs required, `JsonPropertyInfo.IsRequired = true`, for `SpriteCell`'s `column` and `row`
  (about 15 lines with its `(Type, string)` list). Its summary gives the reason: the serializer builds a struct through
  its parameterless constructor, so a missing member would read as 0 (decision 768); `ObjectTile` and `GridCell`
  could join the list (backlog).

## Changes
- `Data/Content/ContentJsonOptions`: registers `RequiredStructMembers.MarkRequired` in `Modifiers`, beside the filters.
- `Data/Content/AbsentWhenNullFilter`: `FacingCycle` `sequences`; `FacingSequences` `down`, `up`, `left`, `right`.
  One comment line: "ADR 0054: only a facing that overrides its row has a sequence."
- `Data/Sprites/FieldSpriteCatalogValidator`: after the rows and frames (both checked as today, read or not, decision
  767), each facing's sequence (one helper taking the facing, so `FindWalkProblem` stays under CA1502):
  - an empty sequence: `walk.sequences.up.cells must hold at least 1 cell.`;
  - a cell: `walk.sequences.up.cells[2].column must be at least 0 (was -1).` (and `.row`), through `RangeProblems`;
  - the class summary gains the sequence rules.

## Tests
`FieldSpriteCatalogValidatorTests` (87 lines; new invalid cases as rows of `InvalidSheets`): an empty sequence, a
negative column, a negative row. New facts: `Validate_FacingOnASequence_Passes`; `Validate_SingleCellSequence_Passes`
(the bound).

`RequiredStructMembersTests` (new, `Data.Tests/Content/`): `Read_CellWithoutRow_IsRefusedNamingIt` (a `FacingSequence`
read through `ContentJsonOptions.Strict` from `{ "cells": [ { "column": 2 } ] }`; the message names `row`). A null,
unknown or doubled member is the strict serializer's own refusal and gets no test here.

`ContentCatalogLoaderFieldSpriteTests` (93 lines, extended): `Load_RecordWithSequence_ReadsItsCellsInOrder` (a copied
content root whose record is rewritten with a sequence, built in the test).

`ContentWriterFieldSpriteTests` (66 lines, extended): `WriteFieldSprites_RecordWithSequence_ReadsBackEqual`;
`WriteFieldSprites_RowRecord_WritesNoSequences` (the written text holds no `sequences`).

`ArtReferenceCheckFieldSpriteTests` (129 lines, extended): `Check_SequenceCellPastTheImage_NamesTheSprite` (the
record built in the test with `with` over the test world's player record).

## Verify by hand
1. `dotnet run --project src/Jrpg.Game`: starts as before (no record has a sequence). Non-visual: the implementer may
   run it.
2. In a scratch copy of `content/` (`-- --content <folder>`), give `npc_placeholder.png`'s Up a sequence whose first
   cell has column -1: the start stops naming `field-sprites.json`, `npc_placeholder.png` and
   `walk.sequences.up.cells[0].column` (read the console line, not a capture).

- **Note from SC1:** the `AbsentWhenNullFilter` line for `FacingCycle` `sequences` landed in SC1 (the repository's
  identical-bytes test needed it); SC2 adds the `FacingSequences` facings' lines.

- **Done, with deviations:** as planned, plus `WriteFieldSprites_SequenceForOneFacing_LeavesTheOthersOut` (the
  facings' filter lines). The validator checks the facings in the enum's order, each through one helper. Hand check
  step 1 (the game starts) is the developer's to see; step 2 is left to the validator's test rows, which assert the
  same message.
