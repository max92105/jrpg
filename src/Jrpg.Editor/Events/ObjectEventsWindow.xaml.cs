using System.Windows;

namespace Jrpg.Editor.Events;

/// <summary>The event dialog for one placed object; OK keeps the edited events, Cancel drops them.</summary>
internal sealed partial class ObjectEventsWindow : Window
{
    private readonly EventListViewModel _events;

    public ObjectEventsWindow(EventListViewModel events)
    {
        InitializeComponent();
        DataContext = _events = events;
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
