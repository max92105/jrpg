using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects;
using Jrpg.Editor.Objects.Composing;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The window's object mode: a click places the chosen object centered on the mouse, Shift + drag moves an object, a
/// right click removes one; Ctrl + click or Ctrl + drag selects objects for Delete and the arrow keys, and Esc clears
/// what is selected.
/// </summary>
internal sealed partial class MainWindow
{
    private static readonly Brush _selectionBrush = Brushes.Cyan;

    private void OnEditShapesClicked(object sender, RoutedEventArgs e) => EditShapes(Placing.StartEditingShapes());

    private void OnNewObjectClicked(object sender, RoutedEventArgs e) => Compose(_viewModel.Composing!.StartNew(), _viewModel.Composing.TryApply);

    private void OnNewObjectFromMapClicked(object sender, RoutedEventArgs e) => Compose(_viewModel.StartObjectFromMap(), _viewModel.TryApplyObjectFromMap);

    private void OnEditObjectClicked(object sender, RoutedEventArgs e) => Compose(_viewModel.Composing!.StartEdit(), _viewModel.Composing.TryApply);

    // A new object's shapes are drawn right away (decision 124), an edited one's when its size changed (decision 462);
    // cancelling them keeps the object as saved.
    /// <param name="composer">The composer to show; null shows nothing.</param>
    /// <param name="tryApply">The composer's Save: the object alone, or with the map cells replaced.</param>
    private void Compose(ObjectComposerViewModel? composer, Func<ObjectComposerViewModel, bool> tryApply)
    {
        if (composer is null || TileBrowser.Images is not { } images
            || new ObjectComposerWindow(composer, images, _viewModel.AnimationTime!, tryApply) { Owner = this }.ShowDialog() != true)
        {
            return;
        }

        EditingTabs.SelectedItem = ObjectsTab;
        if (composer.ShouldOpenShapesAfterSave)
        {
            EditShapes(Placing.StartEditingChosenShapes());
        }
    }

    // A new NPC keeps its default outline (decision 129), so no shapes window opens; Edit shapes is one click away.
    private void OnNewNpcClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.StartNewNpc() is { } newNpc
            && new NewNpcWindow(newNpc, _viewModel.CreateNpc, _drawing!.DrawObject) { Owner = this }.ShowDialog() == true)
        {
            EditingTabs.SelectedItem = ObjectsTab;
        }
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
        // Alt with Shift snaps the move; Ctrl stays exact (Ctrl + Alt is AltGr on some layouts).
        else if ((Keyboard.Modifiers & ~ModifierKeys.Alt) == ModifierKeys.Shift && Placing.ObjectAt(point) is { } grabbed)
        {
            _movedObject = (grabbed, new Vector(grabbed.Position.X - point.X, grabbed.Position.Y - point.Y), point, Placing.IsSelected(grabbed.Uid));
        }
        else
        {
            PlaceChosenObjectAt(point);
            return true;
        }

        _strokeButton = button;
        MapViewport.CaptureMouse();
        return true;
    }

    // Delete removes; G toggles snapping; Esc clears a selection and the arrows nudge it, while the map has the keys
    // (else Esc closes a drop-down and the arrows pan).
    private bool HandleObjectKey(Key key)
    {
        if (_viewModel.Mode != EditingMode.Objects || _viewModel.CurrentMap is null || Keyboard.FocusedElement is TextBox or ComboBox or ComboBoxItem)
        {
            return false;
        }

        switch (key)
        {
            case Key.Delete:
                DeleteSelectedOrHoveredObject();
                return true;
            case Key.G:
                Placing.ToggleGridSnap();
                return true;
            default:
                return HandleSelectionKey(key);
        }
    }

    private bool HandleSelectionKey(Key key)
    {
        if (Placing.SelectedObjectUids.Count == 0 || !MapViewport.IsKeyboardFocusWithin)
        {
            return false;
        }

        switch (key)
        {
            case Key.Escape:
                Placing.ClearSelection();
                return true;
            case Key.Left or Key.Right or Key.Up or Key.Down:
                NudgeSelection(key);
                return true;
            default:
                return false;
        }
    }

    private void PlaceChosenObjectAt(Point2D center)
    {
        if (IsSnapping)
        {
            Placing.PlaceObjectOnGridAt(center);
        }
        else
        {
            Placing.PlaceObjectAt(center);
        }
    }

    private void NudgeSelection(Key key)
    {
        var step = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? ObjectPlacingViewModel.FineNudgeCells : ObjectPlacingViewModel.NudgeCells;
        var (right, down) = key switch
        {
            Key.Left => (-step, 0.0),
            Key.Right => (step, 0.0),
            Key.Up => (0.0, -step),
            _ => (0.0, step),
        };
        Placing.MoveSelectedObjects(right, down);
    }

    private void DeleteSelectedOrHoveredObject()
    {
        if (Placing.SelectedObjectUids.Count > 0)
        {
            Placing.DeleteSelectedObjects();
        }
        else if (_mapPoint is { } point && Placing.ObjectAt(point) is { } hovered)
        {
            Placing.RemoveObject(hovered.Uid);
        }
    }

    private void DrawSelectionMarks()
    {
        SelectionMarks.Children.Clear();
        SelectionMarks.RenderTransform = Transform.Identity;
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
