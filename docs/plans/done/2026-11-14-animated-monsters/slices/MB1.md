# MB1. An enemy's idle loop as content

Core + Data, about 200 lines (about 120 of them tests). Needs MB0 and ADR 0047 written. Decision 631. Design: "Core
changes", "Content format", "Structure" (Data).

## Goal
`enemies.json` may give an enemy `"idleAnimation": { "frameCount": n, "frameMilliseconds": t }`: checked as a tile's
animation is (named by the enemy), left out of the file while absent, and at startup its battle sprite must split into
whole frames.

## New types
- Data `Maps/AnimationProblems` (internal static): one animation's rule under a field name: frame count from
  `TileAnimation.MinFrames`, frame time from 1 ms. `string? Find(string field, TileAnimation animation)` (messages
  "`<field>.frameCount` must be at least 2 (was 1)."); `TileCatalogValidator` and `FieldSpriteCatalogValidator` call it
  instead of their own lines (their messages unchanged: the field names they pass are today's `animation` and
  `walk.frames`).

## Changes
- Core `Battle/EnemyDefinition`: `public TileAnimation? IdleAnimation { get; init; }` (summary in design, "Core
  changes"); Core `Maps/TileAnimation`'s summary names the enemy's loop.
- Data `Battle/EnemyCatalogValidator`: an enemy's `idleAnimation`, when present, through `AnimationProblems.Find
  ("idleAnimation", ...)`, in the enemy's message shape (`enemies.json: enemy 'gnoll': idleAnimation.frameCount must be
  at least 2 (was 1).`).
- Data `Content/AbsentWhenNullFilter`: `(typeof(EnemyDefinition), "idleAnimation")`, under decision 586's comment
  ("only an animated sprite has a loop").
- Data `Art/ArtReferenceCheck.Battle.cs` (MB0's partial; the main file unchanged): `AddPartialBattlerStrips`: every animated enemy's battle sprite
  that can be read, whose width isn't a whole number of `FrameCount` frames: `battlers/GnollWarrior.png: a 1000x128 strip
  doesn't split into 6 frames. Named by enemy 'gnoll_warrior'.` (beside `AddPartialFrameStrips`), the test a private
  `SplitsIntoFrames(int width, int frameCount)` (`width % frameCount == 0`). The summary names the strips.
- `docs/map.md`: Core `Battle` row names `IdleAnimation`; Data's validators row names `AnimationProblems`; the `Art` row
  names the strip check.

## Tests
- `EnemyCatalogValidatorTests` (85): `InvalidEnemies` rows: frame count 1; frame time 0; (null entry not applicable:
  an object, not a list). `Validate_IdleAnimationTwoFramesOneMillisecond_Passes` (the accepted bounds);
  `Validate_NoIdleAnimation_Passes`.
- `TileCatalogValidatorTests`, `FieldSpriteCatalogValidatorTests`: unchanged and passing (the fold's guard; their
  messages are pinned).
- `ContentWriterEnemyTests` (132): `WriteMapCatalogs_EnemyWithoutLoop_LeavesIdleAnimationOut`;
  `WriteMapCatalogs_EnemyWithLoop_WritesIt` (loads back equal).
- `ArtReferenceCheckBattlerTests` (new sibling; `ArtReferenceCheckTests` is at 239), a temp art folder with PNG headers
  written by the test (`PngHeaderFile`) and the test world's catalog `with` an enemy built in the test:
  - `FindProblems_StripOfWholeFrames_IsFine` (800 x 100, 8 frames).
  - `FindProblems_StripNotWholeFrames_NamesEnemyAndSize` (810 x 100, 8 frames; message pinned).
  - `FindProblems_StillSpriteOddWidth_IsFine` (no loop: the filter's other side).

## Verify by hand
None: no enemy is animated yet. The game and the editor start as before.

- **Done, with deviations:** the enemy's loop check sits in `EnemyCatalogValidator.Validate`'s chain beside the drop
  table's, not in `FindProblem`, which was at the complexity limit (CA1502). `Jrpg.Data.Tests` 890 → 899. Review: no Critical or
  Warning; `FindProblems`' summary names the strip check.
