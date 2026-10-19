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
/// The window's event mode: a click on an object opens its events. Objects with events carry the legacy markers in
/// every mode: one dot per event along the object's top edge from the right, yellow when pressed, cyan when walked into.
/// </summary>
internal sealed partial class MainWindow
{
    // Legacy marker size and spacing, in map pixels.
    private const double MarkerRadius = 6;
    private const double MarkerSpacing = 13;
    private const double MarkerInset = 8;

    private bool HandleEventButton(MouseButton button, Point2D point)
    {
        if (button != MouseButton.Left)
        {
            return false;
        }

        var eventEditing = _viewModel.EventEditing!;
        if (eventEditing.StartEditingAt(point) is { } events && new ObjectEventsWindow(events) { Owner = this }.ShowDialog() == true)
        {
            eventEditing.Apply(events);
        }

        return true;
    }

    private void DrawEventMarks()
    {
        EventMarks.Children.Clear();
        foreach (var placed in _viewModel.CurrentMap?.Map.Layout.Objects ?? [])
        {
            var right = (placed.Position.X + _viewModel.Catalog!.Objects[placed.ObjectId].Width) * MapDrawing.CellPixels;
            for (var index = 0; index < placed.Events.Count; index++)
            {
                var marker = new Ellipse
                {
                    Width = 2 * MarkerRadius,
                    Height = 2 * MarkerRadius,
                    Fill = placed.Events[index].Activation == EventActivation.Touch ? Brushes.Cyan : Brushes.Gold,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5,
                };
                Canvas.SetLeft(marker, right - MarkerInset - (index * MarkerSpacing) - MarkerRadius);
                Canvas.SetTop(marker, (placed.Position.Y * MapDrawing.CellPixels) + MarkerInset - MarkerRadius);
                EventMarks.Children.Add(marker);
            }
        }
    }
}
