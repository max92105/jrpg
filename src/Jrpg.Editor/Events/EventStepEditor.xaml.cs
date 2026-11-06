using System.Windows;
using System.Windows.Controls;

namespace Jrpg.Editor.Events;

/// <summary>Shows the fields of the <see cref="EventStepViewModel"/> it is given, for the event and cell event dialogs.</summary>
internal sealed partial class EventStepEditor : UserControl
{
    /// <summary>
    /// A warp step's Pick... was pressed: the dialog holding the editor opens the picker, the editor's data context
    /// being the step.
    /// </summary>
    public static readonly RoutedEvent PickWarpDestinationEvent = EventManager.RegisterRoutedEvent(
        nameof(PickWarpDestination), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EventStepEditor));

    public EventStepEditor()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler PickWarpDestination
    {
        add => AddHandler(PickWarpDestinationEvent, value);
        remove => RemoveHandler(PickWarpDestinationEvent, value);
    }

    private void OnPickWarpDestinationClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(PickWarpDestinationEvent, sender));
}
