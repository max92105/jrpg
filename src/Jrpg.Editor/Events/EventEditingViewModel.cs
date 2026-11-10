using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Events;

/// <summary>
/// Editing the events on the open map: an object's events, or a cell's when no object is clicked; which one a click
/// picks (the locked layers' objects never, decision 525), and applying the dialogs. Each change is one undo step.
/// </summary>
/// <param name="catalog">The objects the clicks hit, and the choices the dialogs offer.</param>
/// <param name="openMaps">Every open map, for the warp picker.</param>
/// <param name="layer">Whose lock decides which objects a click reaches.</param>
internal sealed class EventEditingViewModel(CatalogDraft catalog, OpenMaps openMaps, LayerChoiceViewModel layer)
{
    /// <summary>The map whose events are edited; set by the shell when a map opens.</summary>
    public MapViewModel? OpenMap { get; set; }

    /// <summary>The maps a warp can lead to; set by the shell, which adds a new map once it is saved.</summary>
    public IReadOnlyList<string> MapIds { get; set; } = [];

    /// <summary>
    /// The event dialog's state for the object drawn on top at a point, among those a click reaches; null when no
    /// object is reached there.
    /// </summary>
    /// <param name="point">In map cells.</param>
    public EventListViewModel? StartEditingAt(Point2D point) =>
        OpenMap is { } open
            && PlacedObjectHits.AllAt(open.Map.Layout, point, catalog.Current.Objects).FirstOrDefault(placed => layer.Accepts(placed.Z)) is { } placed
            ? EventListFor(placed)
            : null;

    /// <summary>
    /// The Placed object panel's event dialog: the placed object with this uid; null when it isn't on the map.
    /// </summary>
    public EventListViewModel? StartEditing(string uid) =>
        OpenMap?.Map.Layout.Objects.FirstOrDefault(placed => placed.Uid == uid) is { } found ? EventListFor(found) : null;

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

    private EventListViewModel EventListFor(PlacedObject placed) =>
        new(placed, Choices(), EventPresets.GuessOpenedObject(placed.ObjectId, catalog.Current.Objects));

    // Read when a dialog opens, so objects made since the editor started can be chosen.
    private StepChoices Choices() => new(
        [.. catalog.Current.Items.Keys.Order(StringComparer.Ordinal)],
        [.. catalog.Current.Objects.Keys.Order(StringComparer.Ordinal)],
        MapIds,
        [.. catalog.Current.PartyMembers.Keys.Order(StringComparer.Ordinal)])
    {
        ShopIds = [.. catalog.Current.Shops.Keys.Order(StringComparer.Ordinal)],
        QuestIds = [.. catalog.Current.Quests.Keys.Order(StringComparer.Ordinal)],
        QuestStages = QuestStageChoices(),
        WarpMaps = new(openMaps.EntriesWith(catalog.Current), openMaps.LayoutsWith(catalog.Current), catalog.Current),
    };

    // Quests in id order, each quest's stages in its own order.
    private IReadOnlyList<QuestStageChoice> QuestStageChoices() =>
        [.. catalog.Current.Quests.Values
            .OrderBy(quest => quest.Id, StringComparer.Ordinal)
            .SelectMany(quest => quest.Stages.Select(stage => new QuestStageChoice(quest.Id, stage.Id)))];
}
