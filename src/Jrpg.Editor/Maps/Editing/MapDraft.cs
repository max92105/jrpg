using Jrpg.Core.Events;
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
    private readonly SortedDictionary<int, int[][]> _layerRowsByZ = [];
    private readonly List<PlacedObject> _objects = [];
    private MapLayout _source;

    private MapDraft(MapDefinition source)
    {
        _source = source.Layout;
        Settings = source.Settings;
        ReplaceLayout(source.Layout);
    }

    public MapSettings Settings { get; set; }

    /// <summary>In placement order, which is also their draw order on the same Z and base.</summary>
    public IReadOnlyList<PlacedObject> Objects => _objects;

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

    public void InsertObject(int index, PlacedObject placed) => _objects.Insert(index, placed);

    public void RemoveObjectAt(int index) => _objects.RemoveAt(index);

    public void MoveObject(string uid, Point2D position)
    {
        var index = _objects.FindIndex(placed => placed.Uid == uid);
        _objects[index] = _objects[index] with { Position = position };
    }

    public void SetObjectEvents(string uid, IReadOnlyList<MapEvent> events)
    {
        var index = _objects.FindIndex(placed => placed.Uid == uid);
        _objects[index] = _objects[index] with { Events = events };
    }

    /// <summary>Replaces everything on the map at once: its size, tiles, objects and cell events, as a resize does.</summary>
    public void ReplaceLayout(MapLayout layout)
    {
        _source = layout;
        _layerRowsByZ.Clear();
        foreach (var layer in layout.Layers)
        {
            _layerRowsByZ.Add(layer.Z, [.. layer.Rows.Select(row => row.ToArray())]);
        }

        _objects.Clear();
        _objects.AddRange(layout.Objects);
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
        return new MapDefinition(_source with { Layers = [.. layers], Objects = [.. _objects] }, Settings);
    }
}
