# AN1. TileAnimation: animated tiles in content

Builds on ADR 0039, design.md, "Core changes" and "Key decisions" 5 and 6, and decisions 463, 464 and 468.

## Goal
A tile in `tiles.json` may carry an animation (a frame count running rightward from its cell, and one time per frame), loaded into Core, refused when malformed (naming the tile and the field), checked against its image at game startup, and written back as read; Core turns an elapsed time into the frame shown and the column drawn. Nothing draws it yet (AN2): the game shows the first frame, the tile's own cell, as before.

```json
{
  "id": 12,
  "isWalkable": false,
  "sheetId": "waterfall_b",
  "column": 0,
  "row": 4,
  "animation": {
    "frameCount": 4,
    "frameMilliseconds": 140
  }
}
```

## New types
- `Core/Maps/TileAnimation` (`public sealed record`): a tile's frame count and the time each frame shows; the frame shown after a time.
  - `required int FrameCount`, `required int FrameMilliseconds`, `public const int MinFrames = 2`, `TimeSpan FrameDuration` (computed, left out of the file by `ComputedPropertyFilter`).
  - `int FrameAt(TimeSpan elapsed)`: `(elapsed.Ticks / FrameDuration.Ticks) % FrameCount` in `long`; throws `ArgumentOutOfRangeException` for a negative `elapsed` (both shells only add deltas; a negative time is a caller's bug). The comment says why every copy is in step and why the shells pass the time in.

## Changes
- `Core/Maps/TileDefinition`: `public TileAnimation? Animation { get; init; }` (summary: the frames are the tile's own cell and the cells to its right on the same row) and `public int ColumnShownAt(TimeSpan elapsed)` (`Column` plus `Animation.FrameAt(elapsed)`, `Column` for a still tile).
- `Data/Content/AbsentWhenNullFilter`: `(typeof(TileDefinition), "animation")` with its reason (ADR 0039: only animated tiles have one).
- `Data/Maps/TileCatalogValidator.FindProblem`: the animation's rules in a `FindAnimationProblem` helper (CA1502): `frameCount` at least `MinFrames`, `frameMilliseconds` at least 1. Messages: "animation.frameCount must be at least 2 (was 1).", "animation.frameMilliseconds must be at least 1 (was 0)."
- `Data/Art/ArtReferenceCheck`: the tile loop moves into `AddTilesOutside` (CA1502) and also names a tile whose last frame lies outside the image ("...: tile 12's last animation frame (7, 4) lies outside the 112x160 image."); `FirstTileOutside` counts a tile whose last frame lies outside, so `ImportTilesheetImage` refuses an image too small for the frames.
- `ArtReferenceCheck.FindProblems`' summary: "every tile's cell and its animation's last frame lie inside its sheet".

## Tests
- `Core.Tests/Maps/TileAnimationTests` (new):
  - `FrameAt_Zero_IsTheFirstFrame`.
  - `FrameAt_JustBeforeTheFrameTime_IsStillTheFirstFrame`.
  - `FrameAt_AtTheFrameTime_IsTheSecondFrame`.
  - `FrameAt_PastTheLastFrame_WrapsToTheFirst` (4 frames at 140 ms: 560 ms → 0, 700 ms → 1).
  - `FrameAt_AfterAYearOfPlay_StaysInRange`.
  - `FrameAt_NegativeTime_Throws`.
- `Core.Tests/Maps/TileDefinitionTests` (new): `ColumnShownAt_AnimatedTileAtTheThirdFrame_IsTwoColumnsRight` (the tile at column 5, so the column added isn't the default); `ColumnShownAt_StillTile_IsItsColumn`.
- `Data.Tests/Maps/TileCatalogValidatorTests`:
  - `Validate_AnimationAtTheBounds_DoesNotThrow` (2 frames, 1 ms: both bounds accepted).
  - `Validate_InvalidAnimations_ThrowNamingTheTile` (new theory `InvalidAnimations`, rows: 1 frame; 0 frames; 0 ms; -5 ms).
- `Data.Tests/Content/ContentWriterAnimationTests` (new, as `ContentWriterMusicTests`): `WriteMapCatalogs_TileWithAnimation_ReadsBackEqual`; `WriteMapCatalogs_TileWithoutAnimation_LeavesTheFieldOut` (the identical-bytes test keeps passing).
- `Data.Tests/Art/ArtReferenceCheckTests`: `FindProblems_LastFrameOutsideTheImage_NamesTheTile`; `FindProblems_LastFrameOnTheImagesLastColumn_FindsNothing` (the bound accepted); `FirstTileOutside_OnlyTheLastFrameOutside_ReturnsTheTile`.

Size: about 60 lines of code, 15 tests and rows (about 190).

## Verify by hand
None in the game yet (the first frame shows as before). The game still starts on the repository content.

- **Done, with deviations:** the last-frame tests sit in a new `Data.Tests/Art/ArtReferenceCheckAnimationTests` (`ArtReferenceCheckTests` is at 279 lines); the writer test looks for the `"animation"` field, since a test-world sheet id holds the word. `verify.ps1`'s parameter count read a tuple type's comma as a parameter (it flagged `ArtReferenceCheck`'s four-parameter methods once the file changed): nested groups are now dropped before the list is cut at its close. ADR 0039 written.
