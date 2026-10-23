using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Placing, moving and removing objects on the open map. Every change is one undo step;
/// nothing is placed until an object is chosen.
/// </summary>
internal sealed partial class ObjectPlacingViewModel : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly LayerChoiceViewModel _layer;

    public ObjectPlacingViewModel(CatalogDraft catalog, LayerChoiceViewModel layer)
    {
        Browser = new ObjectBrowserViewModel(catalog);
        Browser.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ObjectBrowserViewModel.SelectedObject))
            {
                OnPropertyChanged(nameof(CanEditShapes));
            }
        };
        _catalog = catalog;
        _layer = layer;
    }

    public ObjectBrowserViewModel Browser { get; }

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

    /// <summary>The map objects are placed on; set by the shell when a map opens, which empties the selection.</summary>
    [ObservableProperty]
    public partial MapViewModel? OpenMap { get; set; }

    /// <summary>The placed objects chosen with a dragged rectangle, for Delete; kept to objects still on the map.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditShapes))]
    public partial IReadOnlyList<string> SelectedObjectUids { get; private set; } = [];

    /// <summary>
    /// Places the chosen object centered on a point, on the chosen layer. A point off the map places nothing, since the
    /// map is drawn clipped and the object couldn't be seen.
    /// </summary>
    /// <param name="center">In map cells; positions aren't snapped to the grid.</param>
    public void PlaceObjectAt(Point2D center)
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
            Position = new Point2D(center.X - (definition.Width / 2.0), center.Y - (definition.Height / 2.0)),
            Z = _layer.Z,
        }));
    }

    /// <summary>The object drawn on top at a point, to move or remove; null when none is there.</summary>
    /// <param name="point">In map cells.</param>
    public PlacedObject? ObjectAt(Point2D point) =>
        OpenMap is { } open ? PlacedObjectHits.TopAt(open.Map.Layout, point, _catalog.Current.Objects) : null;

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

    public void RemoveObject(string uid) => RemoveObjects([uid]);

    /// <summary>Selects the objects overlapping the rectangle between two corners, in map cells.</summary>
    public void SelectObjectsIn(Point2D corner, Point2D oppositeCorner) =>
        SelectedObjectUids = OpenMap is { } open
            ? [.. PlacedObjectHits.InRectangle(open.Map.Layout, corner, oppositeCorner, _catalog.Current.Objects).Select(placed => placed.Uid)]
            : [];

    /// <summary>Removes the selected objects as one undo step.</summary>
    [RelayCommand]
    public void DeleteSelectedObjects()
    {
        RemoveObjects(SelectedObjectUids);
        SelectedObjectUids = [];
    }

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

        var onMap = open.Map.Layout.Objects.Select(placed => placed.Uid).ToHashSet();
        if (SelectedObjectUids.Any(uid => !onMap.Contains(uid)))
        {
            SelectedObjectUids = [.. SelectedObjectUids.Where(onMap.Contains)];
        }
    }

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
