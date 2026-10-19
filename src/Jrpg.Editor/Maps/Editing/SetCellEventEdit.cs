using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Adds, replaces or removes the event of one cell as one undo step. A cell has at most one event, so a second event
/// on a cell replaces the first, in its place in the list.
/// </summary>
/// <param name="Index">Where the cell's event is, or goes, in the map's cell events.</param>
/// <param name="Before">The cell's event before; null when it had none.</param>
/// <param name="After">The cell's event after; null to remove it.</param>
internal sealed record SetCellEventEdit(int Index, CellEvent? Before, CellEvent? After) : IMapEdit
{
    /// <summary>The edit that gives a cell this event, or removes its event when null.</summary>
    /// <param name="cellEvents">The map's cell events as they are.</param>
    /// <param name="cell">The cell.</param>
    /// <param name="mapEvent">Its new event, or null for none.</param>
    public static SetCellEventEdit For(IReadOnlyList<CellEvent> cellEvents, GridCell cell, MapEvent? mapEvent)
    {
        var index = cellEvents.ToList().FindIndex(cellEvent => cellEvent.Cell == cell);
        var after = mapEvent is null ? null : new CellEvent(cell, mapEvent);
        return index < 0 ? new SetCellEventEdit(cellEvents.Count, null, after) : new SetCellEventEdit(index, cellEvents[index], after);
    }

    public void Apply(MapDraft draft) => Change(draft, Before, After);

    public void Revert(MapDraft draft) => Change(draft, After, Before);

    private void Change(MapDraft draft, CellEvent? from, CellEvent? to)
    {
        switch (from, to)
        {
            case (null, { } added):
                draft.InsertCellEvent(Index, added);
                break;
            case ({ }, null):
                draft.RemoveCellEventAt(Index);
                break;
            case ({ }, { } replacement):
                draft.ReplaceCellEvent(Index, replacement);
                break;
        }
    }
}
