using System.Windows;

namespace Jrpg.Editor.Maps.Settings;

/// <summary>
/// The new map dialog. Create hands the name and size to the window's create step, which returns false when no map was
/// made (a field is invalid or the user kept the open map), and the dialog then stays open.
/// </summary>
internal sealed partial class NewMapWindow : Window
{
    private readonly NewMapViewModel _newMap;
    private readonly Func<NewMapViewModel, bool> _create;

    /// <param name="newMap">The name and size, edited here.</param>
    /// <param name="create">Makes and opens the map; false when it didn't.</param>
    public NewMapWindow(NewMapViewModel newMap, Func<NewMapViewModel, bool> create)
    {
        InitializeComponent();
        DataContext = _newMap = newMap;
        _create = create;
        Loaded += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    private void OnCreateClicked(object sender, RoutedEventArgs e)
    {
        if (_create(_newMap))
        {
            DialogResult = true;
        }
    }
}
