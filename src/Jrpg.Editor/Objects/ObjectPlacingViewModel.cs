using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Placing, moving and removing objects on the open map, the selection in its Selection partial (selecting, nudging,
/// deleting), with the selection's copy and paste (<see cref="Copying"/>) and the placed object panel
/// (<see cref="Editing"/>). Every change is one undo step; nothing is placed until an object is chosen.
/// </summary>
internal sealed partial class ObjectPlacingViewModel : ObservableObject
{
    /// <summary>An arrow key's nudge of the selection, in map cells (decision 509; tunable).</summary>
    public const double NudgeCells = 1;

    /// <summary>Shift + arrow's nudge: one art pixel of a 16-pixel cell, exact in binary (decision 509).</summary>
    public const double FineNudgeCells = 1.0 / 16;

    private readonly CatalogDraft _catalog;
    private readonly LayerChoiceViewModel _layer;

    public ObjectPlacingViewModel(CatalogDraft catalog, LayerChoiceViewModel layer, OpenMaps openMaps)
    {
        Browser = new ObjectBrowserViewModel(catalog, () => openMaps.Layouts);
        Browser.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ObjectBrowserViewModel.SelectedObject))
            {
                OnPropertyChanged(nameof(CanEditShapes));
            }
        };
        _catalog = catalog;
        _layer = layer;
        Copying = new ObjectCopying(catalog, this);
        Editing = new PlacedObjectEditing(catalog, this);
        Picking = new ObjectPicking(this, layer);
    }

    public ObjectBrowserViewModel Browser { get; }

    /// <summary>Copy, Cut and Paste of the selected objects (decision 512).</summary>
    public ObjectCopying Copying { get; }

    /// <summary>Ctrl + click's pick, cycling down through the objects under the point (decision 523).</summary>
    public ObjectPicking Picking { get; }

    /// <summary>
    /// The placed object panel's fields: the one selected object's position and layer (decision 513).
    /// </summary>
    public PlacedObjectEditing Editing { get; }

    /// <summary>Whether Objects > Edit shapes has an object to open: one selected on the map, or the one chosen in the panel.</summary>
    public bool CanEditShapes => SelectedObjectUids.Count == 1 || Browser.SelectedObject is not null;

    /// <summary>
    /// For Objects > Edit shapes: the shapes of the one placed object selected on the map, else of the object chosen in the
    /// panel; null when there is neither.
    /// </summary>
    public ObjectShapesViewModel? StartEditingShapes()
    {
        var selected = SelectedObjectUids.Count == 1 ? PlacedObjectWithUid(SelectedObjectUids[0]) : null;
        return selected is not null ? new ObjectShapesViewModel(_catalog.Current.Objects[selected.ObjectId]) : StartEditingChosenShapes();
    }

    /// <summary>The shapes of the object chosen in the panel (a double-click there); null when none is chosen.</summary>
    public ObjectShapesViewModel? StartEditingChosenShapes() =>
        Browser.SelectedObject is { } definition ? new ObjectShapesViewModel(definition) : null;

    /// <summary>The shapes of the placed object on top at a map point (Shift + double-click); null when none is there.</summary>
    /// <param name="point">In map cells.</param>
    public ObjectShapesViewModel? StartEditingShapesAt(Point2D point) =>
        ObjectAt(point) is { } placed ? new ObjectShapesViewModel(_catalog.Current.Objects[placed.ObjectId]) : null;

    /// <summary>Puts the edited object in the catalog draft, as one undo step (nothing when unchanged).</summary>
    /// <exception cref="InvalidOperationException">The edited shapes have a problem (<see cref="ObjectShapesViewModel.Problem"/>).</exception>
    public void ApplyShapes(ObjectShapesViewModel edited) => _catalog.SetObject(edited.ToDefinition());

    /// <summary>
    /// Whether placed and moved objects land on whole cells; Alt does the opposite (decision 511). Off at the start.
    /// </summary>
    [ObservableProperty]
    public partial bool IsGridSnapOn { get; set; }

    /// <summary>The map objects are placed on; set by the shell when a map opens, which empties the selection.</summary>
    [ObservableProperty]
    public partial MapViewModel? OpenMap { get; set; }

    /// <summary>
    /// Places the chosen object centered on a point, on the chosen layer. A point off the map places nothing, since the
    /// map is drawn clipped and the object couldn't be seen.
    /// </summary>
    /// <param name="center">In map cells; positions aren't snapped to the grid.</param>
    public void PlaceObjectAt(Point2D center) => PlaceChosen(center, topLeft => topLeft);

    /// <summary>
    /// As <see cref="PlaceObjectAt"/>, the top-left landing on the nearest whole cell (decision 511).
    /// </summary>
    /// <param name="center">In map cells; the point off the map places nothing, as for a free placing.</param>
    public void PlaceObjectOnGridAt(Point2D center) => PlaceChosen(center, GridSnap.TopLeft);

    /// <summary>G, or the tool bar's Snap to grid box.</summary>
    public void ToggleGridSnap() => IsGridSnapOn = !IsGridSnapOn;

    /// <summary>
    /// The objects at a point that a click reaches (the locked layers' left out, decision 525), the one drawn on top
    /// first; empty with no map.
    /// </summary>
    /// <param name="point">In map cells.</param>
    public IReadOnlyList<PlacedObject> ObjectsAt(Point2D point) => [.. AllObjectsAt(point).Where(placed => _layer.Accepts(placed.Z))];

    /// <summary>Whether the lock is what leaves a click with nothing here: objects, all on locked layers.</summary>
    /// <param name="point">In map cells.</param>
    public bool IsLockHidingObjectsAt(Point2D point) =>
        AllObjectsAt(point) is { Count: > 0 } all && !all.Any(placed => _layer.Accepts(placed.Z));

    /// <summary>The object drawn on top at a point, to remove or open; null when none is there.</summary>
    /// <param name="point">In map cells.</param>
    public PlacedObject? ObjectAt(Point2D point) => ObjectsAt(point) is [var top, ..] ? top : null;

    /// <summary>
    /// The object Shift + drag takes at a point (decision 524): a selected one under it before the one drawn on top, so
    /// an object reached by the pick's cycle can be moved; null when none is there.
    /// </summary>
    /// <param name="point">In map cells.</param>
    public PlacedObject? GrabbedObjectAt(Point2D point)
    {
        var hits = ObjectsAt(point);
        return hits.FirstOrDefault(hit => SelectedObjectUids.Contains(hit.Uid)) ?? (hits is [var top, ..] ? top : null);
    }

    /// <param name="uid">The placed object to move.</param>
    /// <param name="to">Its new top-left corner, in map cells.</param>
    public void MoveObject(string uid, Point2D to)
    {
        var placed = PlacedObjectWithUid(uid);
        if (placed is not null && placed.Position != to)
        {
            OpenMap!.Do(new MoveObjectEdit(uid, placed.Position, to));
        }
    }

    /// <summary>As <see cref="MoveObject"/>, the top-left landing on the nearest whole cell (decision 511).</summary>
    public void MoveObjectOnGrid(string uid, Point2D to) => MoveObject(uid, GridSnap.TopLeft(to));

    public void RemoveObject(string uid) => RemoveObjects([uid]);

    partial void OnOpenMapChanged(MapViewModel? oldValue, MapViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnOpenMapContentChanged;
        newValue?.PropertyChanged += OnOpenMapContentChanged;
        SelectedObjectUids = [];
    }

    // An undo can remove a selected object, and its uid can be given to the next object placed, which mustn't start out selected.
    private void OnOpenMapContentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MapViewModel.Map) || OpenMap is not { } open)
        {
            return;
        }

        Picking.Forget();
        var onMap = open.Map.Layout.Objects.Select(placed => placed.Uid).ToHashSet();
        if (SelectedObjectUids.Any(uid => !onMap.Contains(uid)))
        {
            SelectedObjectUids = [.. SelectedObjectUids.Where(onMap.Contains)];
        }
    }

    // The chosen object centered on a point of the open map, its top-left where land puts it; nothing with nothing
    // chosen, no map, or the point off the map (drawn clipped, the object couldn't be seen).
    private void PlaceChosen(Point2D center, Func<Point2D, Point2D> land)
    {
        if (OpenMap is not { } open || Browser.SelectedObject is not { } definition || !open.Map.Layout.Contains(GridCell.Containing(center)))
        {
            return;
        }

        var takenUids = open.Map.Layout.Objects.Select(placed => placed.Uid).ToHashSet();
        open.Do(new PlaceObjectEdit(new PlacedObject
        {
            Uid = PlacedObjectUids.Next(definition.Id, takenUids),
            ObjectId = definition.Id,
            Position = land(new Point2D(center.X - (definition.Width / 2.0), center.Y - (definition.Height / 2.0))),
            Z = _layer.Z,
        }));
    }

    private IReadOnlyList<PlacedObject> AllObjectsAt(Point2D point) =>
        OpenMap is { } open ? PlacedObjectHits.AllAt(open.Map.Layout, point, _catalog.Current.Objects) : [];

    private PlacedObject? PlacedObjectWithUid(string uid) => OpenMap?.Map.Layout.Objects.FirstOrDefault(placed => placed.Uid == uid);

    private void RemoveObjects(IReadOnlyCollection<string> uids)
    {
        if (OpenMap is not { } open)
        {
            return;
        }

        var edit = new RemoveObjectsEdit(open.Map.Layout.Objects, uids);
        if (edit.Count > 0)
        {
            open.Do(edit);
        }
    }
}
