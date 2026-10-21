using Jrpg.Core.Content;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// What uses a catalog record, so an undo that would remove it can be refused (decision 117): maps, saved or open, and
/// objects. The open map is read as it is on screen, unsaved changes included, in place of its saved version.
/// </summary>
internal static class CatalogUsage
{
    /// <param name="tileId">The tile looked for.</param>
    /// <param name="catalog">The saved maps and the objects.</param>
    /// <param name="openLayout">The open map's layout, unsaved changes included; null when no map is open.</param>
    /// <returns>"map forest" and "object chest" style names, maps first, each sorted by id; empty when unused.</returns>
    public static IReadOnlyList<string> UsersOfTile(int tileId, ContentCatalog catalog, MapLayout? openLayout)
    {
        var layouts = catalog.Maps.Values.Select(map => map.Layout).Where(layout => layout.Id != openLayout?.Id);
        if (openLayout is not null)
        {
            layouts = layouts.Append(openLayout);
        }

        var maps = layouts
            .Where(layout => layout.Layers.Any(layer => layer.Rows.Any(row => row.Contains(tileId))))
            .Select(layout => layout.Id)
            .Order(StringComparer.Ordinal)
            .Select(id => $"map {id}");
        var objects = catalog.Objects.Values
            .Where(definition => definition.Tiles.Any(tile => tile.TileId == tileId))
            .Select(definition => definition.Id)
            .Order(StringComparer.Ordinal)
            .Select(id => $"object {id}");
        return [.. maps, .. objects];
    }

    /// <summary>The tiles cut from a sheet, used or not: a tile can't outlive its sheet, so the sheet's removal waits for theirs.</summary>
    /// <returns>Their ids, lowest first.</returns>
    public static IReadOnlyList<int> TilesCutFrom(string sheetId, ContentCatalog catalog) =>
        [.. catalog.Tiles.Values.Where(tile => tile.SheetId == sheetId).Select(tile => tile.Id).Order()];
}
