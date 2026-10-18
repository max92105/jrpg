namespace Jrpg.Core.World;

/// <summary>What the player changed on the maps: events already done and objects swapped. Kept across map changes.</summary>
internal sealed class WorldState
{
    private readonly HashSet<MapEventKey> _completedEvents = [];
    private readonly Dictionary<(string MapId, string ObjectUid), string> _swappedObjectIds = [];

    public WorldState()
    {
    }

    /// <summary>Restores the changes a save kept.</summary>
    public WorldState(IEnumerable<MapEventKey> completedEvents, IEnumerable<ObjectSwap> objectSwaps)
    {
        _completedEvents = [.. completedEvents];
        foreach (var swap in objectSwaps)
        {
            SwapObject(swap);
        }
    }

    /// <summary>In a fixed order, so the same world always saves the same way.</summary>
    public IReadOnlyList<MapEventKey> CompletedEvents =>
        [.. _completedEvents.OrderBy(key => key.MapId, StringComparer.Ordinal).ThenBy(key => key.EventId, StringComparer.Ordinal)];

    /// <summary>In a fixed order, so the same world always saves the same way.</summary>
    public IReadOnlyList<ObjectSwap> ObjectSwaps =>
        [.. _swappedObjectIds
            .Select(entry => new ObjectSwap(entry.Key.MapId, entry.Key.ObjectUid, entry.Value))
            .OrderBy(swap => swap.MapId, StringComparer.Ordinal)
            .ThenBy(swap => swap.ObjectUid, StringComparer.Ordinal)];

    public bool IsEventDone(MapEventKey key) => _completedEvents.Contains(key);

    /// <returns>The object the placed object was swapped to, or null if it was never swapped.</returns>
    public string? SwappedObjectId(string mapId, string objectUid) =>
        _swappedObjectIds.GetValueOrDefault((mapId, objectUid));

    public void MarkEventDone(MapEventKey key) => _completedEvents.Add(key);

    public void SwapObject(ObjectSwap swap) => _swappedObjectIds[(swap.MapId, swap.ObjectUid)] = swap.ObjectId;
}
