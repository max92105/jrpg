using Jrpg.Core.Geometry;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Events;

/// <summary>
/// Editing the events on the open map: an object's events, or a cell's when no object is clicked; which one a click
/// picks, and applying the dialogs. Each change is one undo step.
/// </summary>
internal sealed class EventEditingViewModel(CatalogDraft catalog)
{
    /// <summary>The map whose events are edited; set by the shell when a map opens.</summary>
    public MapViewModel? OpenMap { get; set; }

    /// <summary>The maps a warp can lead to; set by the shell, which adds a new map once it is saved.</summary>
    public IReadOnlyList<string> MapIds { get; set; } = [];

    /// <summary>The event dialog's state for the object drawn on top at a point; null when no object is there.</summary>
    /// <param name="point">In map cells.</param>
    public EventListViewModel? StartEditingAt(Point2D point) =>
        OpenMap is { } open && PlacedObjectHits.TopAt(open.Map.Layout, point, catalog.Current.Objects) is { } placed
            ? new EventListViewModel(placed, Choices(), EventPresets.GuessOpenedObject(placed.ObjectId, catalog.Current.Objects))
            : null;

    /// <summary>The cell event dialog's state for a cell, with its event if it has one; null off the map.</summary>
    public CellEventViewModel? StartEditingCellAt(GridCell cell) =>
        OpenMap is { } open && open.Map.Layout.Contains(cell)
            ? new CellEventViewModel(cell, open.Map.Layout.CellEvents.FirstOrDefault(cellEvent => cellEvent.Cell == cell), Choices())
            : null;

    /// <summary>Gives the object the dialog's events; nothing when they didn't change.</summary>
    public void Apply(EventListViewModel edited)
    {
        if (edited.ToEdit() is { } edit)
        {
            OpenMap?.Do(edit);
        }
    }

    /// <summary>Gives the cell the dialog's event; nothing when it didn't change.</summary>
    public void Apply(CellEventViewModel edited)
    {
        if (OpenMap is { } open && edited.ToEdit(open.Map.Layout.CellEvents) is { } edit)
        {
            open.Do(edit);
        }
    }

    /// <summary>Removes a cell's event; nothing when it has none.</summary>
    public void RemoveCellEventAt(GridCell cell)
    {
        if (OpenMap is { } open && open.Map.Layout.CellEvents.Any(cellEvent => cellEvent.Cell == cell))
        {
            open.Do(SetCellEventEdit.For(open.Map.Layout.CellEvents, cell, null));
        }
    }

    // Read when a dialog opens, so objects made since the editor started can be chosen.
    private StepChoices Choices() => new(
        [.. catalog.Current.Items.Keys.Order(StringComparer.Ordinal)],
        [.. catalog.Current.Objects.Keys.Order(StringComparer.Ordinal)],
        MapIds);
}
