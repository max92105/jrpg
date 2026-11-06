using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list: choosing and reselecting the shown map, its filter box (decision 504); later Go to's arrival.
/// </summary>
internal sealed partial class MainWindow
{
    private void OpenCurrentMap()
    {
        _shownMap?.PropertyChanged -= OnOpenMapChanged;
        _shownMap = _viewModel.CurrentMap;
        _shownMap?.PropertyChanged += OnOpenMapChanged;

        // Kept in step here rather than bound, so a switch refused (an unknown id) leaves the list on the open map.
        MapList.SelectedValue = _shownMap?.Map.Id;
        _gridDrawnFor = null;
        DrawCurrentMap();
        DrawEventMarks();
        RefitMap();
    }

    private void OnMapListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MapList.SelectedValue is string mapId && !_viewModel.TryOpenMap(mapId))
        {
            MapList.SelectedValue = _shownMap?.Map.Id;
        }
    }

    // A save (a new map, a new name), a row's unsaved mark or the filter replaces the rows, which drops the selection;
    // this runs once the new rows are in.
    private void OnMapListUpdated(object? sender, DataTransferEventArgs e) => MapList.SelectedValue = _shownMap?.Map.Id;

    private void OnGoToMapExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        MapFilterBox.Focus();
        MapFilterBox.SelectAll();
    }

    // Enter opens the first map shown, Esc clears the filter and gives the map back the keys, Down goes to the list.
    private void OnMapFilterKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                _viewModel.Navigation.TryOpenFirstShown();
                break;
            case Key.Escape:
                _viewModel.Navigation.Filter = "";
                MapViewport.Focus();
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
