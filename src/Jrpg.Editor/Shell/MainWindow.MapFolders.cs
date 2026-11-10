using System.Windows;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list's folder dialogs (decisions 533, 534): New folder and Move to folder, each a
/// <see cref="MapFolderWindow"/> over the main view model's folder steps.
/// </summary>
internal sealed partial class MainWindow
{
    // The list's New folder...: under the folder the menu is on, or at the top.
    private void OnNewFolderRequested(object sender, RoutedEventArgs e) =>
        MapFolderWindow.Choose(this, "New folder", _viewModel.StartNewFolder(MapListPanel.MenuFolder), _viewModel.TryAddFolder);

    // The list's Move to folder... on the row it just opened; a folder the map is already in is nothing to do, so the
    // dialog closes either way.
    private void OnMoveToFolderRequested(object sender, RoutedEventArgs e)
    {
        var mapId = _viewModel.CurrentMap!.Map.Id;
        MapFolderWindow.Choose(this, "Move to folder", _viewModel.StartMovingMap(mapId), choice =>
        {
            _viewModel.TryMoveMap(mapId, choice);
            return true;
        });
    }
}
