using System.Windows;

namespace Jrpg.Editor.Workspaces;

/// <summary>
/// Places a Database dialog over its owner at the owner's size (decision 930): a catalog gets all the screen space the
/// main window has.
/// </summary>
internal static class DialogBounds
{
    // A maximized owner's Left, Top, Width and Height are its restored ones (or unset when it started maximized), so
    // the dialog takes its state instead; the dialog's own CenterOwner puts it on the owner's screen to maximize there.
    public static void FitTo(Window dialog, Window owner)
    {
        if (owner.WindowState == WindowState.Maximized)
        {
            dialog.WindowState = WindowState.Maximized;
            return;
        }

        dialog.WindowStartupLocation = WindowStartupLocation.Manual;
        dialog.Left = owner.Left;
        dialog.Top = owner.Top;
        dialog.Width = owner.ActualWidth;
        dialog.Height = owner.ActualHeight;
    }
}
