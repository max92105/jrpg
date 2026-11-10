using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The map list's rows arranged into folders (decision 532): folders first, by name ignoring case, then maps in the
/// list's order; a folder holds its subfolders and maps. The session's empty folders show until a filter narrows the
/// tree to the matching maps inside their folders, every folder shown open (decisions 534, 535).
/// </summary>
internal static class MapTree
{
    /// <param name="entries">The map list's rows, in the list's order.</param>
    /// <param name="sessionFolders">Folders made this session that hold no map yet.</param>
    /// <param name="filter">Part of a map's text, any case; empty shows every map.</param>
    /// <param name="expandedFolders">
    /// The folders open in the tree, kept across rebuilds while no filter applies.
    /// </param>
    public static IReadOnlyList<MapTreeNode> Of(IReadOnlyList<MapEntry> entries, IReadOnlyCollection<string> sessionFolders, string filter, IReadOnlySet<string> expandedFolders)
    {
        var isFiltering = filter.Length > 0;
        var maps = isFiltering ? [.. entries.Where(entry => entry.Matches(filter))] : entries;
        var folders = maps.Select(map => map.Folder).OfType<string>().Concat(isFiltering ? [] : sessionFolders).ToHashSet(StringComparer.Ordinal);
        return RowsUnder(null, new Shown(maps, folders, isFiltering, expandedFolders));
    }

    /// <summary>The maps as the tree shows them, depth first: Enter in the filter opens the first.</summary>
    public static IEnumerable<MapEntry> MapsInOrder(IReadOnlyList<MapTreeNode> rows) =>
        rows.SelectMany(row => row.Map is { } map ? [map] : MapsInOrder(row.Rows));

    /// <summary>
    /// The nodes from the top down to a map, its folders then itself, so the view can open them and select it; empty
    /// when the tree doesn't hold the map.
    /// </summary>
    public static IReadOnlyList<MapTreeNode> PathTo(IReadOnlyList<MapTreeNode> rows, string mapId)
    {
        foreach (var row in rows)
        {
            if (row.Map?.Id == mapId)
            {
                return [row];
            }

            if (PathTo(row.Rows, mapId) is { Count: > 0 } below)
            {
                return [row, .. below];
            }
        }

        return [];
    }

    // A folder's rows: the folders right under it, then its own maps.
    private static List<MapTreeNode> RowsUnder(string? parent, Shown shown)
    {
        var subfolders = shown.Folders
            .Select(folder => FolderRightUnder(parent, folder))
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(folder => folder, StringComparer.OrdinalIgnoreCase)
            .Select(folder => MapTreeNode.ForFolder(folder, RowsUnder(folder, shown), isExpanded: shown.IsFiltering || shown.ExpandedFolders.Contains(folder)));
        var ownMaps = shown.Maps.Where(map => map.Folder == parent).Select(MapTreeNode.ForMap);
        return [.. subfolders, .. ownMaps];
    }

    // The part of a path one name below the parent ("Act 1/Village" under "Act 1"; "Act 1" at the top), or null when
    // the path isn't under it.
    private static string? FolderRightUnder(string? parent, string path)
    {
        var names = MapFolderPath.NamesOf(path);
        var parentDepth = parent is null ? 0 : MapFolderPath.NamesOf(parent).Count;
        if (names.Count <= parentDepth || (parent is not null && !MapFolderPath.IsWithin(path, parent)))
        {
            return null;
        }

        return string.Join(MapFolderPath.Separator, names.Take(parentDepth + 1));
    }

    // What the tree is built from: the maps shown, every folder shown (the maps' and the session's), whether a filter
    // applies (every folder open) and the folders open otherwise.
    private sealed record Shown(IReadOnlyList<MapEntry> Maps, IReadOnlySet<string> Folders, bool IsFiltering, IReadOnlySet<string> ExpandedFolders);
}
