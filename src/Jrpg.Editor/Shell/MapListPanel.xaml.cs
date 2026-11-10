using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list beside the map and its filter box (decision 504): choosing a row opens the map, and the open map's row
/// is selected again whenever the rows are replaced; a right click opens the map and offers Rename map (decision 530).
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
    /// Selects the open map's row, kept here rather than bound, so a switch refused (an unknown id) leaves the list on
    /// the open map; null selects none.
    /// </summary>
    public void SelectMap(string? mapId)
    {
        _openMapId = mapId;
        MapList.SelectedValue = mapId;
    }

    /// <summary>Find / Ctrl+M: the filter box takes the keys, its text selected.</summary>
    public void FocusFilter()
    {
        MapFilterBox.Focus();
        MapFilterBox.SelectAll();
    }

    private void OnMapListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MapList.SelectedValue is string mapId && !ViewModel.TryOpenMap(mapId))
        {
            MapList.SelectedValue = _openMapId;
        }
    }

    // A save (a new map, a new name), a row's unsaved mark or the filter replaces the rows, which drops the selection;
    // this runs once the new rows are in.
    private void OnMapListUpdated(object? sender, DataTransferEventArgs e) => MapList.SelectedValue = _openMapId;

    private void OnRenameMapClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(RenameRequestedEvent, this));

    // Enter opens the first map shown, Esc clears the filter and gives the map back the keys, Down goes to the list.
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
                MapList.Focus();
                break;
            default:
                return;
        }

        e.Handled = true;
    }
}
