using System.Windows;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The folder dialog (decisions 532-534), for New folder, Move to folder and Rename folder: the title says which. OK
/// hands the choice to the caller's step, which returns false when the dialog is to stay open (nothing was done).
/// </summary>
internal sealed partial class MapFolderWindow : Window
{
    private readonly MapFolderChoiceViewModel _choice;
    private readonly Func<MapFolderChoiceViewModel, bool> _apply;

    private MapFolderWindow(string title, MapFolderChoiceViewModel choice, Func<MapFolderChoiceViewModel, bool> apply)
    {
        InitializeComponent();
        Title = title;
        DataContext = _choice = choice;
        _apply = apply;
        Loaded += (_, _) => PathBox.Focus();
    }

    /// <summary>Shows the dialog over its owner until OK's step is done or it is cancelled.</summary>
    /// <param name="owner">The window the dialog is centered on.</param>
    /// <param name="title">What the choice is for: "New folder", "Move to folder".</param>
    /// <param name="choice">The path offered, edited here.</param>
    /// <param name="apply">OK's step; false keeps the dialog open.</param>
    public static void Choose(Window owner, string title, MapFolderChoiceViewModel choice, Func<MapFolderChoiceViewModel, bool> apply) =>
        new MapFolderWindow(title, choice, apply) { Owner = owner }.ShowDialog();

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (_apply(_choice))
        {
            DialogResult = true;
        }
    }
}
