# BG3. The animated background record, loaded, validated and written

Core, Data; about 230 lines with tests. Needs ADR 0059. Decisions 896, 897. Design: "Animated backgrounds (ADR 0059)",
"Core changes".

## Goal
`content/animated-backgrounds.json` exists (an empty list) and loads into `ContentCatalog.AnimatedBackgrounds`, each
record checked; the editor's writer writes it back as it reads it. Core knows which frame shows at a time and where a
frame sits in its sheet. Nothing draws a record yet; no content lists one (BG4 adds the first, BG5 plays it).

## New types
- Core `Battle/AnimatedBackgroundDefinition` (public sealed record): one animated battle background's frame size and
  pace, by its file name in the backgrounds folder (ADR 0059).
  - `required string File`, `required int FrameWidth`, `required int FrameHeight`, `required TileAnimation Animation`;
  - `public int FrameAt(TimeSpan elapsed) => Animation.FrameAt(elapsed);`
  - `public (int X, int Y) FrameOrigin(int frame, int imageWidth)`: the top-left of the frame's cell, frames left to
    right then top to bottom, `imageWidth / FrameWidth` columns; throws `ArgumentOutOfRangeException` for a frame
    outside `0..FrameCount - 1` or an image narrower than one frame. Summary: the game, the editor's preview and the art
    check all read it.
- Data `Battle/AnimatedBackgroundCatalogValidator` (internal static): the rules for `animated-backgrounds.json`: each
  background named once by its file, a blank file refused, `frameWidth` and `frameHeight` from 1, `animation` checked by
  `Maps/AnimationProblems.Find("animation", ...)`, a missing `animation` refused ("animation is required"). Whether the
  image is there and holds the grid is the startup art check's (BG4).

## Changes
- Core `Content/ContentCatalog`: `AnimatedBackgrounds`, `IReadOnlyDictionary<string, AnimatedBackgroundDefinition>`,
  empty by default (ordinal), its summary: "the animated battle backgrounds' frame sizes and paces by file name, exact
  case (ADR 0059); a background with none is a still picture".
- Data `Content/ContentCatalogLoader`: `internal const string AnimatedBackgroundsFile = "animated-backgrounds.json";`,
  read and validated after the effect strips, into the catalog by file (ordinal).
- Data `Content/CatalogProblems` (about +20): `OfAnimatedBackgrounds(IReadOnlyList<AnimatedBackgroundDefinition>)` and
  `CheckAnimatedBackgrounds(...)`, as `OfEffectStrips`/`CheckEffectStrips`.
- Data `Content/ContentWriter.Catalogs`: `CheckCatalogs` calls `CheckAnimatedBackgrounds`; `SerializedCatalogs` writes
  the file, sorted by file (`SortedAnimatedBackgrounds`); the summary's "one place to add a catalog" holds.
- `content/animated-backgrounds.json`: `[]` and a trailing newline, CRLF, as the writer writes an empty list (check
  `ContentFileWriter.Serialize([])`'s bytes and match them).
- `docs/map.md`: Core `Battle` names `AnimatedBackgroundDefinition`; Data's validators line names
  `animated-backgrounds.json`.

## Tests
- `AnimatedBackgroundDefinitionTests` (Core, new; records built in the test, 480 x 270 frames):
  - `FrameOrigin_FirstFrame_IsTheTopLeft`
  - `FrameOrigin_LastFrameOfTheFirstRow_IsOnRowZero` (8 columns in a 3840 px image, frame 7)
  - `FrameOrigin_FirstFrameOfTheSecondRow_WrapsToColumnZero` (frame 8)
  - `FrameOrigin_ImageWidthNotAWholeNumberOfFrames_UsesTheWholeColumns` (a 4000 px image: still 8 columns)
  - `FrameOrigin_ImageNarrowerThanAFrame_Throws`
  - `FrameOrigin_FrameAtTheCount_Throws` and `FrameOrigin_NegativeFrame_Throws`
  - `FrameAt_PastTheLastFrame_LoopsToTheFirst`
- `AnimatedBackgroundCatalogValidatorTests` (Data, new), an invalid-records theory with rows: the file listed twice, a
  blank file, `frameWidth` 0, `frameHeight` 0, a one-frame animation, a 0 ms frame time, a null `animation`; and facts:
  - `Validate_SmallestAcceptedRecord_Accepts` (1 x 1 frames, 2 frames, 1 ms)
  - `Validate_EmptyList_Accepts`
  - `Validate_NullEntry_IsRefused` (the strict serializer lets a null element through)
- `ContentCatalogLoaderAnimatedBackgroundTests` (Data, new): `Load_FileWithARecord_ReadsItByFile` (a copied content
  root with one record written in the test).
- `ContentWriterAnimatedBackgroundTests` (Data, new):
  - `WriteMapCatalogsAndMaps_ARecordAdded_WritesItSortedByFile` (two records added out of order)
  - `WriteMapCatalogsAndMaps_InvalidRecord_IsRefusedAndNothingWritten`
- `ContentWriterRepositoryTests` (81 lines): `CatalogFileBytes` lists the catalog files by name; add
  `animated-backgrounds.json`, so `WriteMapCatalogs_UnchangedRepositoryCatalogs_WritesIdenticalBytes` checks the new
  file's bytes.

## Verify by hand
1. The game and the editor start with the empty file as before.
2. Add a record by hand to `content/animated-backgrounds.json` with `"frameWidth": 0`: the game's start names the file
   and "frameWidth". Revert.

- **Done, with deviations:** 424 lines with tests, not about 230 (about 110 of code; the listed tests alone are
  about 280). `OfAnimatedBackgrounds`'s two tests went to a new `CatalogProblemsAnimatedBackgroundTests` (30 lines),
  since `CatalogProblemsTests` is 268; `AnimatedBackgroundDefinitionTests` (97) adds the last frame, a one-frame-wide
  image, an `int.MaxValue`-wide image, frame `int.MaxValue` and the last frame time. `CatalogProblems.cs` is 256.
