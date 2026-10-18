using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>The open map: what the editor shows, and from slice 11d on what it edits.</summary>
internal sealed class MapViewModel(MapDefinition map)
{
    public MapDefinition Map { get; } = map;

    /// <summary>Bottom first, the order they're drawn in.</summary>
    public IReadOnlyList<TileLayer> Layers { get; } = [.. map.Layout.Layers.OrderBy(layer => layer.Z)];
}
