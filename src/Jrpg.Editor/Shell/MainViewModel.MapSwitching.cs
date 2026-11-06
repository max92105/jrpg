using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Shell;

/// <summary>Which maps are open and shown: the map list, opening and making maps.</summary>
internal sealed partial class MainViewModel
{
    /// <summary>The saved maps, by id; a new map joins when it is first saved.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> MapIds { get; private set; } = [];

    /// <summary>The map list's rows: display name then id, sorted by display name (decision 122).</summary>
    [ObservableProperty]
    public partial IReadOnlyList<MapEntry> MapEntries { get; private set; } = [];

    /// <summary>Opens another map, asking first if the open one has unsaved changes.</summary>
    /// <returns>False when the user kept the open map.</returns>
    public bool TryOpenMap(string mapId)
    {
        if (CurrentMap?.Map.Id == mapId)
        {
            return true;
        }

        if (!ConfirmLeaveCurrentMap())
        {
            return false;
        }

        CurrentMap = new MapViewModel(Catalog!.Maps[mapId]);
        SaveError = null;
        return true;
    }

    /// <summary>
    /// The new map dialog's state; null when the content or its art didn't load, so no map can be made.
    /// </summary>
    public NewMapViewModel? StartNewMap() => Painting is not null ? new NewMapViewModel([.. Catalog!.Maps.Keys]) : null;

    /// <summary>
    /// Opens the new map dialog's map as an unsaved draft, asking first if the open map has unsaved changes. It joins
    /// the map list, and the content, when it is saved.
    /// </summary>
    /// <returns>False when nothing was opened: the name or size is invalid, or the user kept the open map.</returns>
    public bool TryCreateMap(NewMapViewModel newMap)
    {
        if (newMap.NameProblem is not null || newMap.SizeProblem is not null || !ConfirmLeaveCurrentMap())
        {
            return false;
        }

        CurrentMap = MapViewModel.ForNewMap(newMap.ToMap());
        SaveError = null;
        return true;
    }

    /// <summary>
    /// Whether the open map or the tiles changed since they were saved; the tiles' changes stay when another map opens.
    /// </summary>
    public bool HasUnsavedChanges => CurrentMap is { IsDirty: true } || Content is { IsDirty: true };

    partial void OnMapIdsChanged(IReadOnlyList<string> value) => EventEditing?.MapIds = value;

    partial void OnCurrentMapChanged(MapViewModel? oldValue, MapViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnOpenMapChanged;
        newValue?.PropertyChanged += OnOpenMapChanged;
        History?.OpenMap = newValue;
        Painting?.OpenMap = newValue;
        Placing?.OpenMap = newValue;
        EventEditing?.OpenMap = newValue;
        RefreshLayerEntries();
        OnPropertyChanged(nameof(HasUnsavedChanges));
    }

    // A save can add a map or rename one, so both lists are rebuilt; the ids only when they changed, since the window
    // reselects the open map then.
    private void ShowMapList(ContentCatalog catalog)
    {
        IReadOnlyList<string> ids = [.. catalog.Maps.Keys.Order(StringComparer.Ordinal)];
        if (!ids.SequenceEqual(MapIds))
        {
            MapIds = ids;
        }

        MapEntries =
        [
            .. catalog.Maps.Values
                .OrderBy(map => map.Settings.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(map => map.Id, StringComparer.Ordinal)
                .Select(map => new MapEntry(map.Id, $"{map.Settings.DisplayName} ({map.Id})")),
        ];
    }

    // A stroke still being dragged counts as a change, so it ends before the question.
    private bool ConfirmLeaveCurrentMap()
    {
        CurrentMap?.EndStroke();
        return CurrentMap is not { IsDirty: true } open
            || ConfirmDiscard($"Map '{open.Map.Id}' has");
    }

    /// <param name="unsaved">What has unsaved changes, with its verb: "Map 'forest' has", "The tiles have".</param>
    private bool ConfirmDiscard(string unsaved) => _confirm($"{unsaved} unsaved changes. Discard them?");
}
