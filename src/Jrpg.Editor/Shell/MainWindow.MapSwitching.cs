using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Showing the open map: its row in the map list panel kept selected (decision 504), the filter box
/// reached by Find, and a warp's Go to arriving on its destination (decision 508).
/// </summary>
internal sealed partial class MainWindow
{
    private void OpenCurrentMap()
    {
        _shownMap?.PropertyChanged -= OnOpenMapChanged;
        _shownMap = _viewModel.CurrentMap;
        _shownMap?.PropertyChanged += OnOpenMapChanged;
        MapListPanel.SelectMap(_shownMap?.Map.Id);
        _gridDrawnFor = null;
        HideDestinationMark();
        DrawCurrentMap();
        DrawEventMarks();
        RefitMap();
    }

    private void OnGoToMapExecuted(object sender, ExecutedRoutedEventArgs e) => MapListPanel.FocusFilter();

    // Esc in the filter box: the map takes the keys back, so the arrows pan again.
    private void OnMapFilterLeft(object sender, RoutedEventArgs e) => MapViewport.Focus();

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
