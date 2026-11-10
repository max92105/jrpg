using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The map's parts in Core's <see cref="MapDrawOrder"/>, the player included, rebuilt every update into the same
/// storage: nothing is allocated once it has grown.
/// </summary>
internal sealed class MapDrawList
{
    private readonly List<MapDrawItem> _items = [];

    public IReadOnlyList<MapDrawItem> Items => _items;

    /// <param name="layout">The map whose layers and placed objects are drawn.</param>
    /// <param name="currentDefinitionOf">What each placed object is now, after any swap, for its height and canopy.</param>
    /// <param name="playerPosition">The player's position, in cells.</param>
    public void Rebuild(MapLayout layout, Func<PlacedObject, ObjectDefinition> currentDefinitionOf, Point2D playerPosition) =>
        MapDrawOrder.FillWithPlayer(_items, layout, currentDefinitionOf, playerPosition);
}
