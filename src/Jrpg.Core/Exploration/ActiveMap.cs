using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.World;

namespace Jrpg.Core.Exploration;

/// <summary>
/// The map the player is on, as the player changed it: swapped objects and events already done; the one gate that says
/// whether an event may start now (ADR 0033).
/// </summary>
internal sealed class ActiveMap(
    MapDefinition definition,
    IReadOnlyDictionary<string, ObjectDefinition> objects,
    WalkabilityGrid walkability,
    WorldState world)
{
    public MapDefinition Definition { get; } = definition;

    public WalkabilityGrid Walkability { get; } = walkability;

    public ObjectDefinition CurrentDefinitionOf(PlacedObject placed) =>
        objects[world.SwappedObjectId(Definition.Id, placed.Uid) ?? placed.ObjectId];

    public void SwapObject(PlacedObject placed, string objectId) =>
        world.SwapObject(new ObjectSwap(Definition.Id, placed.Uid, objectId));

    /// <returns>The touch event on that cell, or null. Called only when the player enters a new cell.</returns>
    public CellEvent? CellEventAt(GridCell cell)
    {
        var cellEvents = Definition.Layout.CellEvents;
        for (var index = 0; index < cellEvents.Count; index++)
        {
            if (cellEvents[index].Cell == cell && cellEvents[index].Event.Activation == EventActivation.Touch)
            {
                return cellEvents[index];
            }
        }

        return null;
    }

    public bool IsDone(MapEvent mapEvent) => world.IsEventDone(KeyOf(mapEvent));

    /// <summary>
    /// Whether the event may start now, whatever its trigger (ADR 0033): for this leader (decision 185), not a "once"
    /// event already done, and its quest condition met by the world's log. The object's look is
    /// <see cref="MapEvent.CanRunOn"/>.
    /// </summary>
    public bool CanStart(MapEvent mapEvent, string leaderId) =>
        mapEvent.IsForLeader(leaderId)
        && !(mapEvent.IsOnce && IsDone(mapEvent))
        && (mapEvent.OnlyWhenQuest is not { } gate || gate.IsMetBy(world.Quests));

    public void MarkDone(MapEvent mapEvent) => world.MarkEventDone(KeyOf(mapEvent));

    private MapEventKey KeyOf(MapEvent mapEvent) => new(Definition.Id, mapEvent.Id);
}
