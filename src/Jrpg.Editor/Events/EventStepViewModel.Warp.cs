using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Events;

/// <summary>A warp step's destination: picked on the map's picture (decision 506), and followed by Go to.</summary>
internal sealed partial class EventStepViewModel
{
    /// <summary>Where the step leads; null when it isn't a warp or its fields aren't valid.</summary>
    public WarpDestination? Destination =>
        Kind == EventStepKind.Warp && Problem is null && TypedCell() is { } cell ? new WarpDestination(MapId, cell) : null;

    /// <summary>The picker, on the step's map, and on its cell when both numbers are valid.</summary>
    public WarpDestinationViewModel StartPickingDestination(WarpMaps maps) => new(maps, MapId, TypedCell());

    /// <summary>Puts the picked map and cell in the step's boxes.</summary>
    /// <exception cref="InvalidOperationException">The picker has a problem: no map or no cell chosen.</exception>
    public void TakeDestination(WarpDestinationViewModel picked)
    {
        if (picked is not { MapId: { } mapId, Cell: { } cell })
        {
            throw new InvalidOperationException(picked.Problem);
        }

        MapId = mapId;
        CellXText = Text(cell.X);
        CellYText = Text(cell.Y);
    }

    private GridCell? TypedCell() =>
        WholeNumber(CellXText, minimum: 0) is { } x && WholeNumber(CellYText, minimum: 0) is { } y ? new GridCell(x, y) : null;
}
