using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list's folders, managed from the tree (decisions 532-534): New folder adds an empty folder for the session;
/// Move to folder, or a drag, shows the moved map and records one settings step on it, so Ctrl+Z undoes the move and
/// Ctrl+S saves it.
/// </summary>
internal sealed partial class MainViewModel
{
    /// <summary>
    /// The folder dialog for New folder: the box offers the parent's path with the separator after it.
    /// </summary>
    /// <param name="parentPath">The folder right-clicked; null for a folder at the top.</param>
    public MapFolderChoiceViewModel StartNewFolder(string? parentPath) =>
        new(KnownFolders, parentPath is null ? null : parentPath + MapFolderPath.Separator);

    /// <returns>False when nothing was added: the path has a problem or is the top of the list.</returns>
    public bool TryAddFolder(MapFolderChoiceViewModel choice)
    {
        if (choice.Problem is not null || choice.ToPath() is not { } path)
        {
            return false;
        }

        Navigation.AddSessionFolder(path);
        return true;
    }

    /// <summary>The folder dialog for Move to folder, starting on the map's folder as it is now.</summary>
    /// <param name="mapId">A map the list shows.</param>
    public MapFolderChoiceViewModel StartMovingMap(string mapId) =>
        new(KnownFolders, MapEntries.Single(entry => entry.Id == mapId).Folder);

    /// <returns>False when nothing was recorded: the path has a problem, or the map is already there.</returns>
    public bool TryMoveMap(string mapId, MapFolderChoiceViewModel choice) => choice.Problem is null && TryMoveMapTo(mapId, choice.ToPath());

    /// <summary>
    /// A drag onto a folder, or the dialog's choice: the map is shown first, so Ctrl+Z reaches the step (decision 503),
    /// then its folder is set as one settings step.
    /// </summary>
    /// <param name="mapId">A map the list shows, saved or open.</param>
    /// <param name="folderPath">The folder moved to; null for the top of the list.</param>
    /// <returns>False when nothing was recorded: no map has the id, or the map is already there.</returns>
    public bool TryMoveMapTo(string mapId, string? folderPath)
    {
        // The folder is checked on the row before the map is shown: a drop onto its own folder shows nothing.
        if (MapEntries.SingleOrDefault(entry => entry.Id == mapId) is not { } entry || entry.Folder == folderPath)
        {
            return false;
        }

        TryOpenMap(mapId);
        var settings = CurrentMap!.Map.Settings;
        CurrentMap.Do(new SetMapSettingsEdit(settings, settings with { Folder = folderPath }));
        return true;
    }

    // The folders the dialog offers: the listed maps' as they are now and the session's empty ones.
    private IEnumerable<string> KnownFolders => MapEntries.Select(entry => entry.Folder).OfType<string>().Concat(Navigation.SessionFolders);
}
