using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list's folders, managed from the tree (decisions 532-534): New folder adds an empty folder for the session;
/// Move to folder, or a drag, shows the moved map and records one settings step on it, so Ctrl+Z undoes the move and
/// Ctrl+S saves it; Rename folder changes every map in the folder and is written at once, refused while anything is
/// unsaved, as a map id rename (decision 533).
/// </summary>
internal sealed partial class MainViewModel
{
    private const string SaveAllBeforeFolderRename = "Save all first (Ctrl+Shift+S): renaming a folder writes every map in it.";

    /// <summary>Whether Rename folder may open: nothing is unsaved.</summary>
    public bool CanRenameFolder => !HasUnsavedChanges;

    /// <summary>The Rename folder entry's tooltip: what a rename does, or why the entry is greyed.</summary>
    public string RenameFolderTip => HasUnsavedChanges ? SaveAllBeforeFolderRename : "Change the folder's path; every map in it and its subfolders is written at once.";

    /// <summary>The folder dialog for New folder.</summary>
    /// <param name="parentPath">The folder right-clicked; null for a folder at the top.</param>
    public MapFolderChoiceViewModel StartNewFolder(string? parentPath) => MapFolderChoiceViewModel.ForNewFolder(KnownFolders, parentPath);

    /// <returns>False when nothing was added: the path has a problem (the top among them, for a new folder).</returns>
    public bool TryAddFolder(MapFolderChoiceViewModel choice)
    {
        if (choice.Problem is not null)
        {
            return false;
        }

        Navigation.AddSessionFolder(choice.ToPath()!);
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

    /// <summary>The folder dialog for Rename folder, starting on the folder's path.</summary>
    public MapFolderChoiceViewModel StartRenamingFolder(string folderPath) => MapFolderChoiceViewModel.ForRenamingFolder(KnownFolders, folderPath);

    /// <summary>
    /// Every map in the folder and its subfolders gets its path under the new one, written at once through Data, the
    /// editor carrying on from the written content with every undo history afresh; a folder holding no map is renamed
    /// in the list alone.
    /// </summary>
    /// <returns>
    /// False when nothing changed: the path has a problem, something is unsaved (the greyed entry rules it out; this is
    /// the backstop), or the write was refused, <see cref="SaveError"/> saying why.
    /// </returns>
    public bool TryRenameFolder(string folderPath, MapFolderChoiceViewModel choice)
    {
        if (choice.Problem is not null)
        {
            return false;
        }

        if (HasUnsavedChanges)
        {
            SaveError = SaveAllBeforeFolderRename;
            return false;
        }

        var newPath = choice.ToPath()!;
        var moved = MapsMoved(Content!.Current, folderPath, newPath);
        SaveError = null;
        if (moved.Count > 0)
        {
            var maps = new Dictionary<string, MapDefinition>(Content.Current.Maps);
            foreach (var map in moved)
            {
                maps[map.Id] = map;
            }

            var rewritten = Content.Current with { Maps = maps };
            if (!TryWrite(() => _writer.WriteMaps(rewritten, [.. moved.Select(map => map.Id)])))
            {
                return false;
            }

            Content.ReplaceAfterRewrite(rewritten);
        }

        Navigation.RenameFolders(folderPath, newPath);
        ShowMapList(Content.Current);
        return true;
    }

    // The maps within the folder, each with its path moved under the new one; "Act 10" isn't within "Act 1".
    private static List<MapDefinition> MapsMoved(ContentCatalog content, string folderPath, string newPath) =>
        [.. content.Maps.Values
            .Where(map => map.Settings.Folder is { } folder && MapFolderPath.IsWithin(folder, folderPath))
            .Select(map => new MapDefinition(map.Layout, map.Settings with { Folder = MapFolderPath.Moved(map.Settings.Folder!, folderPath, newPath) }))];

    // The folders the dialog offers: the listed maps' as they are now and the session's empty ones.
    private IEnumerable<string> KnownFolders => MapEntries.Select(entry => entry.Folder).OfType<string>().Concat(Navigation.SessionFolders);
}
