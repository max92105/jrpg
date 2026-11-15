# SC3. A facing mirrors another: the record, the check and both shells' flip

Core, Data, Game, Editor, about 260 lines (about 150 of them tests). Decisions 764, 766, 769, 770. Builds on SC1,
SC2; design.md, "The record" and "Replacement mapping".

## Goal
A facing's sequence may be `{ "mirrorOf": "left" }`: it shows the named facing's frame at the same time (from its row
or its own sequence), flipped left to right in the same area. Core gives every frame as a `SpriteFrame` (its cell and
whether mirrored); the game's map and player drawing, the editor's map (`TileImages`) and both walk previews
(`WalkPreview`, the Character sprites and New NPC windows) draw a mirrored frame flipped. The loader refuses a
sequence with both cells and a mirror or neither, a mirror of itself and a mirror of a mirror.

## New types
- `Core/Sprites/SpriteFrame` (public readonly record struct `(SpriteCell Cell, bool IsMirrored)`): a frame to draw:
  its cell, and whether it is drawn flipped left to right.

## Changes
- `Core/Sprites/FacingSequence`: `MirrorOf` (`Facing?`, init, null by default); equality and hash include it; the
  summary says a sequence is cells or a mirror, never both (the loader's rule).
- `Core/Sprites/FieldSpriteSheet`: `StandingFrame(Facing)` and `WalkFrameAt(Facing, TimeSpan)` replace `StandingCell`
  and `WalkCellAt` (renamed, returning `SpriteFrame`): a mirror reads the mirrored facing's count and cell with
  `IsMirrored` set; `ColumnsNeeded` and `RowsNeeded` count nothing for a mirror (it reads what its facing reads; its
  own row stays unread).
- `Data/Content/AbsentWhenNullFilter`: `FacingSequence` `mirrorOf`. `AbsentWhenEmptyFilter`: `FacingSequence`
  `cells` (a mirror has none).
- `Data/Sprites/FieldSpriteCatalogValidator`: per sequence, before SC2's cell checks:
  - both: `walk.sequences.right has both cells and mirrorOf.`;
  - neither: `walk.sequences.right needs cells or mirrorOf.` (replaces SC2's "at least 1 cell" message; its theory
    row is re-pinned by name);
  - itself: `walk.sequences.right.mirrorOf can't name its own facing.`;
  - a mirror: `walk.sequences.right.mirrorOf names left, which is itself a mirror.` (the facing spelled through
    `ContentJsonOptions.EnumName`).
- `Game/Exploration/FieldSpriteFrame`: `EffectsOf(SpriteFrame)` → `SpriteEffects.FlipHorizontally` or `None`;
  `SourceOf` keeps taking the cell.
- `Game/Exploration/MapView.DrawNpc` and `ExplorationScene.DrawPlayer`: read the `SpriteFrame` and draw with the
  overload taking `SpriteEffects` (rotation 0, origin zero, depth 0), source from `frame.Cell`.
- `Editor/Tiles/TileImages.FieldSpriteFrameOf`: a mirrored standing frame is the cropped frame in a frozen
  `TransformedBitmap` with `ScaleTransform(-1, 1)`; cached by `(sheet, facing)` as today; the summary says so.
- `Editor/Sprites/WalkPreview`: remembers the last `SpriteFrame`s; draws a mirrored frame inside
  `PushTransform(new ScaleTransform(-1, 1, centerX, 0))` around its area's center, then `Pop`.
- `Editor/Sprites/FieldSpriteSetupViewModel.PreviewCellAt` → `PreviewFrameAt`, returning `SpriteFrame?`.

## Tests
`FieldSpriteSheetMirrorTests` (new, `Core.Tests/Sprites/`; the sheet built in the test, Right mirroring Left on row
1): `StandingFrame_MirrorFacing_IsTheMirroredFacingsFirstFlipped`;
`WalkFrameAt_MirrorFacing_FollowsTheMirroredFacingsFrame` (at 200 ms: Left's third cell, mirrored);
`WalkFrameAt_MirrorOfASequence_PlaysItsCellsFlipped`; `WalkFrameAt_RowFacing_IsNotMirrored`;
`RowsNeeded_MirrorFacingsOwnRow_IsNotCounted` (Right's row 9 under the mirror).
`FacingSequenceTests` (SC1's): `Equals_OtherMirror_Differs`.
Renamed calls: `FieldSpriteSheetTests` (4), `FieldSpriteSheetSequenceTests` (SC1's, read `.Cell`),
`FieldSpriteSetupViewModelTests` (4 `PreviewCellAt`).

`FieldSpriteCatalogValidatorTests`: rows for both, neither, itself, a mirror of a mirror; SC2's empty-sequence row
re-pinned to the "needs cells or mirrorOf" message; `Validate_MirrorOfARowFacing_Passes`;
`Validate_MirrorOfASequenceFacing_Passes`.
`ContentCatalogLoaderFieldSpriteTests`: `Load_MirrorRecord_ReadsItsFacing`.
`ContentWriterFieldSpriteTests`: `WriteFieldSprites_MirrorRecord_WritesOnlyMirrorOf` (no `cells` in the text).

`FieldSpriteFrameTests` (Game, 42 lines): `EffectsOf_MirroredFrame_FlipsHorizontally`;
`EffectsOf_PlainFrame_DrawsAsIs`.

`TileImagesSpriteTests` (Editor, 101 lines): `FieldSpriteFrameOf_MirrorFacing_IsTheMirroredFacingsFrameFlipped` (the
test world's placeholder with Right mirroring Left, through its `WithPlaceholder` helper widened to take the walk:
a `TransformedBitmap` whose source crops Left's row and whose transform's `ScaleX` is -1).

## Split point
The Editor flip (the flipped bitmap in `TileImages`, the pushed transform in `WalkPreview` and their test, about 40
lines) can be its own slice SC3b right after; the renamed calls (`.Cell` in both, `PreviewFrameAt`) stay in SC3, since
the rename breaks their build. Until SC3b the editor draws a mirror unflipped, and no record has one.

## Verify by hand
The developer's: the steps show the milestone map's Mana Seed tiles and the editor's sheets (decision 438). The
implementer runs the tests and step 1's start only.
1. Make a local edit to `content/field-sprites.json`, reverted in step 4: in `npc_placeholder.png`'s record add
   `"sequences": { "right": { "mirrorOf": "left" } }` to `walk` (its rows stay).
2. `dotnet run --project src/Jrpg.Game`, go to the milestone map, stand on the villager's right (east) side and press
   Confirm: while its event runs it faces you (ADR 0044) and shows its Left frame flipped, feet where they were.
3. `dotnet run --project src/Jrpg.Editor`, open the milestone map, Objects mode, Ctrl + click the villager; in the
   placed object's panel set Facing to Right: the map shows the flipped Left frame (Ctrl+Z after). NPCs > Character
   sprites..., choose `npc_placeholder.png`: in "Walking: down, left, right, up" the third figure walks as the second,
   flipped; close it with Cancel.
4. Revert the edit (`git checkout content/field-sprites.json`).
