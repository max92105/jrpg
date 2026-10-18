namespace Jrpg.Core.World;

/// <summary>What the player changed on the maps: events already done and objects swapped. Kept across map changes.</summary>
internal sealed class WorldState
{
    private readonly HashSet<MapEventKey> _completedEvents = [];
    private readonly Dictionary<(string MapId, string ObjectUid), string> _swappedObjectIds = [];

    public bool IsEventDone(MapEventKey key) => _completedEvents.Contains(key);

    /// <returns>The object the placed object was swapped to, or null if it was never swapped.</returns>
    public string? SwappedObjectId(string mapId, string objectUid) =>
        _swappedObjectIds.GetValueOrDefault((mapId, objectUid));

    public void MarkEventDone(MapEventKey key) => _completedEvents.Add(key);

    public void SwapObject(ObjectSwap swap) => _swappedObjectIds[(swap.MapId, swap.ObjectUid)] = swap.ObjectId;
}
