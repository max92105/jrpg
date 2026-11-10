using System.Windows;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The Rename map dialog (decision 530). OK hands the new id to the window's rename step, which returns false when
/// nothing was written (the id has a problem, or the write was refused), and the dialog then stays open.
/// </summary>
internal sealed partial class MapRenameWindow : Window
{
    private readonly MapRenamingViewModel _renaming;
    private readonly Func<MapRenamingViewModel, bool> _tryRename;

    /// <param name="renaming">The shown map's new id, edited here.</param>
    /// <param name="tryRename">Writes the rename; false when it didn't.</param>
    public MapRenameWindow(MapRenamingViewModel renaming, Func<MapRenamingViewModel, bool> tryRename)
    {
        InitializeComponent();
        DataContext = _renaming = renaming;
        _tryRename = tryRename;
        Loaded += (_, _) =>
        {
            IdBox.Focus();
            IdBox.SelectAll();
        };
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (_tryRename(_renaming))
        {
            DialogResult = true;
        }
    }
}
