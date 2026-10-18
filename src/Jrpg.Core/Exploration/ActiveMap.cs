using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>The map the player is on, with what moving around it needs.</summary>
internal sealed class ActiveMap(MapDefinition definition, IReadOnlyDictionary<string, ObjectDefinition> objects, WalkabilityGrid walkability)
{
    public MapDefinition Definition { get; } = definition;

    public WalkabilityGrid Walkability { get; } = walkability;

    public ObjectDefinition CurrentDefinitionOf(PlacedObject placed) => objects[placed.ObjectId];
}
