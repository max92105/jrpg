# FS2. The startup art check reads each sheet's own layout

Core + Data, about 180 lines (100 of them tests). Design: "Core changes", "Replacement mapping". Builds on FS1.

## Goal
The game's startup refuses a field sprite image too small for its own record's frames (the walk's columns on the highest row it names), not for the fixed 8 x 4 grid of 48 px. A bought 4 x 7 sheet with a record of 4 frames passes.

## New types
None.

## Changes
- `Core/Sprites/FieldSpriteSheet`: `ColumnsNeeded` (the walk's frame count), `RowsNeeded` (the highest row the walk names, plus one), each counting whole frames.
- `Data/Art/ArtReferenceCheck`: `AddSpritesWithoutWalkFrames` becomes `AddSpritesTooSmall`, reading the record from `catalog.FieldSprites` for each named file (FS1 guarantees one); the message names the file, its size, the frames needed and their size ("villager.png: a 128x96 sprite doesn't hold 4 x 4 frames of 32 x 32 px (128x128). Named by object 'villager'."). The size test compares counts (`width / FrameWidth >= ColumnsNeeded`), never a pixel product, so a hand-typed count near the int limit can't wrap (the code-quality rule on bounds). `FieldSpriteLayout` stays for the game and the editor until FS5.
- The class summary of `ArtReferenceCheck` drops "the walk frames of `FieldSpriteLayout`".

## Tests
- `FieldSpriteSheetTests` (new, Core `Sprites/`): `ColumnsNeeded_EightFrameWalk_IsEight`; `RowsNeeded_RowsZeroToThree_IsFour`; `RowsNeeded_RepeatedRow_CountsHighest`.
- `ArtReferenceCheckFieldSpriteTests` (new sibling; `ArtReferenceCheckTests` is at 279): `FindProblems_SheetHoldingItsFrames_NoProblem` (a test-written 128 x 224 PNG, 32 px frames, 4 x 4 walk); `FindProblems_SheetTooNarrowForItsWalk_NamesFileAndFrames`; `FindProblems_SheetTooShortForItsRows_NamesFile`; `FindProblems_RecordCountsNearIntLimit_NoOverflow` (frame count `int.MaxValue` reported, not wrapped).
- `ArtReferenceCheckTests`: the existing walk-frame tests move to the sibling file with their expectations re-pinned to the record's message (count them before and after: same number of tests).

## Verify by hand
None: the repository's two sprites pass as before.
