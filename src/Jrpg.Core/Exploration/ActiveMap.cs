using Jrpg.Core.Events;
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

    public bool IsDone(MapEvent mapEvent) => world.IsEventDone(KeyOf(mapEvent));

    public void MarkDone(MapEvent mapEvent) => world.MarkEventDone(KeyOf(mapEvent));

    private MapEventKey KeyOf(MapEvent mapEvent) => new(Definition.Id, mapEvent.Id);
}
