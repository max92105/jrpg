# AI1. TiledTilesetReader: a Tiled .tsx file's animations

Builds on ADR 0039, design.md, "Key decisions" 10, and decision 469.

## Goal
Data reads a Tiled tileset file (`.tsx`, XML) into what the editor's import needs: its image's file name, its cut (tile width and height, columns, margin, spacing), and each animated tile's frame count and time by cell; or, for a file whose animations don't have ADR 0039's shape, the first offending tile and why. Reading is Data's (a file format, like `PngSize`); matching a sheet and changing the catalog are the editor's (AI2). The format is Tiled's documented one: `<tileset tilewidth tileheight columns margin spacing>`, `<image source>`, `<tile id="n"><animation><frame tileid="m" duration="ms"/>...</animation></tile>`, a tile id being `row * columns + column`.

## New types
- `Data/Art/TiledTilesetReader` (`public static`): reads a Tiled `.tsx` file into a `TiledTileset`, refusing XML it can't read with the file named.
  - `static TiledTileset Read(string tsxPath)`: throws `ContentValidationException` naming the file for unreadable XML or a missing `tilewidth`, `tileheight`, `columns` or `<image source>` (margin and spacing default to 0, as Tiled writes them only when set).
- `Data/Art/TiledTileset` (`public sealed record`): a `.tsx` as the import needs it: `ImageFile` (the `<image source>`'s file name, without its folder), `TileWidth`, `TileHeight`, `Margin`, `Spacing`, `Columns`, `Animations` (`IReadOnlyDictionary<(int Column, int Row), TileAnimation>`), `Refusal` (`string?`: the first animation of another shape, "tile 37: frames must be the tile then the next ones on its row." or "tile 37: frames last 70 and 140 ms; one time for all is needed."; null when every animation fits).

## Changes
None outside the new types. `System.Xml.Linq` is in the framework: no package.

## Tests (new `tests/Jrpg.Data.Tests/Art/TiledTilesetReaderTests`, each writing its `.tsx` text to a temporary folder)
- `TiledTilesetReaderTests: Read_TwoAnimatedTiles_GivesEachCellItsCountAndTime` (ids 0 and 13 on a 12-column tileset: cells (0, 0) and (1, 1)).
- `TiledTilesetReaderTests: Read_ImageInASubfolder_KeepsTheFileNameOnly`.
- `TiledTilesetReaderTests: Read_NoMarginOrSpacing_ReadsThemAsZero`.
- `TiledTilesetReaderTests: Read_TileWithoutAnimation_IsLeftOut`.
- `TiledTilesetReaderTests: Read_FramesNotFromTheTilesOwnId_SetsTheRefusal`.
- `TiledTilesetReaderTests: Read_FramesSkippingAnId_SetsTheRefusal`.
- `TiledTilesetReaderTests: Read_FramesPastTheRowsEnd_SetsTheRefusal` (the last frames wrap to the next row in Tiled's ids).
- `TiledTilesetReaderTests: Read_FramesWithTwoDurations_SetsTheRefusal`.
- `TiledTilesetReaderTests: Read_OneFrameAnimation_SetsTheRefusal` (below `TileAnimation.MinFrames`).
- `TiledTilesetReaderTests: Read_MissingColumns_ThrowsNamingTheFile`.
- `TiledTilesetReaderTests: Read_NotXml_ThrowsNamingTheFile`.

Size: about 75 lines of code, 11 tests (about 200).

## Verify by hand
Nothing to see yet (AI2).
