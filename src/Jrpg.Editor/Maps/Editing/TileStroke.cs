using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// One drag of the brush or the eraser. It changes the draft as the mouse moves, so the map shows each cell at once,
/// and becomes a single undo step when the button is released.
/// </summary>
internal sealed class TileStroke(MapDraft draft, TileBrush brush)
{
    private readonly List<TileChange> _changes = [];
    private GridCell? _lastCell;

    /// <summary>Puts the brush down on every cell from the previous one to this one, skipping cells off the map.</summary>
    /// <returns>Whether any tile changed.</returns>
    public bool MoveTo(GridCell cell)
    {
        var changed = false;
        foreach (var lineCell in BrushCells.Line(_lastCell ?? cell, cell))
        {
            foreach (var brushCell in BrushCells.Square(lineCell, brush.Size).Where(draft.Contains))
            {
                changed |= Paint(brushCell);
            }
        }

        _lastCell = cell;
        return changed;
    }

    /// <summary>The stroke as one undo step, already applied to the draft; null when it changed nothing.</summary>
    public SetTilesEdit? ToEdit() => _changes.Count == 0 ? null : new SetTilesEdit(brush.LayerZ, [.. _changes]);

    // A cell the stroke crosses again already holds the brush's tile, so each cell is recorded once, with its original tile.
    private bool Paint(GridCell cell)
    {
        var before = draft.TileIdAt(brush.LayerZ, cell);
        if (before == brush.TileId)
        {
            return false;
        }

        draft.SetTileId(brush.LayerZ, cell, brush.TileId);
        _changes.Add(new TileChange(cell, before, brush.TileId));
        return true;
    }
}
