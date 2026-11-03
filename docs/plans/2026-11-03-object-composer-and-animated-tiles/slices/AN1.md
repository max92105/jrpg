# AN1. TileAnimation: animated tiles in content

Builds on the ADR (animated tiles), design.md, "Core changes" and "Key decisions" 5 and 6, and the README's pick 9.

## Goal
A tile in `tiles.json` may carry an animation, loaded into Core, refused when malformed (naming the tile and the field), checked against its image at game startup, and written back as read; Core turns an elapsed time into the frame shown. Nothing draws it yet (AN2): the game shows the first frame, the tile's own cell, as before.

```json
{
  "id": 12,
  "isWalkable": false,
  "sheetId": "autumn_animations",
  "column": 0,
  "row": 3,
  "animation": {
    "frameMilliseconds": 150,
    "frames": [
      {
        "column": 0,
        "row": 3
      },
      {
        "column": 1,
        "row": 3
      }
    ]
  }
}
```

## New types
- `Core/Maps/TileAnimation` (`public sealed record`): a tile's frames and the time each shows; the frame shown after a time. `required int FrameMilliseconds`, `required IReadOnlyList<AnimationFrame> Frames`, `public const int MinFrames = 2`, `TimeSpan FrameDuration` (computed, left out of the file by `ComputedPropertyFilter`), `int FrameAt(TimeSpan elapsed)` (`(elapsed.Ticks / FrameDuration.Ticks) % Frames.Count` in `long`; the comment says why every copy is in step and why the shells pass the time in).
- `Core/Maps/AnimationFrame` (`public readonly record struct AnimationFrame(int Column, int Row)`): one frame's cell on the tile's sheet.

## Changes
- `Core/Maps/TileDefinition`: `public TileAnimation? Animation { get; init; }` (summary: the first frame is the tile's own cell, frames are on its sheet).
- `Data/Content/AbsentWhenNullFilter`: `(typeof(TileDefinition), "animation")` with its reason (the ADR: only animated tiles have one).
- `Data/Maps/TileCatalogValidator.FindProblem`: the animation's rules in a `FindAnimationProblem` helper (CA1502): `frameMilliseconds` at least 1; `frames` not null and at least `MinFrames`; `frames[0]` the tile's own cell; no negative column or row. Messages: "animation.frameMilliseconds must be at least 1 (was 0).", "animation.frames must hold at least 2 frames (had 1).", "animation.frames[0] must be the tile's own cell (2, 5) (was (3, 5)).", "animation.frames[1] has a negative column or row (-1, 0)."
- `Data/Art/ArtReferenceCheck`: the tile loop moves into `AddTilesOutside` (CA1502) and also names a frame outside the image ("...: tile 12's animation frame 2 (9, 3) lies outside the 128x64 image."); `FirstTileOutside` counts a tile whose frame lies outside, so `ImportTilesheetImage` refuses an image too small for the frames.
- `ArtReferenceCheck.FindProblems`' summary: "every tile's cell and animation frames lie inside its sheet".

## Tests
- `Core.Tests/Maps/TileAnimationTests` (new):
  - `FrameAt_Zero_IsTheFirstFrame`.
  - `FrameAt_JustBeforeTheFrameTime_IsStillTheFirstFrame`.
  - `FrameAt_AtTheFrameTime_IsTheSecondFrame`.
  - `FrameAt_PastTheLastFrame_WrapsToTheFirst` (three frames, 150 ms, 450 ms → 0, 600 ms → 1).
  - `FrameAt_AfterAYearOfPlay_StaysInRange`.
- `Data.Tests/Maps/TileCatalogValidatorTests`:
  - `Validate_AnimationAtTheBounds_DoesNotThrow` (2 frames, 1 ms: both bounds accepted).
  - `Validate_InvalidAnimations_ThrowNamingTheTile` (new theory `InvalidAnimations`, rows: 0 ms; one frame; no frames; first frame another cell; a negative frame; `frames` null).
- `Data.Tests/Content/ContentCatalogLoaderAnimationTests` (new, as the loader's quest and shop files): `Load_AnimationFrameNull_RefusesNamingTiles` (a `null` in `frames`: the struct can't hold it, the file is named).
- `Data.Tests/Content/ContentWriterAnimationTests` (new, as `ContentWriterMusicTests`): `WriteMapCatalogs_TileWithAnimation_ReadsBackEqual`; `WriteMapCatalogs_TileWithoutAnimation_LeavesTheFieldOut` (the identical-bytes test keeps passing).
- `Data.Tests/Art/ArtReferenceCheckTests`: `FindProblems_AnimationFrameOutsideTheImage_NamesTheTileAndFrame`; `FirstTileOutside_OnlyAFrameOutside_ReturnsTheTile`.

Size: about 75 lines of code, 13 tests and rows (about 230).

## Verify by hand
None in the game yet (the first frame shows as before). The game still starts on the repository content.
