namespace Jrpg.Editor.Workspaces;

/// <summary>
/// Which half of the editor the main window shows, chosen on the mode bar (decision 916); not
/// <see cref="Shell.EditingMode"/>, the side panel's tab inside World.
/// </summary>
internal enum EditorWorkspace
{
    /// <summary>The maps: the map list, the map, the side panel and their menus.</summary>
    World,

    /// <summary>The catalogs, with the map and its keys out of reach (decision 925).</summary>
    Database,
}
