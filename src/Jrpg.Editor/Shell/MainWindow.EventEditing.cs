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
/// a right click removes a cell's. Events carry their markers in every mode: one dot per object event along the
/// object's top edge from the right, yellow when pressed, cyan when walked into, and a cyan square on a cell with one.
/// </summary>
internal sealed partial class MainWindow
{
    // Marker size and spacing, in map pixels (placeholders).
    private const double CellMarkerSize = 10;
    private const double CellMarkerInset = 2;
    private const double MarkerRadius = 6;
    private const double MarkerSpacing = 13;
    private const double MarkerInset = 8;

    // Left: the clicked object's events, or the cell's when no object is there. Right: removes the cell's event.
    private bool HandleEventButton(MouseButton button, Point2D point)
    {
        var eventEditing = _viewModel.EventEditing!;
        var cell = GridCell.Containing(point);
        switch (button)
        {
            case MouseButton.Left when eventEditing.StartEditingAt(point) is { } events:
                if (new ObjectEventsWindow(events) { Owner = this }.ShowDialog() == true)
                {
                    eventEditing.Apply(events);
                }

                return true;
            case MouseButton.Left when eventEditing.StartEditingCellAt(cell) is { } cellEvent:
                if (new CellEventWindow(cellEvent) { Owner = this }.ShowDialog() == true)
                {
                    eventEditing.Apply(cellEvent);
                }

                return true;
            // An object hides the cell under it, so its event isn't removed unseen.
            case MouseButton.Right when _viewModel.Placing!.ObjectAt(point) is null:
                eventEditing.RemoveCellEventAt(cell);
                return true;
            default:
                return false;
        }
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
