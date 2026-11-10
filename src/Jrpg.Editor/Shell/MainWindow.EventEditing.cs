using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The window's event mode: a click on an object opens its events, a click on a cell without one opens the cell's, and
/// a right click removes a cell's; with Ctrl, the cell under any object: a click opens its event, a right click removes
/// it (decision 526). Events carry their markers in every mode: one dot per object event along the object's top edge
/// from the right, yellow when pressed, cyan when walked into, and a cyan square on a cell with one.
/// </summary>
internal sealed partial class MainWindow
{
    // Marker size and spacing, in map pixels (placeholders).
    private const double CellMarkerSize = 10;
    private const double CellMarkerInset = 2;
    private const double MarkerRadius = 6;
    private const double MarkerSpacing = 13;
    private const double MarkerInset = 8;

    // The Placed object panel's Edit events...: the same dialog as a click in Events mode, Go to included.
    private void OnPlacedObjectEditEvents(object sender, RoutedEventArgs e)
    {
        if (_viewModel.EventEditing!.StartEditing(Placing.Editing.Uid) is { } events)
        {
            EditObjectEvents(events);
        }
    }

    private void EditObjectEvents(EventListViewModel events)
    {
        var objectWindow = new ObjectEventsWindow(events, _drawing!) { Owner = this };
        if (objectWindow.ShowDialog() == true)
        {
            _viewModel.EventEditing!.Apply(events);
            GoToFollowedWarp(objectWindow.FollowedWarp);
        }
    }

    // Left: the clicked object's events, or the cell's when no object is there. Right: removes the cell's event. Ctrl
    // goes to the cell whatever covers it.
    private bool HandleEventButton(MouseButton button, Point2D point)
    {
        var cell = GridCell.Containing(point);
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            return HandleCellEventButton(button, cell);
        }

        switch (button)
        {
            case MouseButton.Left when _viewModel.EventEditing!.StartEditingAt(point) is { } events:
                EditObjectEvents(events);
                return true;
            case MouseButton.Left:
                return EditCellEventAt(cell);
            // An object hides the cell under it, so its event isn't removed unseen.
            case MouseButton.Right when _viewModel.Placing!.ObjectAt(point) is null:
                _viewModel.EventEditing!.RemoveCellEventAt(cell);
                return true;
            default:
                return false;
        }
    }

    // Ctrl reaches the cell under any object (decision 526): left opens its event, right removes it.
    private bool HandleCellEventButton(MouseButton button, GridCell cell)
    {
        switch (button)
        {
            case MouseButton.Left:
                return EditCellEventAt(cell);
            case MouseButton.Right:
                _viewModel.EventEditing!.RemoveCellEventAt(cell);
                return true;
            default:
                return false;
        }
    }

    // The cell's dialog, Go to included; false off the map.
    private bool EditCellEventAt(GridCell cell)
    {
        if (_viewModel.EventEditing!.StartEditingCellAt(cell) is not { } cellEvent)
        {
            return false;
        }

        var cellWindow = new CellEventWindow(cellEvent, _drawing!) { Owner = this };
        if (cellWindow.ShowDialog() == true)
        {
            _viewModel.EventEditing.Apply(cellEvent);
            GoToFollowedWarp(cellWindow.FollowedWarp);
        }

        return true;
    }

    private void DrawEventMarks()
    {
        EventMarks.Children.Clear();
        DrawCellEventMarks();
        foreach (var placed in _viewModel.CurrentMap?.Map.Layout.Objects ?? [])
        {
            var right = (placed.Position.X + _viewModel.Catalog!.Objects[placed.ObjectId].Width) * MapDrawing.CellPixels;
            for (var index = 0; index < placed.Events.Count; index++)
            {
                var marker = new Ellipse
                {
                    Width = 2 * MarkerRadius,
                    Height = 2 * MarkerRadius,
                    Fill = MarkerFill(placed.Events[index].Activation),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5,
                };
                Canvas.SetLeft(marker, right - MarkerInset - (index * MarkerSpacing) - MarkerRadius);
                Canvas.SetTop(marker, (placed.Position.Y * MapDrawing.CellPixels) + MarkerInset - MarkerRadius);
                EventMarks.Children.Add(marker);
            }
        }
    }

    // One color per trigger, on objects and cells alike: cyan when walked into, gold when pressed, orchid on arrival.
    private static SolidColorBrush MarkerFill(EventActivation activation) => activation switch
    {
        EventActivation.Touch => Brushes.Cyan,
        EventActivation.Arrive => Brushes.Orchid,
        _ => Brushes.Gold,
    };

    // The trigger marker: a small square in the cell's top-right corner, colored by what starts the event.
    private void DrawCellEventMarks()
    {
        foreach (var cellEvent in _viewModel.CurrentMap?.Map.Layout.CellEvents ?? [])
        {
            var marker = new Rectangle
            {
                Width = CellMarkerSize,
                Height = CellMarkerSize,
                Fill = MarkerFill(cellEvent.Event.Activation),
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };
            Canvas.SetLeft(marker, ((cellEvent.Cell.X + 1) * MapDrawing.CellPixels) - CellMarkerSize - CellMarkerInset);
            Canvas.SetTop(marker, (cellEvent.Cell.Y * MapDrawing.CellPixels) + CellMarkerInset);
            EventMarks.Children.Add(marker);
        }
    }
}
