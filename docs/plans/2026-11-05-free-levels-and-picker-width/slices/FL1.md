# FL1. Levels from -7 to 7 in `objects.json`

Builds on design.md, "Core changes" and "Key decisions" 1; decisions 488 and 489. The ADR 0040 amendment is written first (README, "ADRs").

## Goal
An object tile's level may be any whole number from -7 to 7; 0 stays the base, left out of the file while 0. Data refuses a level outside -7 to 7 and a lower level listed after a higher one, naming the tile, and accepts a list that starts on a negative level. Every existing object loads and writes back byte for byte. Nothing in the editor can paint a negative level yet (FL2); the game would draw one under the base already, since it draws the list in order.

## New types
None.

## Changes
- `Core/Maps/ObjectTile`: `public const int MinLevel = -7;` beside `MaxLevel`, one summary for both ("the lowest and highest levels a tile may have: a technical limit, far past a shadow, a wall and a window over it"). The record's summary: a level runs from `MinLevel` to `MaxLevel`, 0 the base; a lower level is drawn under a higher one (ADR 0040 and its amendment).
- `Data/Maps/ObjectCatalogValidator`:
  - `FindSingleTileProblem`: `tile.Level is < ObjectTile.MinLevel or > ObjectTile.MaxLevel`; message `tile (x, y) has level 8; levels run from -7 to 7.` (both bounds from the constants).
  - `FindTileListProblem`: `previousLevel` starts at `ObjectTile.MinLevel` (at 0, a list starting on -1 is refused as "listed after a tile on level 0"); its comment says the list is lowest level first, negative levels included.
- `docs/decisions/0040-object-tile-levels.md`: the "Amendment (2026-11-05): free levels" section from the README's entry; `docs/decisions/README.md`, the 0040 line: "(-7 to 7, 0 the base, ...)".

## Tests
- `ObjectCatalogValidatorTests: Validate_TileLevelOutsideTheRange_ThrowsNamingTheTile`: rows re-pinned to -8, 8, `int.MinValue`, `int.MaxValue` (the -1 row leaves: -1 is a level now, the developer's decision) and the message to "levels run from -7 to 7".
- `ObjectCatalogValidatorTests: Validate_TileLevelAtEachBound_Passes`: rows -7, 0, 7 (the -7 row is new; 0 stays as the base).
- `ObjectCatalogValidatorTests: Validate_LevelUnderTheBaseListedFirst_Passes` (a tile on -1, then one on 0 on the same cell; fails today twice over: the range, then the order check's start at 0).
- `ObjectCatalogValidatorTests: Validate_LevelUnderTheBaseListedAfterTheBase_ThrowsNamingTheTile` (0 then -1: "tile (0, 1) on level -1 is listed after a tile on level 0; ...").
- `ContentWriterObjectLevelTests: WriteMapCatalogs_ObjectWithALevelUnderTheBase_ReadsBackEqual` (a copy of a test-world object with its first tile copied onto level -1, listed first; read back equal with `SequenceEqual`, as the level-one test does; fails today, the writer validating first).
- Guards that must pass unchanged: the identical-bytes writer tests (`ContentWriterTests`, `ContentWriterRepositoryTests`), every other `ObjectCatalogValidatorTests` case, `ContentWriterObjectLevelTests`' two existing cases.

Size: about 10 lines of code, 3 new tests and 2 re-pinned theories (about 50): about 60, plus the ADR amendment.

## Map
`docs/map.md`, Core `Maps` row: `ObjectTile` (an object's tile at a cell and level, -7 to 7, 0 the base, ADR 0040).

## Hand check (yours)
1. Start the editor: Visual Studio's toolbar startup list → "Game and editor" → F5 (or `dotnet run --project src/Jrpg.Editor`).
2. In the side panel's Objects tab choose any object you made with the composer; Objects > **Edit object...**; press **Save** without changing anything; in the Shapes window press **OK**.
3. Press Ctrl+S. In Visual Studio's Git Changes window, `content/objects.json` is not listed (written back byte for byte).
4. The game, started by the same F5, reaches the title with no content error.
