using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The one cascade (decision 442, design key decision 6): a sheet whose image is missing can't be cleared by erasing
/// what can't be seen, so its removal takes its tiles, erases their cells on every map, and removes the objects made of
/// any of them with their placements. Pure: content in, purged content out.
/// </summary>
internal static class SheetPurge
{
    /// <summary>
    /// Why the purge can't go: an event that stays names an object that would go, as a swap or a gate, which the save
    /// would refuse; null when nothing stands in the way.
    /// </summary>
    public static string? RefusalOf(ContentCatalog catalog, string sheetId)
    {
        var removed = ObjectsUsing(catalog, sheetId);
        foreach (var layout in catalog.Maps.Values.Select(map => map.Layout).OrderBy(layout => layout.Id, StringComparer.Ordinal))
        {
            var staying = layout.Objects.Where(placed => !removed.Contains(placed.ObjectId)).SelectMany(placed => placed.Events)
                .Concat(layout.CellEvents.Select(cellEvent => cellEvent.Event));
            if (staying.SelectMany(ObjectsNamedBy).FirstOrDefault(removed.Contains) is { } named)
            {
                return $"Can't remove sheet {sheetId}: an event on map {layout.Id} names object {named}, which is made of its tiles.";
            }
        }

        return null;
    }

    /// <exception cref="InvalidOperationException">
    /// The purge is refused (<see cref="RefusalOf"/>), or no sheet has the id.
    /// </exception>
    public static SheetPurgeResult Purge(ContentCatalog catalog, string sheetId)
    {
        if (!catalog.Tilesheets.ContainsKey(sheetId))
        {
            throw new InvalidOperationException($"No sheet '{sheetId}' to remove.");
        }

        if (RefusalOf(catalog, sheetId) is { } refusal)
        {
            throw new InvalidOperationException(refusal);
        }

        var tileIds = catalog.Tiles.Values.Where(tile => tile.SheetId == sheetId).Select(tile => tile.Id).ToHashSet();
        var objectIds = ObjectsUsing(catalog, sheetId);
        var maps = new Dictionary<string, MapDefinition>(catalog.Maps);
        var changed = new List<string>();
        var (cells, placements, withEvents) = (0, 0, 0);
        foreach (var map in catalog.Maps.Values)
        {
            var erased = map.Layout.Layers.Sum(layer => layer.Rows.Sum(row => row.Count(tileIds.Contains)));
            var gone = map.Layout.Objects.Where(placed => objectIds.Contains(placed.ObjectId)).ToList();
            if (erased == 0 && gone.Count == 0)
            {
                continue;
            }

            maps[map.Id] = new MapDefinition(Without(map.Layout, tileIds, objectIds), map.Settings);
            changed.Add(map.Id);
            cells += erased;
            placements += gone.Count;
            withEvents += gone.Count(placed => placed.Events.Count > 0);
        }

        var purged = catalog with
        {
            Tilesheets = catalog.Tilesheets.Where(pair => pair.Key != sheetId).ToDictionary(),
            Tiles = catalog.Tiles.Where(pair => !tileIds.Contains(pair.Key)).ToDictionary(),
            Objects = catalog.Objects.Where(pair => !objectIds.Contains(pair.Key)).ToDictionary(),
            Maps = maps,
        };
        return new SheetPurgeResult(purged, [.. changed.Order(StringComparer.Ordinal)], cells, objectIds.Count, placements, withEvents);
    }

    // Any tile of the sheet takes the whole object: it couldn't be drawn whole without it.
    private static HashSet<string> ObjectsUsing(ContentCatalog catalog, string sheetId) =>
        catalog.Objects.Values
            .Where(definition => definition.Tiles.Any(tile => catalog.Tiles.TryGetValue(tile.TileId, out var used) && used.SheetId == sheetId))
            .Select(definition => definition.Id)
            .ToHashSet(StringComparer.Ordinal);

    private static IEnumerable<string> ObjectsNamedBy(MapEvent mapEvent) =>
        mapEvent.Steps.OfType<SwapObjectStep>().Select(swap => swap.ObjectId)
            .Concat(mapEvent.OnlyWhenObjectId is { } gate ? [gate] : []);

    private static MapLayout Without(MapLayout layout, HashSet<int> tileIds, HashSet<string> objectIds) => layout with
    {
        Layers = [.. layout.Layers.Select(layer => layer with
        {
            Rows = [.. layer.Rows.Select(row => (IReadOnlyList<int>)[.. row.Select(id => tileIds.Contains(id) ? TileLayer.EmptyTileId : id)])],
        })],
        Objects = [.. layout.Objects.Where(placed => !objectIds.Contains(placed.ObjectId))],
    };
}
