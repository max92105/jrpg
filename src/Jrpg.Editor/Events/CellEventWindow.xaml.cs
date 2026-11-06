using System.Windows;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Events;

/// <summary>The event dialog for one cell; OK keeps the edited event, Cancel drops it.</summary>
internal sealed partial class CellEventWindow : Window
{
    private readonly CellEventViewModel _cellEvent;

    private readonly MapDrawing _drawing;

    /// <param name="cellEvent">The cell's event, edited here until OK.</param>
    /// <param name="drawing">Draws the maps in a warp step's destination picker.</param>
    public CellEventWindow(CellEventViewModel cellEvent, MapDrawing drawing)
    {
        InitializeComponent();
        DataContext = _cellEvent = cellEvent;
        _drawing = drawing;
    }

    /// <summary>The destination a warp step's Go to closed the dialog with; null when OK or Cancel closed it.</summary>
    public WarpDestination? FollowedWarp { get; private set; }

    // As OK, keeping the events, then naming the step's destination for the main window to show.
    private void OnGoToWarpDestination(object sender, RoutedEventArgs e)
    {
        ProblemText.Text = _cellEvent.Problem;
        if (_cellEvent.Problem is null && e.Source is FrameworkElement { DataContext: EventStepViewModel { Destination: { } destination } })
        {
            FollowedWarp = destination;
            DialogResult = true;
        }
    }

    private void OnPickWarpDestination(object sender, RoutedEventArgs e)
    {
        if (e.Source is FrameworkElement { DataContext: EventStepViewModel step } && _cellEvent.Choices.WarpMaps is { } maps)
        {
            WarpDestinationWindow.TryPick(this, step, maps, _drawing);
        }
    }

    // An event without steps, or with a number left half typed, keeps the dialog open, saying what is missing.
    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        ProblemText.Text = _cellEvent.Problem;
        if (_cellEvent.Problem is null)
        {
            DialogResult = true;
        }
    }
}
