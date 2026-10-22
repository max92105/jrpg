using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects;

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
    private (PlacedObject Placed, Vector Offset, Point2D From)? _movedObject;
    private ObjectDefinition? _ghostDefinition;

    private void OnEditShapesClicked(object sender, RoutedEventArgs e) => EditShapes(Placing.StartEditingShapes());

    // A new object's shapes are drawn right away (decision 124); cancelling them keeps the object, without shapes.
    private void OnNewObjectClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.StartNewObject() is not { } newObject
            || new NewObjectWindow(newObject, _viewModel.CreateObject) { Owner = this }.ShowDialog() != true)
        {
            return;
        }

        EditingTabs.SelectedItem = ObjectsTab;
        EditShapes(Placing.StartEditingChosenShapes());
    }

    // Only a double-click on a listed object: not on the preview, a category header or the scroll bar.
    private void OnObjectBrowserDoubleClicked(object sender, MouseButtonEventArgs e)
    {
        for (var element = e.OriginalSource as DependencyObject; element is not null; element = VisualTreeHelper.GetParent(element))
        {
            if (element is ListBoxItem)
            {
                EditShapes(Placing.StartEditingChosenShapes());
                return;
            }
        }
    }

    // OK applies the outline as one catalog step; the catalog change redraws the map and the preview.
    private void EditShapes(ObjectShapesViewModel? shapes)
    {
        if (_drawing is null || shapes is null)
        {
            return;
        }

        var picture = _drawing.DrawObject(_viewModel.Catalog!.Objects[shapes.ObjectId]);
        if (new ObjectShapesWindow(shapes, picture) { Owner = this }.ShowDialog() == true)
        {
            Placing.ApplyShapes(shapes);
        }
    }

    // A plain click places the chosen object, so a placed object's shapes open on Shift + double-click: a Shift click
    // that doesn't move the object changes nothing.
    private bool HandleObjectButton(MouseButton button, Point2D point, int clickCount)
    {
        if (button == MouseButton.Left && clickCount == 2 && Keyboard.Modifiers == ModifierKeys.Shift)
        {
            EditShapes(Placing.StartEditingShapesAt(point));
            return true;
        }

        if (button == MouseButton.Right)
        {
            if (Placing.ObjectAt(point) is { } hit)
            {
                Placing.RemoveObject(hit.Uid);
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
        else if (Keyboard.Modifiers == ModifierKeys.Shift && Placing.ObjectAt(point) is { } grabbed)
        {
            _movedObject = (grabbed, new Vector(grabbed.Position.X - point.X, grabbed.Position.Y - point.Y), point);
        }
        else
        {
            Placing.PlaceObjectAt(point);
            return true;
        }

        _strokeButton = button;
        MapViewport.CaptureMouse();
        return true;
    }

    private bool HandleObjectKey(Key key)
    {
        if (key != Key.Delete || _viewModel.Mode != EditingMode.Objects || _viewModel.CurrentMap is null || Keyboard.FocusedElement is TextBox)
        {
            return false;
        }

        if (Placing.SelectedObjectUids.Count > 0)
        {
            Placing.DeleteSelectedObjects();
        }
        else if (_mapPoint is { } point && Placing.ObjectAt(point) is { } hovered)
        {
            Placing.RemoveObject(hovered.Uid);
        }

        return true;
    }

    /// <summary>Shows where the mouse would put an object: the dragged selection, the moved object, or the chosen one.</summary>
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
            Placing.SelectObjectsIn(start, point);
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
        var selected = _viewModel.Placing?.SelectedObjectUids ?? [];
        foreach (var placed in objects.Where(placed => selected.Contains(placed.Uid)))
        {
            var definition = _viewModel.Catalog!.Objects[placed.ObjectId];
            var mark = new Rectangle { Stroke = _selectionBrush, StrokeThickness = 2 / _viewport.Zoom };
            PlaceOnMap(mark, placed.Position, new Point2D(placed.Position.X + definition.Width, placed.Position.Y + definition.Height));
            SelectionMarks.Children.Add(mark);
        }
    }

    // Placing exists whenever a map is open, which the mouse and key handlers check first.
    private ObjectPlacingViewModel Placing => _viewModel.Placing!;

    private void OnEditingTabChanged(object sender, SelectionChangedEventArgs e)
    {
        // Selection changes inside the tabs' lists bubble up here too.
        if (e.OriginalSource == EditingTabs)
        {
            _viewModel.Mode = EditingTabs.SelectedItem switch
            {
                var tab when tab == ObjectsTab => EditingMode.Objects,
                var tab when tab == EventsTab => EditingMode.Events,
                _ => EditingMode.Tiles,
            };
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
