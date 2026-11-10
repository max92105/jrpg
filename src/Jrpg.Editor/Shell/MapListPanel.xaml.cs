using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list beside the map, a tree of folders (decision 532), and its filter box (decision 504): choosing a map row
/// opens the map, and the open map's row is selected again, its folders opened, whenever the rows are replaced; a right
/// click selects the row and offers Rename map (decision 530); a folder opened or closed stays so. Its DataContext is
/// the main view model.
/// </summary>
internal sealed partial class MapListPanel : UserControl
{
    /// <summary>Escape in the filter box: the map takes the keys back (the window focuses it).</summary>
    public static readonly RoutedEvent FilterLeftEvent = EventManager.RegisterRoutedEvent(
        nameof(FilterLeft), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapListPanel));

    /// <summary>Rename map... was chosen on a row: the window opens the Rename map dialog on the shown map.</summary>
    public static readonly RoutedEvent RenameRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(RenameRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapListPanel));

    private string? _openMapId;

    public MapListPanel()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler FilterLeft
    {
        add => AddHandler(FilterLeftEvent, value);
        remove => RemoveHandler(FilterLeftEvent, value);
    }

    public event RoutedEventHandler RenameRequested
    {
        add => AddHandler(RenameRequestedEvent, value);
        remove => RemoveHandler(RenameRequestedEvent, value);
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    /// <summary>
    /// Selects the open map's row and opens the folders above it, kept here rather than bound, so a switch refused (an
    /// unknown id) leaves the tree on the open map; null selects none.
    /// </summary>
    public void SelectMap(string? mapId)
    {
        _openMapId = mapId;
        var path = mapId is null ? [] : MapTree.PathTo(ViewModel.Navigation.ShownTree, mapId);
        if (path.Count == 0)
        {
            // The tree keeps a row selected until told otherwise; a row left selected can't be clicked again.
            if (MapTreeView.SelectedItem is MapTreeNode shown)
            {
                shown.IsSelected = false;
            }

            return;
        }

        foreach (var folder in path.SkipLast(1))
        {
            folder.IsExpanded = true;
        }

        path[^1].IsSelected = true;
    }

    /// <summary>Find / Ctrl+M: the filter box takes the keys, its text selected.</summary>
    public void FocusFilter()
    {
        MapFilterBox.Focus();
        MapFilterBox.SelectAll();
    }

    // A map row opens its map; a folder row opens nothing.
    private void OnMapTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is MapTreeNode { Map: { } map } && !ViewModel.TryOpenMap(map.Id))
        {
            SelectMap(_openMapId);
        }
    }

    // A save (a new map, a new name), a row's unsaved mark or the filter replaces the rows, which drops the selection;
    // this runs once the new rows are in.
    private void OnMapTreeUpdated(object? sender, DataTransferEventArgs e) => SelectMap(_openMapId);

    private void OnFolderExpanded(object sender, RoutedEventArgs e) => NoteExpanded(e, isExpanded: true);

    private void OnFolderCollapsed(object sender, RoutedEventArgs e) => NoteExpanded(e, isExpanded: false);

    // SelectMap opening a folder is noted too: the folder stays open on the next rebuild.
    private void NoteExpanded(RoutedEventArgs e, bool isExpanded)
    {
        if (e.OriginalSource is TreeViewItem { DataContext: MapTreeNode { FolderPath: { } path } })
        {
            ViewModel.Navigation.NoteExpanded(path, isExpanded);
        }
    }

    // The row under the mouse is selected, which opens its map, before the context menu shows.
    private void OnMapTreeRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        for (var element = e.OriginalSource as DependencyObject; element is not null; element = VisualTreeHelper.GetParent(element))
        {
            if (element is TreeViewItem row)
            {
                row.IsSelected = true;
                return;
            }
        }
    }

    private void OnRenameMapClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(RenameRequestedEvent, this));

    // Enter opens the first map shown, Esc clears the filter and gives the map back the keys, Down goes to the tree.
    private void OnMapFilterKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                ViewModel.Navigation.TryOpenFirstShown();
                break;
            case Key.Escape:
                ViewModel.Navigation.Filter = "";
                RaiseEvent(new RoutedEventArgs(FilterLeftEvent, this));
                break;
            case Key.Down:
                MapTreeView.Focus();
                break;
            default:
                return;
        }

        e.Handled = true;
    }
}
