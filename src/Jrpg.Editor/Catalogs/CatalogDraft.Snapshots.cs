using Jrpg.Core.Content;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The catalog with one tile, object or sheet added, replaced or removed: the two functions of each
/// <see cref="CatalogEdit"/> are built from these.
/// </summary>
internal sealed partial class CatalogDraft
{
    private static ContentCatalog WithTile(ContentCatalog catalog, TileDefinition tile) =>
        catalog with { Tiles = new Dictionary<int, TileDefinition>(catalog.Tiles) { [tile.Id] = tile } };

    private static ContentCatalog WithTiles(ContentCatalog catalog, IEnumerable<TileDefinition> tiles)
    {
        var changed = new Dictionary<int, TileDefinition>(catalog.Tiles);
        foreach (var tile in tiles)
        {
            changed[tile.Id] = tile;
        }

        return catalog with { Tiles = changed };
    }

    private static ContentCatalog WithoutTile(ContentCatalog catalog, int tileId) =>
        catalog with { Tiles = catalog.Tiles.Where(pair => pair.Key != tileId).ToDictionary() };

    private static ContentCatalog WithObject(ContentCatalog catalog, ObjectDefinition definition) =>
        catalog with
        {
            Objects = new Dictionary<string, ObjectDefinition>(catalog.Objects) { [definition.Id] = definition },
        };

    private static ContentCatalog WithoutObject(ContentCatalog catalog, string objectId) =>
        catalog with { Objects = catalog.Objects.Where(pair => pair.Key != objectId).ToDictionary() };

    private static ContentCatalog WithTilesheet(ContentCatalog catalog, TilesheetDefinition sheet) =>
        catalog with
        {
            Tilesheets = new Dictionary<string, TilesheetDefinition>(catalog.Tilesheets) { [sheet.Id] = sheet },
        };

    private static ContentCatalog WithoutTilesheet(ContentCatalog catalog, string sheetId) =>
        catalog with { Tilesheets = catalog.Tilesheets.Where(pair => pair.Key != sheetId).ToDictionary() };
}
