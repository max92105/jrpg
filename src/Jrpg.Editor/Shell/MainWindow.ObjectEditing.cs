using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The window's object mode (legacy controls): a click places the chosen object centered on the mouse, Shift + drag
/// moves an object, a right click removes one, Ctrl + drag selects objects for Delete.
/// </summary>
internal sealed partial class MainWindow
{
    private static readonly Brush _selectionBrush = Brushes.Cyan;

    // The drag under way, if any: a selection rectangle from a point, or an object moved with an offset from the mouse.
    private Point2D? _selectionStart;
    private (PlacedObject Placed, Vector Offset)? _movedObject;
    private ObjectDefinition? _ghostDefinition;

    private bool HandleObjectButton(MouseButton button, Point2D point)
    {
        if (button == MouseButton.Right)
        {
            if (_viewModel.ObjectAt(point) is { } hit)
            {
                _viewModel.RemoveObject(hit.Uid);
            }

            return true;
        }

        if (button != MouseButton.Left)
        {
            return false;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            _selectionStart = point;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Shift && _viewModel.ObjectAt(point) is { } grabbed)
        {
            _movedObject = (grabbed, new Vector(grabbed.Position.X - point.X, grabbed.Position.Y - point.Y));
        }
        else
        {
            _viewModel.PlaceObjectAt(point);
            return true;
        }

        _strokeButton = button;
        MapViewport.CaptureMouse();
        return true;
    }

    private bool HandleObjectKey(Key key)
    {
        if (key != Key.Delete || _viewModel.Mode != EditingMode.Objects || Keyboard.FocusedElement is TextBox)
        {
            return false;
        }

        if (_viewModel.SelectedObjectUids.Count > 0)
        {
            _viewModel.DeleteSelectedObjects();
        }
        else if (_mapPoint is { } point && _viewModel.ObjectAt(point) is { } hovered)
        {
            _viewModel.RemoveObject(hovered.Uid);
        }

        return true;
    }

    /// <summary>Shows where the mouse would put an object: the dragged selection, the moved object, or the chosen one.</summary>
    /// <param name="point">The mouse in map cells, or null to hide it all.</param>
    private void ShowObjectDragAt(Point2D? point)
    {
        SelectionOutline.Visibility = Visibility.Collapsed;
        ObjectGhost.Visibility = Visibility.Collapsed;
        if (point is not { } at || _viewModel.Mode != EditingMode.Objects || _drawing is null)
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
        else if (_viewModel.Objects?.SelectedObject is { } chosen)
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
            _viewModel.SelectObjectsIn(start, point);
        }
        else if (_movedObject is { } moved)
        {
            _viewModel.MoveObject(moved.Placed.Uid, new Point2D(point.X + moved.Offset.X, point.Y + moved.Offset.Y));
        }
    }

    private void CancelObjectDrag()
    {
        _selectionStart = null;
        _movedObject = null;
        ShowObjectDragAt(_mapPoint);
    }

    private void DrawSelectionMarks()
    {
        SelectionMarks.Children.Clear();
        var objects = _viewModel.CurrentMap?.Map.Layout.Objects ?? [];
        foreach (var placed in objects.Where(placed => _viewModel.SelectedObjectUids.Contains(placed.Uid)))
        {
            var definition = _viewModel.Catalog!.Objects[placed.ObjectId];
            var mark = new Rectangle { Stroke = _selectionBrush, StrokeThickness = 2 / _viewport.Zoom };
            PlaceOnMap(mark, placed.Position, new Point2D(placed.Position.X + definition.Width, placed.Position.Y + definition.Height));
            SelectionMarks.Children.Add(mark);
        }
    }

    private void OnEditingTabChanged(object sender, SelectionChangedEventArgs e)
    {
        // Selection changes inside the tabs' lists bubble up here too.
        if (e.OriginalSource == EditingTabs)
        {
            _viewModel.Mode = ObjectsTab.IsSelected ? EditingMode.Objects : EditingMode.Tiles;
        }
    }

    /// <summary>Sizes a shape to the rectangle between two corners given in map cells.</summary>
    private static void PlaceOnMap(FrameworkElement shape, Point2D corner, Point2D oppositeCorner)
    {
        Canvas.SetLeft(shape, Math.Min(corner.X, oppositeCorner.X) * MapDrawing.CellPixels);
        Canvas.SetTop(shape, Math.Min(corner.Y, oppositeCorner.Y) * MapDrawing.CellPixels);
        shape.Width = Math.Abs(oppositeCorner.X - corner.X) * MapDrawing.CellPixels;
        shape.Height = Math.Abs(oppositeCorner.Y - corner.Y) * MapDrawing.CellPixels;
    }
}
