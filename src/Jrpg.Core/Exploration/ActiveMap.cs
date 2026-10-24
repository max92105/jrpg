using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.World;

namespace Jrpg.Core.Exploration;

/// <summary>The map the player is on, as the player changed it: swapped objects and events already done.</summary>
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

    public void MarkDone(MapEvent mapEvent) => world.MarkEventDone(KeyOf(mapEvent));

    private MapEventKey KeyOf(MapEvent mapEvent) => new(Definition.Id, mapEvent.Id);
}
