using Jrpg.Core.Events;

namespace Jrpg.Core.Exploration;

/// <summary>
/// Finds the event a map runs when the player arrives on it (ADR 0016): the first arrival event, on objects then on
/// cells, in map order, that the leader and the object's current definition allow and that isn't a "once" event
/// already done.
/// </summary>
internal static class ArrivalEvents
{
    public static TriggeredEvent? FindFirst(ActiveMap map, string leaderId)
    {
        var layout = map.Definition.Layout;
        var onObjects = layout.Objects.SelectMany(placed => placed.Events
            .Where(mapEvent => IsRunnable(mapEvent, map, leaderId) && mapEvent.CanRunOn(map.CurrentDefinitionOf(placed).Id))
            .Select(mapEvent => (TriggeredEvent?)new TriggeredEvent(mapEvent, placed)));
        var onCells = layout.CellEvents
            .Where(cellEvent => IsRunnable(cellEvent.Event, map, leaderId))
            .Select(cellEvent => (TriggeredEvent?)new TriggeredEvent(cellEvent.Event, Source: null));
        return onObjects.Concat(onCells).FirstOrDefault();
    }

    private static bool IsRunnable(MapEvent mapEvent, ActiveMap map, string leaderId) =>
        mapEvent.Activation == EventActivation.Arrive && mapEvent.IsForLeader(leaderId) && !(mapEvent.IsOnce && map.IsDone(mapEvent));
}
