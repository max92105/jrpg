using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>One row of the map tree as the tree shows it (decision 532): a folder with its rows, or a map.</summary>
internal sealed partial class MapTreeNode : ObservableObject
{
    private MapTreeNode(string text, string? folderPath, MapEntry? map, IReadOnlyList<MapTreeNode> rows)
    {
        (Text, FolderPath, Map, Rows) = (text, folderPath, map, rows);
    }

    /// <summary>A folder's last name, or the map entry's text.</summary>
    public string Text { get; }

    /// <summary>A folder's full path; null for a map.</summary>
    public string? FolderPath { get; }

    /// <summary>The map's entry; null for a folder.</summary>
    public MapEntry? Map { get; }

    /// <summary>A folder's rows, its subfolders first; empty for a map.</summary>
    public IReadOnlyList<MapTreeNode> Rows { get; }

    public bool IsFolder => FolderPath is not null;

    /// <summary>Whether a folder shows its rows; the view binds it both ways.</summary>
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    /// <summary>Whether the row is the tree's selected one; the view binds it both ways.</summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <param name="path">The folder's full path; the row shows its last name.</param>
    /// <param name="rows">What it holds.</param>
    /// <param name="isExpanded">Whether it starts open.</param>
    public static MapTreeNode ForFolder(string path, IReadOnlyList<MapTreeNode> rows, bool isExpanded) =>
        new(MapFolderPath.NamesOf(path)[^1], path, null, rows) { IsExpanded = isExpanded };

    public static MapTreeNode ForMap(MapEntry map) => new(map.Text, null, map, []);
}
