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
/// click selects the row and offers Rename map (decision 530), Move to folder and New folder (the window opens the
/// dialogs); a map row dragged onto a folder moves the map there (decision 533); a folder opened or closed stays so.
/// Its DataContext is the main view model.
/// </summary>
internal sealed partial class MapListPanel : UserControl
{
    /// <summary>Escape in the filter box: the map takes the keys back (the window focuses it).</summary>
    public static readonly RoutedEvent FilterLeftEvent = EventManager.RegisterRoutedEvent(
        nameof(FilterLeft), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapListPanel));

    /// <summary>Rename map... was chosen on a row: the window opens the Rename map dialog on the shown map.</summary>
    public static readonly RoutedEvent RenameRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(RenameRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapListPanel));

    /// <summary>
    /// Move to folder... was chosen on a map row: the window opens the folder dialog on the shown map.
    /// </summary>
    public static readonly RoutedEvent MoveRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(MoveRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapListPanel));

    /// <summary>New folder... was chosen: the window opens the folder dialog under <see cref="MenuFolder"/>.</summary>
    public static readonly RoutedEvent NewFolderRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(NewFolderRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapListPanel));

    // A dragged map row's payload; text dragged from a box or another app is refused by it.
    private const string MapRowFormat = "Jrpg.MapId";

    private string? _openMapId;
    private (Point At, string MapId)? _dragStart;

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

    public event RoutedEventHandler MoveRequested
    {
        add => AddHandler(MoveRequestedEvent, value);
        remove => RemoveHandler(MoveRequestedEvent, value);
    }

    public event RoutedEventHandler NewFolderRequested
    {
        add => AddHandler(NewFolderRequestedEvent, value);
        remove => RemoveHandler(NewFolderRequestedEvent, value);
    }

    /// <summary>
    /// The folder the open menu is on (a folder row, or a map row's folder, by mouse or keyboard); null on the empty
    /// space below the rows or a map at the top. New folder... starts under it.
    /// </summary>
    public string? MenuFolder { get; private set; }

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
        if (RowUnder(e.OriginalSource) is { } row)
        {
            row.IsSelected = true;
        }
    }

    // By keyboard (Shift+F10), the source is the focused row.
    private void OnMapTreeMenuOpening(object sender, ContextMenuEventArgs e)
    {
        var row = RowUnder(e.OriginalSource);
        MenuFolder = row?.FolderPath ?? row?.Map?.Folder;
    }

    // A drag starts on a map row and becomes one past the system's drag distance, so a click still only opens the map.
    private void OnMapTreeLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        _dragStart = RowUnder(e.OriginalSource) is { Map: { } map } ? (e.GetPosition(MapTreeView), map.Id) : null;

    private void OnMapTreeMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragStart is not { } start || e.LeftButton != MouseButtonState.Pressed)
        {
            _dragStart = null;
            return;
        }

        var moved = e.GetPosition(MapTreeView) - start.At;
        if (Math.Abs(moved.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(moved.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _dragStart = null;
        DragDrop.DoDragDrop(MapTreeView, new DataObject(MapRowFormat, start.MapId), DragDropEffects.Move);
    }

    private void OnMapTreeDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(MapRowFormat) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    // Dropped on a folder row: into it; on a map row: beside it; on the empty space below the rows: the top.
    private void OnMapTreeDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(MapRowFormat) is not string mapId)
        {
            return;
        }

        var row = RowUnder(e.OriginalSource);
        ViewModel.TryMoveMapTo(mapId, row?.FolderPath ?? row?.Map?.Folder);
        e.Handled = true;
    }

    private static MapTreeNode? RowUnder(object source)
    {
        for (var element = source as DependencyObject; element is not null; element = VisualTreeHelper.GetParent(element))
        {
            if (element is TreeViewItem { DataContext: MapTreeNode row })
            {
                return row;
            }
        }

        return null;
    }

    private void OnRenameMapClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(RenameRequestedEvent, this));

    private void OnMoveToFolderClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(MoveRequestedEvent, this));

    private void OnNewFolderClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(NewFolderRequestedEvent, this));

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
