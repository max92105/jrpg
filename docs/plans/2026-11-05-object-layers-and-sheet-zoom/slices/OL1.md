# OL1. `ObjectTile.Level` in the format

Builds on design.md, "Core changes" and "Key decisions" 1-2; decisions 472-475. ADR 0040 is written first (README, "ADRs").

## Goal
An object's tile carries its level on its cell, read from and written to `objects.json` as `level`, left out while 0. Data refuses two tiles on one cell and level, a level outside 0 to 7, and a tile listed after one on a higher level, naming the tile. Every existing object (all level 0) loads and writes back byte for byte.

## New types
- `Data/Content/AbsentWhenZeroFilter` (static): leaves a whole-number field added later out of a file while it is 0, so every file that predates it writes back byte for byte. `static void SkipZeroOptionalFields(JsonTypeInfo typeInfo)`, one entry: `(typeof(ObjectTile), "level")` with the reason (ADR 0040: only stacked tiles have a level above the base).

## Changes
- `Core/Maps/ObjectTile` becomes `ObjectTile(int X, int Y, int TileId, int Level = 0)` (a fourth positional parameter with a default: the existing `new ObjectTile(x, y, id)` sites compile unchanged, and the strict serializer reads a missing `level` as 0, an optional constructor parameter not being required) and gains `public const int MaxLevel = 7` (a technical limit, decision 473). The summary: one tile of an object's image, at a cell counted from its top-left cell and a level on that cell, 0 the base; a higher level is drawn over a lower one (ADR 0040).
- `Core/Maps/ObjectDefinition.Tiles`' summary: each on its own cell and level within the size, listed bottom level first, the order they are drawn in.
- `Data/Maps/ObjectCatalogValidator`: `FindTileProblem` splits into a per-tile check (the id, inside the object, the level from 0 to `ObjectTile.MaxLevel`) and the list's checks in the same pass (not below the previous tile's level; the (x, y, level) not seen before). Messages:
  - `tile (x, y) has level 8; levels run from 0 to 7.`
  - `tile (x, y) on level 0 is listed after a tile on level 1; tiles are listed bottom level first, the order they are drawn in.`
  - `tile (x, y) on level 0 is listed more than once; an object has one tile per cell on each level.` (replaces "an object has one tile per cell").
- `Data/Content/ContentJsonOptions`: `AbsentWhenZeroFilter.SkipZeroOptionalFields` registered after `AbsentWhenNullFilter`'s.
- Known gap until OL3: a stacked object written by hand makes Objects > Edit object throw (`ObjectCanvas.Of`); nothing in the editor makes one yet.

## Tests
- `ObjectCatalogValidatorTests: Validate_TwoTilesOnOneCellAndLevel_ThrowsNamingTheObject`: the existing `Validate_TwoTilesOnOneCell_ThrowsNamingTheObject` renamed and re-pinned on the new message (its expected value changes because the rule now names the level, not weakened).
- `ObjectCatalogValidatorTests: Validate_TwoTilesOnOneCellOnTwoLevels_Passes`.
- `ObjectCatalogValidatorTests: Validate_UpperLevelOnACellWithoutABaseTile_Passes`.
- `ObjectCatalogValidatorTests: Validate_TileLevelOutsideTheRange_ThrowsNamingTheTile` (theory: -1, 8, `int.MaxValue`).
- `ObjectCatalogValidatorTests: Validate_TileLevelAtEachBound_Passes` (theory: 0, 7).
- `ObjectCatalogValidatorTests: Validate_HigherLevelListedFirst_ThrowsNamingTheTile`.
- `ContentWriterObjectLevelTests` (new, on a `CopiedContentRoot` of the test world, as `ContentWriterAnimationTests`): `WriteMapCatalogs_ObjectWithALevelOneTile_ReadsBackEqual` (the tiles compared with `SequenceEqual`, since the record compares lists by reference); `WriteMapCatalogs_BaseLevelTiles_LeaveTheFieldOut` (`"level"` absent from `objects.json`).
- Guards that must pass unchanged: the identical-bytes writer tests on the test world and the repository (`ContentWriterTests`, `ContentWriterRepositoryTests`), every `ObjectCatalogValidatorTests` case but the re-pinned one.

Size: about 65 lines of code, 9 tests (about 110).

## Map
- `docs/map.md`, Core `Maps` row: `ObjectTile` (an object's tile at a cell and level, 0 the base, ADR 0040). Data `Content` row: `AbsentWhenZeroFilter` beside `AbsentWhenNullFilter`.

## Hand check (yours)
1. Start the editor: Visual Studio's toolbar startup list → "Game and editor" → F5 (or `dotnet run --project src/Jrpg.Editor`).
2. In the side panel's Tiles tab, choose any set-up sheet in the sheet box; in the tool bar tick **Mark blocked**; click one tile cell on the sheet twice (red cross on, then off); untick **Mark blocked**.
3. Press Ctrl+S.
4. In Visual Studio's Git Changes window: `content/objects.json` is not listed (written back byte for byte). Revert anything else the save touched if you don't want it.
5. Start the game (the same F5 profile): it starts with no content error.
