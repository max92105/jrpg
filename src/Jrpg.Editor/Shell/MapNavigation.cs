using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list as filtered (decision 504) and as a tree of folders (decision 532), the session's empty folders
/// (decision 534), and the map shown before the current one, for Back to previous map (decision 505). None asks
/// anything: maps stay open.
/// </summary>
internal sealed partial class MapNavigation : ObservableObject
{
    private readonly Func<string, bool> _tryOpenMap;
    private readonly HashSet<string> _sessionFolders = new(StringComparer.Ordinal);
    private readonly HashSet<string> _expandedFolders = new(StringComparer.Ordinal);
    private IReadOnlyList<MapEntry> _entries = [];
    private string? _shownMapId;
    private string? _previousMapId;

    /// <param name="tryOpenMap">Shows a map by id; false when no map has it.</param>
    public MapNavigation(Func<string, bool> tryOpenMap) => _tryOpenMap = tryOpenMap;

    /// <summary>Part of a map's display name or id, any case; empty shows every map.</summary>
    [ObservableProperty]
    public partial string Filter { get; set; } = "";

    /// <summary>The list's rows holding <see cref="Filter"/> in their text, in the list's order.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<MapEntry> ShownEntries { get; private set; } = [];

    /// <summary>The same rows as the tree shows them: folders holding their maps (<see cref="MapTree"/>).</summary>
    [ObservableProperty]
    public partial IReadOnlyList<MapTreeNode> ShownTree { get; private set; } = [];

    /// <summary>New folder (decision 534): an empty folder shown for the session, until a map moves in.</summary>
    public void AddSessionFolder(string path)
    {
        _sessionFolders.Add(path);
        _expandedFolders.Add(path);
        RefreshShownEntries();
    }

    /// <summary>A folder was opened or closed in the tree, so a rebuild after a save keeps it so.</summary>
    public void NoteExpanded(string path, bool isExpanded)
    {
        if (isExpanded)
        {
            _expandedFolders.Add(path);
        }
        else
        {
            _expandedFolders.Remove(path);
        }
    }

    /// <summary>The map list's rows changed: a save, a new map, a row's unsaved mark.</summary>
    public void ShowEntries(IReadOnlyList<MapEntry> entries)
    {
        _entries = entries;
        RefreshShownEntries();
        GoBackCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// A map is shown: the one shown until now becomes the previous one; the same map again changes nothing.
    /// </summary>
    public void NoteShown(string? mapId)
    {
        if (mapId == _shownMapId)
        {
            return;
        }

        _previousMapId = _shownMapId;
        _shownMapId = mapId;
        GoBackCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// A map's id was renamed (decision 530): the shown and the previous map follow it, so Back still reaches the
    /// renamed map or returns from it.
    /// </summary>
    public void RenameMap(string oldId, string newId)
    {
        if (_shownMapId == oldId)
        {
            _shownMapId = newId;
        }

        if (_previousMapId == oldId)
        {
            _previousMapId = newId;
        }

        GoBackCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Enter in the filter box: shows the first map the tree shows.</summary>
    /// <returns>False when the filter matches no map.</returns>
    public bool TryOpenFirstShown() => MapTree.MapsInOrder(ShownTree).FirstOrDefault() is { } first && _tryOpenMap(first.Id);

    partial void OnFilterChanged(string value) => RefreshShownEntries();

    // A previous map no longer listed (a new map dropped, B2) can't be gone back to.
    private bool CanGoBack() => _previousMapId is { } previous && _entries.Any(entry => entry.Id == previous);

    /// <summary>Shows the map shown before the current one; pressed again, it returns.</summary>
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() => _tryOpenMap(_previousMapId!);

    private void RefreshShownEntries()
    {
        ShownEntries = Filter.Length == 0 ? _entries : [.. _entries.Where(entry => entry.Matches(Filter))];
        ShownTree = MapTree.Of(_entries, _sessionFolders, Filter, _expandedFolders);
    }
}
