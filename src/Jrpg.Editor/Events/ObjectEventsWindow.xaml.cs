using System.Windows;

namespace Jrpg.Editor.Events;

/// <summary>The event dialog for one placed object; OK keeps the edited events, Cancel drops them.</summary>
internal sealed partial class ObjectEventsWindow : Window
{
    public ObjectEventsWindow(EventListViewModel events)
    {
        InitializeComponent();
        DataContext = events;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
