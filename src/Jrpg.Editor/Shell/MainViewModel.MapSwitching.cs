using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Which maps are open and shown: the map list, opening and making maps. Every map opened stays open with its unsaved
/// changes, so switching never asks (decision 497).
/// </summary>
internal sealed partial class MainViewModel
{
    private readonly OpenMaps _openMaps = new();

    /// <summary>The saved maps, by id; a new map joins when it is first saved.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> MapIds { get; private set; } = [];

    /// <summary>
    /// The map list's rows: the saved maps and the new ones not saved yet, display name then id, sorted by display name
    /// (decision 122); an unsaved map's row is marked (decision 498).
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<MapEntry> MapEntries { get; private set; } = [];

    /// <summary>The map list's filter and Back to previous map (decisions 504, 505).</summary>
    public MapNavigation Navigation => field ??= new(TryOpenMap);

    /// <summary>Shows another map, opening it the first time; an open map comes back as it was left.</summary>
    /// <returns>False when no map has this id, saved or open.</returns>
    public bool TryOpenMap(string mapId)
    {
        if (CurrentMap?.Map.Id == mapId)
        {
            return true;
        }

        if (_openMaps.Find(mapId) is not { } open)
        {
            if (Catalog?.Maps.GetValueOrDefault(mapId) is not { } saved)
            {
                return false;
            }

            open = new MapViewModel(saved);
            _openMaps.Add(open);
        }

        Show(open);
        return true;
    }

    /// <summary>
    /// The new map dialog's state; null when the content or its art didn't load, so no map can be made. The saved maps'
    /// ids and the open new maps' are taken.
    /// </summary>
    public NewMapViewModel? StartNewMap() =>
        Painting is not null ? new NewMapViewModel([.. Catalog!.Maps.Keys.Union(_openMaps.All.Select(open => open.Map.Id))]) : null;

    /// <summary>
    /// Opens the new map dialog's map as an unsaved draft, listed at once and open until the editor closes; it joins
    /// the content when it is saved.
    /// </summary>
    /// <returns>False when nothing was opened: the name or size is invalid.</returns>
    public bool TryCreateMap(NewMapViewModel newMap)
    {
        if (newMap.NameProblem is not null || newMap.SizeProblem is not null)
        {
            return false;
        }

        var created = MapViewModel.ForNewMap(newMap.ToMap());
        _openMaps.Add(created);
        ShowMapList(Catalog!);
        Show(created);
        return true;
    }

    /// <summary>Whether an open map or the tiles changed since they were saved.</summary>
    public bool HasUnsavedChanges => _openMaps.Dirty.Count > 0 || Content is { IsDirty: true };

    partial void OnMapIdsChanged(IReadOnlyList<string> value) => EventEditing?.MapIds = value;

    partial void OnCurrentMapChanged(MapViewModel? oldValue, MapViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnOpenMapChanged;
        newValue?.PropertyChanged += OnOpenMapChanged;
        History?.OpenMap = newValue;
        Painting?.OpenMap = newValue;
        Placing?.OpenMap = newValue;
        EventEditing?.OpenMap = newValue;
        Navigation.NoteShown(newValue?.Map.Id);
        RefreshLayerEntries();
        RaiseUnsavedChanged();
    }

    // A stroke still being dragged becomes its map's undo step before another map is shown.
    private void Show(MapViewModel map)
    {
        CurrentMap?.EndStroke();
        CurrentMap = map;
        SaveError = null;
    }

    // The purge or a rename wrote the content at once: the open maps are rebuilt from the written content, with no
    // history, and the shown one shown again, under its new id after a rename, before the catalogs change (the window
    // redraws on that change). Never through "no map shown", which would lose the previous map for Back.
    private void ShowRewritten(ContentCatalog written)
    {
        _openMaps.ReplaceFrom(written);
        var shownId = _shownAfterRewrite ?? CurrentMap?.Map.Id;
        _shownAfterRewrite = null;
        if (shownId is not null && _openMaps.Find(shownId) is null && written.Maps.TryGetValue(shownId, out var renamed))
        {
            _openMaps.Add(new MapViewModel(renamed));
        }

        CurrentMap = shownId is null ? null : _openMaps.Find(shownId);
    }

    private void OnOpenMapsDirtyChanged(object? sender, EventArgs e)
    {
        ShowMapList(Catalog!);
        RaiseUnsavedChanged();
    }

    // A save can add a map or rename one, and an edit or its undo marks a row or clears its mark, so both lists are
    // rebuilt; each only when it changed, since the window reselects the open map then.
    private void ShowMapList(ContentCatalog catalog)
    {
        IReadOnlyList<string> ids = [.. catalog.Maps.Keys.Order(StringComparer.Ordinal)];
        if (!ids.SequenceEqual(MapIds))
        {
            MapIds = ids;
        }

        var entries = _openMaps.EntriesWith(catalog);
        if (!entries.SequenceEqual(MapEntries))
        {
            MapEntries = entries;
            Navigation.ShowEntries(entries);
        }
    }
}
