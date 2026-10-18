using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// The map being edited: a mutable copy of a <see cref="MapDefinition"/>, changed only through <see cref="IMapEdit"/>s
/// so every change can be undone (ADR 0012). Layers are found by their Z, so painting onto a new Z inserts a layer
/// without moving the others.
/// </summary>
internal sealed class MapDraft
{
    private readonly MapLayout _source;
    private readonly SortedDictionary<int, int[][]> _layerRowsByZ;
    private readonly MapSettings _settings;

    private MapDraft(MapDefinition source)
    {
        _source = source.Layout;
        _layerRowsByZ = new(_source.Layers.ToDictionary(layer => layer.Z, layer => layer.Rows.Select(row => row.ToArray()).ToArray()));
        _settings = source.Settings;
    }

    public static MapDraft From(MapDefinition map) => new(map);

    public bool Contains(GridCell cell) => _source.Contains(cell);

    /// <returns>The tile on the layer at <paramref name="layerZ"/>; empty when the map has no layer there.</returns>
    public int TileIdAt(int layerZ, GridCell cell) =>
        _layerRowsByZ.TryGetValue(layerZ, out var rows) ? rows[cell.Y][cell.X] : TileLayer.EmptyTileId;

    /// <summary>Sets a cell on the layer at <paramref name="layerZ"/>, adding the layer if the map has none there.</summary>
    public void SetTileId(int layerZ, GridCell cell, int tileId)
    {
        if (!_layerRowsByZ.TryGetValue(layerZ, out var rows))
        {
            rows = [.. Enumerable.Range(0, _source.Height).Select(_ => new int[_source.Width])];
            _layerRowsByZ.Add(layerZ, rows);
        }

        rows[cell.Y][cell.X] = tileId;
    }

    /// <summary>
    /// An immutable map with the draft's current content, to draw or save; later edits don't change it. A layer with
    /// no tiles left is omitted, so erasing a layer's last tile removes the layer.
    /// </summary>
    public MapDefinition ToDefinition()
    {
        var layers = _layerRowsByZ
            .Where(layer => layer.Value.Any(row => row.Any(tileId => tileId != TileLayer.EmptyTileId)))
            .Select(layer => new TileLayer { Z = layer.Key, Rows = [.. layer.Value.Select(row => (IReadOnlyList<int>)[.. row])] });
        return new MapDefinition(_source with { Layers = [.. layers] }, _settings);
    }
}
