using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Events;

/// <summary>Editing the events of objects on the open map: which object a click picks, and applying the event dialog.</summary>
internal sealed class EventEditingViewModel(IReadOnlyDictionary<string, ObjectDefinition> definitions)
{
    /// <summary>The map whose objects are edited; set by the shell when a map opens.</summary>
    public MapViewModel? OpenMap { get; set; }

    /// <summary>The event dialog's state for the object drawn on top at a point; null when no object is there.</summary>
    /// <param name="point">In map cells.</param>
    public EventListViewModel? StartEditingAt(Point2D point) =>
        OpenMap is { } open && PlacedObjectHits.TopAt(open.Map.Layout, point, definitions) is { } placed
            ? new EventListViewModel(placed, definitions.Keys)
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
