using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Events;

/// <summary>
/// The warp step's destination picker (decision 506): the map chosen in a list and the cell clicked on its picture, a
/// warning on a cell the player can't stand on or that starts an event (decision 507), and what stops OK.
/// </summary>
internal sealed partial class WarpDestinationViewModel : ObservableObject
{
    private readonly WarpMaps _maps;

    /// <param name="maps">The maps a warp can lead to.</param>
    /// <param name="mapId">The step's map; one not listed starts with no map.</param>
    /// <param name="cell">The step's cell; one outside the map starts with no cell.</param>
    public WarpDestinationViewModel(WarpMaps maps, string mapId, GridCell? cell)
    {
        _maps = maps;
        MapId = maps.Layouts.ContainsKey(mapId) ? mapId : null;
        if (cell is { } at)
        {
            ChooseCell(at);
        }
    }

    public IReadOnlyList<MapEntry> Maps => _maps.Entries;

    /// <summary>The chosen map; another map keeps the cell when it fits there.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Layout), nameof(Warning), nameof(Problem))]
    public partial string? MapId { get; set; }

    public MapLayout? Layout => MapId is { } id ? _maps.Layouts.GetValueOrDefault(id) : null;

    /// <summary>The arrival cell, inside the chosen map; null until one is clicked.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellText), nameof(Warning), nameof(Problem))]
    public partial GridCell? Cell { get; private set; }

    public string CellText => Cell is { } at ? $"Cell ({at.X}, {at.Y})" : "Click a cell on the map";

    /// <summary>Why the cell may be wrong; null with no cell or a plain place to stand.</summary>
    public string? Warning => Layout is { } layout && Cell is { } at ? WarpArrivalWarning.Of(layout, _maps.Catalog, at) : null;

    /// <summary>What stops OK: no map or no cell chosen.</summary>
    public string? Problem =>
        MapId is null ? "Choose the map to warp to."
        : Cell is null ? "Click the cell to arrive on."
        : null;

    /// <summary>A click on the map's picture; a cell outside the map is ignored.</summary>
    public void ChooseCell(GridCell cell)
    {
        if (Layout?.Contains(cell) == true)
        {
            Cell = cell;
        }
    }

    partial void OnMapIdChanged(string? value)
    {
        if (Cell is { } at && Layout?.Contains(at) != true)
        {
            Cell = null;
        }
    }
}
