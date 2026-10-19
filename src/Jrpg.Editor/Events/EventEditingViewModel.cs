using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Events;

/// <summary>Editing the events of objects on the open map: which object a click picks, and applying the event dialog.</summary>
internal sealed class EventEditingViewModel(ContentCatalog catalog)
{
    private readonly IReadOnlyList<string> _itemIds = [.. catalog.Items.Keys.Order(StringComparer.Ordinal)];
    private readonly IReadOnlyList<string> _objectIds = [.. catalog.Objects.Keys.Order(StringComparer.Ordinal)];

    /// <summary>The map whose objects are edited; set by the shell when a map opens.</summary>
    public MapViewModel? OpenMap { get; set; }

    /// <summary>The maps a warp can lead to; set by the shell, which adds a new map once it is saved.</summary>
    public IReadOnlyList<string> MapIds { get; set; } = [];

    /// <summary>The event dialog's state for the object drawn on top at a point; null when no object is there.</summary>
    /// <param name="point">In map cells.</param>
    public EventListViewModel? StartEditingAt(Point2D point) =>
        OpenMap is { } open && PlacedObjectHits.TopAt(open.Map.Layout, point, catalog.Objects) is { } placed
            ? new EventListViewModel(placed, new StepChoices(_itemIds, _objectIds, MapIds), EventPresets.GuessOpenedObject(placed.ObjectId, catalog.Objects))
            : null;

    /// <summary>Gives the object the dialog's events, as one undo step; nothing when they didn't change.</summary>
    public void Apply(EventListViewModel edited)
    {
        if (edited.ToEdit() is { } edit)
        {
            OpenMap?.Do(edit);
        }
    }
}
