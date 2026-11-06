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

    /// <summary>
    /// A warp step's Go to was pressed: the dialog keeps its events as OK does, then the main window shows the step's
    /// destination (decision 508).
    /// </summary>
    public static readonly RoutedEvent GoToWarpDestinationEvent = EventManager.RegisterRoutedEvent(
        nameof(GoToWarpDestination), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EventStepEditor));

    public EventStepEditor()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler PickWarpDestination
    {
        add => AddHandler(PickWarpDestinationEvent, value);
        remove => RemoveHandler(PickWarpDestinationEvent, value);
    }

    public event RoutedEventHandler GoToWarpDestination
    {
        add => AddHandler(GoToWarpDestinationEvent, value);
        remove => RemoveHandler(GoToWarpDestinationEvent, value);
    }

    private void OnGoToWarpDestinationClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(GoToWarpDestinationEvent, sender));

    private void OnPickWarpDestinationClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(PickWarpDestinationEvent, sender));
}
