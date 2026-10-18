using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Core.World;

namespace Jrpg.Data.Saving;

/// <summary>Keeps the saved world changes that still name existing maps, events, placed objects and objects.</summary>
internal static class WorldStateRepair
{
    public static IReadOnlyList<MapEventKey> KeepKnownEvents(IReadOnlyList<MapEventKey> completed, ContentCatalog catalog, List<string> changes)
    {
        var kept = new List<MapEventKey>();
        foreach (var key in completed)
        {
            var missing = !catalog.Maps.TryGetValue(key.MapId, out var map) ? "the map"
                : !EventIdsOf(map).Contains(key.EventId) ? "the event"
                : null;
            if (missing is null)
            {
                kept.Add(key);
            }
            else
            {
                changes.Add($"forgot done event '{key.EventId}' on map '{key.MapId}': {missing} no longer exists.");
            }
        }

        return kept;
    }

    public static IReadOnlyList<ObjectSwap> KeepKnownSwaps(IReadOnlyList<ObjectSwap> swaps, ContentCatalog catalog, List<string> changes)
    {
        var kept = new List<ObjectSwap>();
        foreach (var swap in swaps)
        {
            var missing = !catalog.Maps.TryGetValue(swap.MapId, out var map) ? "the map"
                : !map.Layout.Objects.Any(placed => placed.Uid == swap.ObjectUid) ? "the placed object"
                : !catalog.Objects.ContainsKey(swap.ObjectId) ? $"object '{swap.ObjectId}'"
                : null;
            if (missing is null)
            {
                kept.Add(swap);
            }
            else
            {
                changes.Add($"forgot that '{swap.ObjectUid}' on map '{swap.MapId}' became '{swap.ObjectId}': {missing} no longer exists.");
            }
        }

        return kept;
    }

    private static HashSet<string> EventIdsOf(MapDefinition map) =>
        [.. map.Layout.Objects.SelectMany(placed => placed.Events).Concat(map.Layout.CellEvents.Select(cellEvent => cellEvent.Event))
            .Select(mapEvent => mapEvent.Id)];
}
