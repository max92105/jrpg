using System.Windows;
using System.Windows.Controls;

namespace Jrpg.Editor.Objects;

/// <summary>Shows <see cref="PlacedObjectEditing"/> under the Objects panel's list (decision 513).</summary>
internal sealed partial class PlacedObjectPanel : UserControl
{
    /// <summary>
    /// Edit events... was pressed: the window opens the shown object's event dialog, as a click in Events mode does.
    /// </summary>
    public static readonly RoutedEvent EditEventsRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(EditEventsRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PlacedObjectPanel));

    public PlacedObjectPanel()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler EditEventsRequested
    {
        add => AddHandler(EditEventsRequestedEvent, value);
        remove => RemoveHandler(EditEventsRequestedEvent, value);
    }

    private void OnEditEventsClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(EditEventsRequestedEvent, this));
}
