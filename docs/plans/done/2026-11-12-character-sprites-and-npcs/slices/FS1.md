# FS1. Field sprite sheets load from `field-sprites.json`

Core + Data + content, about 260 lines (130 of them tests). Design: "Core changes", "Content format", key decision 1. Decisions 560, 561, 565, 566, 567; ADR 0043 written first.

## Goal
Every field sprite image the content names has a layout record in `content/field-sprites.json`; the loader refuses a sprite without one, naming the object or the new game. Nothing changes on screen: the two records hold today's numbers and nothing reads them yet but the loader.

## New types
- `Core/Sprites/FieldSpriteSheet` (public sealed record): one field sprite image's layout as content: `File`, `FrameWidth`, `FrameHeight`, `Scale`, `FeetPixels` (init, 0), `Walk`. `MaxScale = 8`.
- `Core/Sprites/FacingCycle` (public sealed record): a loop of frames per facing: `Rows`, `Frames` (`Maps/TileAnimation`).
- `Core/Sprites/FacingRows` (public sealed record): the sheet row each facing reads; `RowOf(Facing)` (throws `ArgumentOutOfRangeException` on an unknown value).
- `Data/Sprites/FieldSpriteCatalogValidator` (internal static): the rules for `field-sprites.json` (design, "Content format"), through `ContentListValidator` with the file as id.
- `Data/Sprites/FieldSpriteReferenceCheck` (internal static; not `...References`, which in Data names a record of ids a validator reads): the cross-file check: every object's and the new game's `fieldSpriteFile` has a record, exact case; called from `ContentCatalogLoader` after the new game loads.

## Changes
- `Core/Content/ContentCatalog`: `FieldSprites` (`IReadOnlyDictionary<string, FieldSpriteSheet>`, init, empty by default) keyed by file, ordinal.
- `Core/Maps/TileAnimation`: its summary names its second job (a sprite's walk loop, ADR to come).
- `Data/Content/ContentCatalogLoader`: `FieldSpritesFile = "field-sprites.json"` (internal), read and validated after the objects; the references checked after the new game.
- `Data/Content/AbsentWhenZeroFilter`: a row for `FieldSpriteSheet.feetPixels`.
- `content/field-sprites.json`: `npc_placeholder.png` and `player.png`, 48 x 48, scale 1, rows down 0, left 1, right 2, up 3, 8 frames at 100 ms, in the writer's layout.
- `tests/TestWorld/field-sprites.json`: the same two records, frozen; `TestWorld.props` excludes the live file; `tests/TestWorld/README.md` lists it.
- `docs/map.md`: a `Sprites` row in Core and Data.

## Tests
- `FacingRowsTests`: `RowOf_EachFacing_ReturnsItsRow` (theory, four rows); `RowOf_UnknownFacing_Throws`.
- `FieldSpriteCatalogValidatorTests` (new, Data `Sprites/`): `Validate_RepositoryShapedSheet_Passes`; `InvalidSheets` theory rows: blank file, file not ending in `.png`, file listed twice, frame width 0, frame height 0, scale 0, scale 9, feet negative, feet equal to the frame height, a row negative, frame count 1, frame time 0, frame count `int.MaxValue` passing (no overflow in the check).
- `ContentCatalogLoaderFieldSpriteTests` (new sibling; `ContentCatalogLoaderTests` is at 272): `Load_RepositoryContent_LoadsTwoSheets`; `Load_ObjectSpriteWithoutRecord_ThrowsNamingObject`; `Load_NewGameSpriteWithoutRecord_ThrowsNamingNewGame`; `Load_SpriteNamedInOtherCase_Throws`; `Load_MissingFieldSpritesFile_ThrowsNamingFile`.
- `TestWorldTests` (existing pattern): `FieldSprites_FrozenCopy_ListsEveryNamedSprite`.

## Verify by hand
None: nothing is drawn from the records yet. `dotnet run --project src/Jrpg.Game` starts as before.

- **Done, with deviations:** `ContentFileReader` now reports a missing content file as a content problem ("field-sprites.json: the file is missing.") instead of a bare `FileNotFoundException`, so the start names the file; the loader tests copy the test world, the repository load reads `content` directly.
