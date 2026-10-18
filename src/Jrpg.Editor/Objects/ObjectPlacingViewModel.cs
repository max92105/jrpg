using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Placing, moving and removing objects on the open map, as in the legacy editor. Every change is one undo step;
/// nothing is placed until an object is chosen.
/// </summary>
internal sealed partial class ObjectPlacingViewModel : ObservableObject
{
    private readonly IReadOnlyDictionary<string, ObjectDefinition> _definitions;
    private readonly LayerChoiceViewModel _layer;

    public ObjectPlacingViewModel(IReadOnlyDictionary<string, ObjectDefinition> definitions, LayerChoiceViewModel layer)
    {
        Browser = new ObjectBrowserViewModel(definitions);
        _definitions = definitions;
        _layer = layer;
    }

    public ObjectBrowserViewModel Browser { get; }

    /// <summary>The map objects are placed on; set by the shell when a map opens, which empties the selection.</summary>
    [ObservableProperty]
    public partial MapViewModel? OpenMap { get; set; }

    /// <summary>The placed objects chosen with a dragged rectangle, for Delete; kept to objects still on the map.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> SelectedObjectUids { get; private set; } = [];

    /// <summary>
    /// Places the chosen object centered on a point, on the chosen layer, as in the legacy editor. A point off the map
    /// places nothing, since the map is drawn clipped and the object couldn't be seen.
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
        OpenMap is { } open ? PlacedObjectHits.TopAt(open.Map.Layout, point, _definitions) : null;

    /// <param name="uid">The placed object to move.</param>
    /// <param name="to">Its new top-left corner, in map cells.</param>
    public void MoveObject(string uid, Point2D to)
    {
        var placed = OpenMap?.Map.Layout.Objects.FirstOrDefault(candidate => candidate.Uid == uid);
        if (placed is not null && placed.Position != to)
        {
            OpenMap!.Do(new MoveObjectEdit(uid, placed.Position, to));
        }
    }

    public void RemoveObject(string uid) => RemoveObjects([uid]);

    /// <summary>Selects the objects overlapping the rectangle between two corners, in map cells.</summary>
    public void SelectObjectsIn(Point2D corner, Point2D oppositeCorner) =>
        SelectedObjectUids = OpenMap is { } open
            ? [.. PlacedObjectHits.InRectangle(open.Map.Layout, corner, oppositeCorner, _definitions).Select(placed => placed.Uid)]
            : [];

    /// <summary>Removes the selected objects as one undo step.</summary>
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
