using System.Windows.Controls;

namespace Jrpg.Editor.Shell;

/// <summary>The map list: choosing and reselecting the shown map; later its filter, Back and Go to's arrival.</summary>
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

    // A save (a new map, a new name) or a row's unsaved mark rebuilds the list, which drops its selection; the list is
    // refreshed first, since this handler may run before its binding does.
    private void ReselectShownMap()
    {
        MapList.GetBindingExpression(ItemsControl.ItemsSourceProperty)?.UpdateTarget();
        MapList.SelectedValue = _shownMap?.Map.Id;
    }
}
