using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map list as filtered (decision 504), and the map shown before the current one, for Back to previous map
/// (decision 505). Neither asks anything: maps stay open.
/// </summary>
internal sealed partial class MapNavigation : ObservableObject
{
    private readonly Func<string, bool> _tryOpenMap;
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

    /// <summary>Enter in the filter box: shows the first map the list shows.</summary>
    /// <returns>False when the filter matches no map.</returns>
    public bool TryOpenFirstShown() => ShownEntries is [var first, ..] && _tryOpenMap(first.Id);

    partial void OnFilterChanged(string value) => RefreshShownEntries();

    // A previous map no longer listed (a new map dropped, B2) can't be gone back to.
    private bool CanGoBack() => _previousMapId is { } previous && _entries.Any(entry => entry.Id == previous);

    /// <summary>Shows the map shown before the current one; pressed again, it returns.</summary>
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() => _tryOpenMap(_previousMapId!);

    private void RefreshShownEntries() =>
        ShownEntries = Filter.Length == 0 ? _entries : [.. _entries.Where(entry => entry.Text.Contains(Filter, StringComparison.OrdinalIgnoreCase))];
}
