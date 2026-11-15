using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jrpg.Editor.Events;

/// <summary>
/// Shows the <see cref="EventViewModel"/> it is given, for the object and the cell event dialogs alike: its conditions,
/// its steps and the chosen step's fields. The rows only an object's event has follow
/// <see cref="EventViewModel.IsObjectEvent"/>. The step list takes Insert, Delete, Alt+Up and Alt+Down (decision 734).
/// </summary>
internal sealed partial class EventView : UserControl
{
    public EventView() => InitializeComponent();

    private void OnAddStepClicked(object sender, RoutedEventArgs e) => DropDownMenu.Open(AddStepButton);

    // A plain Up or Down stays the list's own. Insert opens Add step: the open menu takes the keyboard.
    private void OnStepListKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not EventViewModel mapEvent)
        {
            return;
        }

        var pressed = ListKeyCommand.Pressed(e);
        if (pressed == (Key.Insert, ModifierKeys.None))
        {
            DropDownMenu.Open(AddStepButton);
            e.Handled = true;
            return;
        }

        ICommand? command = pressed switch
        {
            (Key.Delete, ModifierKeys.None) => mapEvent.RemoveStepCommand,
            (Key.Up, ModifierKeys.Alt) => mapEvent.MoveStepUpCommand,
            (Key.Down, ModifierKeys.Alt) => mapEvent.MoveStepDownCommand,
            _ => null,
        };
        if (command is not null)
        {
            ListKeyCommand.Run(StepList, command);
            e.Handled = true;
        }
    }
}
