using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// The map being edited: a mutable copy of a <see cref="MapDefinition"/>, changed only through <see cref="IMapEdit"/>s
/// so every change can be undone (ADR 0012). Layers stay in Z order, bottom first.
/// </summary>
internal sealed class MapDraft
{
    private readonly MapLayout _source;
    private readonly List<int> _layerZs;
    private readonly List<int[][]> _layerRows;
    private readonly MapSettings _settings;

    private MapDraft(MapDefinition source)
    {
        _source = source.Layout;
        var layers = _source.Layers.OrderBy(layer => layer.Z).ToList();
        _layerZs = [.. layers.Select(layer => layer.Z)];
        _layerRows = [.. layers.Select(layer => layer.Rows.Select(row => row.ToArray()).ToArray())];
        _settings = source.Settings;
    }

    public static MapDraft From(MapDefinition map) => new(map);

    public int TileIdAt(int layerIndex, GridCell cell) => _layerRows[layerIndex][cell.Y][cell.X];

    public void SetTileId(int layerIndex, GridCell cell, int tileId) => _layerRows[layerIndex][cell.Y][cell.X] = tileId;

    /// <summary>An immutable map with the draft's current content, to draw or save; later edits don't change it.</summary>
    public MapDefinition ToDefinition()
    {
        var layers = _layerZs.Select((z, index) => new TileLayer
        {
            Z = z,
            Rows = [.. _layerRows[index].Select(row => (IReadOnlyList<int>)[.. row])],
        });
        return new MapDefinition(_source with { Layers = [.. layers] }, _settings);
    }
}
