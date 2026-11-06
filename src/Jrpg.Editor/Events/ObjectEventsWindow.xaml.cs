using System.Windows;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Events;

/// <summary>The event dialog for one placed object; OK keeps the edited events, Cancel drops them.</summary>
internal sealed partial class ObjectEventsWindow : Window
{
    private readonly EventListViewModel _events;

    private readonly MapDrawing _drawing;

    /// <param name="events">The object's events, edited here until OK.</param>
    /// <param name="drawing">Draws the maps in a warp step's destination picker.</param>
    public ObjectEventsWindow(EventListViewModel events, MapDrawing drawing)
    {
        InitializeComponent();
        DataContext = _events = events;
        _drawing = drawing;
    }

    /// <summary>The destination a warp step's Go to closed the dialog with; null when OK or Cancel closed it.</summary>
    public WarpDestination? FollowedWarp { get; private set; }

    // As OK, keeping the events, then naming the step's destination for the main window to show.
    private void OnGoToWarpDestination(object sender, RoutedEventArgs e)
    {
        ProblemText.Text = _events.Problem;
        if (_events.Problem is null && e.Source is FrameworkElement { DataContext: EventStepViewModel { Destination: { } destination } })
        {
            FollowedWarp = destination;
            DialogResult = true;
        }
    }

    private void OnPickWarpDestination(object sender, RoutedEventArgs e)
    {
        if (e.Source is FrameworkElement { DataContext: EventStepViewModel step } && _events.Choices.WarpMaps is { } maps)
        {
            WarpDestinationWindow.TryPick(this, step, maps, _drawing);
        }
    }

    // A number left half typed keeps the dialog open, saying which event and step it is in.
    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        ProblemText.Text = _events.Problem;
        if (_events.Problem is null)
        {
            DialogResult = true;
        }
    }
}
