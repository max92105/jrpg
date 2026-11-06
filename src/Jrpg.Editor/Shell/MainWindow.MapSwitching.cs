using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list: choosing and reselecting the shown map, its filter box (decision 504), and a warp's Go to arriving on
/// its destination (decision 508).
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
        HideDestinationMark();
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

    // The dialog closed with Go to: its map shown (opened if needed, never asked), at 100 % with the arrival cell in
    // the middle of the view, marked until the next press on the map.
    private void GoToFollowedWarp(WarpDestination? warp)
    {
        if (warp is null || !_viewModel.TryOpenMap(warp.MapId))
        {
            return;
        }

        var view = new Size(MapViewport.ActualWidth, MapViewport.ActualHeight);
        var cellPixels = MapDrawing.CellPixels;
        _isFitPending = false;
        _viewport.ZoomAt(new Point(view.Width / 2, view.Height / 2), 1);
        _viewport.CenterOn(new Point((warp.Cell.X + 0.5) * cellPixels, (warp.Cell.Y + 0.5) * cellPixels), view);
        ApplyViewport();
        Canvas.SetLeft(DestinationMark, warp.Cell.X * cellPixels);
        Canvas.SetTop(DestinationMark, warp.Cell.Y * cellPixels);
        DestinationMark.Width = DestinationMark.Height = cellPixels;
        DestinationMark.Visibility = Visibility.Visible;
    }

    // Any press on the map, handled or not, and another map shown: the arrival was seen.
    private void HideDestinationMark() => DestinationMark.Visibility = Visibility.Collapsed;
}
