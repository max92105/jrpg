using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The placed objects selected with Ctrl + click or a dragged rectangle (decision 509): selecting them, moving them as
/// one step, deleting them. The open map's changes keep the selection to objects still on it
/// (<c>OnOpenMapContentChanged</c>, beside <see cref="OpenMap"/>).
/// </summary>
internal sealed partial class ObjectPlacingViewModel
{
    /// <summary>
    /// The placed objects chosen with Ctrl + click or a dragged rectangle, for Delete and the arrow keys; kept to
    /// objects still on the map.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditShapes))]
    public partial IReadOnlyList<string> SelectedObjectUids { get; private set; } = [];

    /// <summary>
    /// Selects the objects overlapping the rectangle between two corners, in map cells, the locked layers' left out.
    /// </summary>
    public void SelectObjectsIn(Point2D corner, Point2D oppositeCorner) =>
        SelectedObjectUids = OpenMap is { } open
            ? [.. PlacedObjectHits.InRectangle(open.Map.Layout, corner, oppositeCorner, _catalog.Current.Objects)
                .Where(placed => _layer.Accepts(placed.Z))
                .Select(placed => placed.Uid)]
            : [];

    /// <summary>
    /// Ctrl + click: selects only the object drawn on top at a point, or the next one under the one selected object
    /// when that object is there (<see cref="Picking"/>); none there clears the selection.
    /// </summary>
    /// <param name="point">In map cells.</param>
    public void SelectObjectAt(Point2D point) => Picking.PickAt(point);

    public void ClearSelection() => SelectedObjectUids = [];

    /// <summary>
    /// Selects these objects only (the pasted ones); uids not on the map are dropped at the next change.
    /// </summary>
    public void Select(IReadOnlyList<string> uids) => SelectedObjectUids = uids;

    /// <summary>
    /// Moves the selection so the grabbed object's top-left lands on the nearest whole cell, the others keeping their
    /// offsets from it, as one undo step (decision 511).
    /// </summary>
    /// <param name="grabbedUid">The selected object under the mouse.</param>
    /// <param name="grabbedTo">Its free top-left where the mouse left it, in map cells.</param>
    public void MoveSelectedObjectsOnGrid(string grabbedUid, Point2D grabbedTo)
    {
        if (PlacedObjectWithUid(grabbedUid) is { } grabbed)
        {
            var landed = GridSnap.TopLeft(grabbedTo);
            MoveSelectedObjects(landed.X - grabbed.Position.X, landed.Y - grabbed.Position.Y);
        }
    }

    public bool IsSelected(string uid) => SelectedObjectUids.Contains(uid);

    /// <summary>
    /// Moves every selected object by the same offset as one undo step (an arrow's nudge, a Shift + drag on the
    /// selection); nothing selected, or no offset, records nothing.
    /// </summary>
    /// <param name="cellsRight">The offset right, in map cells; negative moves left.</param>
    /// <param name="cellsDown">The offset down, in map cells; negative moves up.</param>
    public void MoveSelectedObjects(double cellsRight, double cellsDown)
    {
        if (OpenMap is not { } open || (cellsRight == 0 && cellsDown == 0))
        {
            return;
        }

        IReadOnlyList<MoveObjectEdit> moves =
        [
            .. open.Map.Layout.Objects
                .Where(placed => SelectedObjectUids.Contains(placed.Uid))
                .Select(placed => new MoveObjectEdit(
                    placed.Uid, placed.Position, new Point2D(placed.Position.X + cellsRight, placed.Position.Y + cellsDown))),
        ];
        if (moves.Count > 0)
        {
            open.Do(new MoveObjectsEdit(moves));
        }
    }

    /// <summary>Removes the selected objects as one undo step.</summary>
    [RelayCommand]
    public void DeleteSelectedObjects()
    {
        RemoveObjects(SelectedObjectUids);
        SelectedObjectUids = [];
    }
}
