using System.Windows;
using System.Windows.Controls;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>The object drags on the map: the selection rectangle, the moved objects and the placing ghost.</summary>
internal sealed partial class MainWindow
{
    // The drag under way, if any: a selection rectangle from a point, or an object moved with an offset from the mouse.
    private Point2D? _selectionStart;
    private (PlacedObject Placed, Vector Offset, Point2D From)? _movedObject;
    private ObjectDefinition? _ghostDefinition;

    /// <summary>
    /// Shows where the mouse would put an object: the dragged selection, the moved object, or the chosen one.
    /// </summary>
    /// <param name="point">The mouse in map cells, or null to hide it all.</param>
    private void ShowObjectDragAt(Point2D? point)
    {
        SelectionOutline.Visibility = Visibility.Collapsed;
        ObjectGhost.Visibility = Visibility.Collapsed;
        if (point is not { } at || _viewModel.Mode != EditingMode.Objects || _viewModel.CurrentMap is null)
        {
            return;
        }

        if (_selectionStart is { } start)
        {
            PlaceOnMap(SelectionOutline, start, at);
            SelectionOutline.StrokeThickness = 1 / _viewport.Zoom;
            SelectionOutline.Visibility = Visibility.Visible;
        }
        else if (_movedObject is { } moved)
        {
            ShowGhost(_viewModel.Catalog!.Objects[moved.Placed.ObjectId], new Point2D(at.X + moved.Offset.X, at.Y + moved.Offset.Y));
        }
        else if (Placing.Browser.SelectedObject is { } chosen)
        {
            ShowGhost(chosen, new Point2D(at.X - (chosen.Width / 2.0), at.Y - (chosen.Height / 2.0)));
        }
    }

    private void ShowGhost(ObjectDefinition definition, Point2D topLeft)
    {
        if (_ghostDefinition != definition)
        {
            ObjectGhost.Source = _drawing!.DrawObject(definition);
            _ghostDefinition = definition;
        }

        Canvas.SetLeft(ObjectGhost, topLeft.X * MapDrawing.CellPixels);
        Canvas.SetTop(ObjectGhost, topLeft.Y * MapDrawing.CellPixels);
        ObjectGhost.Visibility = Visibility.Visible;
    }

    private void FinishObjectDrag(Point2D point)
    {
        if (_selectionStart is { } start)
        {
            SelectAfterDrag(start, point);
        }
        else if (_movedObject is { } moved && IsPastDragDistance(moved.From, point))
        {
            Placing.MoveObject(moved.Placed.Uid, new Point2D(point.X + moved.Offset.X, point.Y + moved.Offset.Y));
        }
    }

    // A press and release within Windows' drag distance is a click, so a Shift + double-click doesn't nudge the object.
    private bool IsPastDragDistance(Point2D from, Point2D to)
    {
        var screenPixelsPerCell = MapDrawing.CellPixels * _viewport.Zoom;
        return Math.Abs(to.X - from.X) * screenPixelsPerCell >= SystemParameters.MinimumHorizontalDragDistance
            || Math.Abs(to.Y - from.Y) * screenPixelsPerCell >= SystemParameters.MinimumVerticalDragDistance;
    }

    // Ctrl + click, released within Windows' drag distance, selects the object on top there (decision 509).
    private void SelectAfterDrag(Point2D start, Point2D end)
    {
        if (IsPastDragDistance(start, end))
        {
            Placing.SelectObjectsIn(start, end);
        }
        else
        {
            Placing.SelectObjectAt(start);
        }
    }

    private void CancelObjectDrag()
    {
        _selectionStart = null;
        _movedObject = null;
        ShowObjectDragAt(_mapPoint);
    }
}
