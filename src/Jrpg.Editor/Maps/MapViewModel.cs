using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>The open map: what the editor shows, and from slice 11d on what it edits.</summary>
internal sealed class MapViewModel(MapDefinition map)
{
    public MapDefinition Map { get; } = map;

    /// <summary>Bottom first, the order they're drawn in.</summary>
    public IReadOnlyList<TileLayer> Layers { get; } = [.. map.Layout.Layers.OrderBy(layer => layer.Z)];

    /// <summary>The tile seen at a cell: the one on the highest layer that has a tile there; null outside the map or on an empty cell.</summary>
    public TileAtCell? TopTileAt(GridCell cell)
    {
        if (!Map.Layout.Contains(cell))
        {
            return null;
        }

        for (var index = Layers.Count - 1; index >= 0; index--)
        {
            var tileId = Layers[index].TileIdAt(cell);
            if (tileId != TileLayer.EmptyTileId)
            {
                return new TileAtCell(tileId, Layers[index].Z);
            }
        }

        return null;
    }
}
