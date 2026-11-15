using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Jrpg.Editor.Events;

/// <summary>
/// Opens a button's context menu under the button, as a drop-down: on its click, not only a right click.
/// </summary>
internal static class DropDownMenu
{
    /// <param name="button">The button whose context menu holds the entries.</param>
    public static void Open(Button button)
    {
        var menu = button.ContextMenu;
        menu.PlacementTarget = button;
        menu.Placement = PlacementMode.Bottom;
        menu.IsOpen = true;
    }
}
