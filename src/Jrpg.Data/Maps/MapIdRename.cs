using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;

namespace Jrpg.Data.Maps;

/// <summary>
/// The content with one map's id and every reference to it renamed (decision 530, ADR 0042): the map's layout and
/// settings ids, each warp step leading to it on every map (its own included) and the new game's start. Steps are
/// rebuilt with <c>with</c>; nothing else in a layout moves.
/// </summary>
internal static class MapIdRename
{
    /// <exception cref="ArgumentException">
    /// The old id is unknown or the new one already used (the editor checks first; this is the backstop).
    /// </exception>
    public static RenamedContent Apply(ContentCatalog content, string oldId, string newId)
    {
        if (!content.Maps.ContainsKey(oldId))
        {
            throw new ArgumentException($"No map '{oldId}' to rename.", nameof(oldId));
        }

        // Compared without case: ids are file names, and Windows file names ignore case.
        if (content.Maps.Keys.Contains(newId, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Map '{newId}' already exists.", nameof(newId));
        }

        var maps = new Dictionary<string, MapDefinition>();
        var changedMapIds = new List<string>();
        foreach (var map in content.Maps.Values)
        {
            var layout = WithWarpsRenamed(map.Layout, oldId, newId, out var isChanged);
            if (map.Id == oldId)
            {
                maps[newId] = new MapDefinition(layout with { Id = newId }, map.Settings with { Id = newId });
                continue;
            }

            maps[map.Id] = isChanged ? new MapDefinition(layout, map.Settings) : map;
            if (isChanged)
            {
                changedMapIds.Add(map.Id);
            }
        }

        var newGame = content.NewGame;
        var startsThere = newGame.StartLocation.MapId == oldId;
        if (startsThere)
        {
            newGame = newGame with { StartLocation = newGame.StartLocation with { MapId = newId } };
        }

        return new RenamedContent(content with { Maps = maps, NewGame = newGame }, changedMapIds, startsThere);
    }

    private static MapLayout WithWarpsRenamed(MapLayout layout, string oldId, string newId, out bool isChanged)
    {
        var isAnyWarpRenamed = false;
        MapEvent Renamed(MapEvent mapEvent)
        {
            if (!mapEvent.Steps.Any(step => step is WarpStep warp && warp.MapId == oldId))
            {
                return mapEvent;
            }

            isAnyWarpRenamed = true;
            return mapEvent with { Steps = [.. mapEvent.Steps.Select(step => step is WarpStep warp && warp.MapId == oldId ? warp with { MapId = newId } : step)] };
        }

        IReadOnlyList<CellEvent> cellEvents = [.. layout.CellEvents.Select(cellEvent => cellEvent with { Event = Renamed(cellEvent.Event) })];
        IReadOnlyList<PlacedObject> objects = [.. layout.Objects.Select(placed => placed with { Events = [.. placed.Events.Select(Renamed)] })];
        isChanged = isAnyWarpRenamed;
        return isChanged ? layout with { CellEvents = cellEvents, Objects = objects } : layout;
    }
}
