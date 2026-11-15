using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Jrpg.Editor.Events;

/// <summary>
/// Runs an event dialog list's command from a key (decision 734) and gives the keyboard back to the list's chosen row,
/// so the key can be pressed again.
/// </summary>
internal static class ListKeyCommand
{
    /// <summary>
    /// The key pressed and the modifiers held; Alt turns an arrow into the system key, read here as the arrow.
    /// </summary>
    public static (Key Key, ModifierKeys Modifiers) Pressed(KeyEventArgs e) =>
        (e.Key == Key.System ? e.SystemKey : e.Key, Keyboard.Modifiers);

    /// <param name="list">The list the key was pressed in.</param>
    /// <param name="command">Removes, moves or adds a row; nothing happens while it can't run.</param>
    public static void Run(ListBox list, ICommand command)
    {
        // The toolkit's Execute doesn't check CanExecute itself (.claude/rules/editor.md).
        if (!command.CanExecute(null))
        {
            return;
        }

        command.Execute(null);

        // A removed or moved row's container is replaced, which drops the focus: wait for the rows' new layout.
        list.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => FocusChosenRow(list));
    }

    private static void FocusChosenRow(ListBox list)
    {
        if (list.SelectedItem is { } chosen && list.ItemContainerGenerator.ContainerFromItem(chosen) is ListBoxItem row)
        {
            row.Focus();
        }
        else
        {
            list.Focus();
        }
    }
}
