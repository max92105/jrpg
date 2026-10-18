using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Changes a map's size, keeping cell (0, 0) where it is, as the legacy editor did. Tiles and cell events on cells
/// outside the new size are dropped, and objects that no longer overlap the map at all; an object reaching past an
/// edge stays, as some legacy objects do. Undo puts back the map exactly as it was.
/// </summary>
internal sealed class ResizeMapEdit : IMapEdit
{
    private readonly MapLayout _before;
    private readonly MapLayout _after;

    /// <param name="before">The map as it is.</param>
    /// <param name="width">The new width in cells, at least 1.</param>
    /// <param name="height">The new height in cells, at least 1.</param>
    /// <param name="definitions">The object definitions, for the objects' sizes.</param>
    public ResizeMapEdit(MapLayout before, int width, int height, IReadOnlyDictionary<string, ObjectDefinition> definitions)
    {
        _before = before;
        var resized = before with { Width = width, Height = height };
        _after = resized with
        {
            Layers = [.. before.Layers.Select(layer => layer with { Rows = ResizedRows(layer, width, height) })],
            Objects = [.. before.Objects.Where(placed => Overlaps(placed, definitions[placed.ObjectId], width, height))],
            CellEvents = [.. before.CellEvents.Where(cellEvent => resized.Contains(cellEvent.Cell))],
        };
        DroppedTileCount = CountTiles(before) - CountTiles(_after);
        DroppedObjectCount = before.Objects.Count - _after.Objects.Count;
        DroppedCellEventCount = before.CellEvents.Count - _after.CellEvents.Count;
    }

    public int DroppedTileCount { get; }

    public int DroppedObjectCount { get; }

    public int DroppedCellEventCount { get; }

    public void Apply(MapDraft draft) => draft.ReplaceLayout(_after);

    public void Revert(MapDraft draft) => draft.ReplaceLayout(_before);

    // Cells kept keep their tile; new cells are empty.
    private static IReadOnlyList<IReadOnlyList<int>> ResizedRows(TileLayer layer, int width, int height) =>
        [.. Enumerable.Range(0, height).Select(y => (IReadOnlyList<int>)[.. Enumerable.Range(0, width).Select(x =>
            y < layer.Rows.Count && x < layer.Rows[y].Count ? layer.TileIdAt(new GridCell(x, y)) : TileLayer.EmptyTileId)])];

    private static bool Overlaps(PlacedObject placed, ObjectDefinition definition, int width, int height) =>
        placed.Position.X < width && placed.Position.X + definition.Width > 0
        && placed.Position.Y < height && placed.Position.Y + definition.Height > 0;

    private static int CountTiles(MapLayout layout) =>
        layout.Layers.Sum(layer => layer.Rows.Sum(row => row.Count(tileId => tileId != TileLayer.EmptyTileId)));
}
