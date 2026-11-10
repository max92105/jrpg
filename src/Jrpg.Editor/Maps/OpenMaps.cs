using System.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The maps opened in this session, each kept with its unsaved changes and its history, so switching back shows a map
/// as it was left (decision 497).
/// </summary>
internal sealed class OpenMaps
{
    private readonly List<MapViewModel> _all = [];

    /// <summary>Any open map's <see cref="MapViewModel.IsDirty"/> changed: an edit, its undo or a save.</summary>
    public event EventHandler? DirtyChanged;

    /// <summary>In opening order.</summary>
    public IReadOnlyList<MapViewModel> All => _all;

    /// <summary>Each open map's layout as it is now, unsaved changes included: what the usage checks read.</summary>
    public IReadOnlyCollection<MapLayout> Layouts => [.. _all.Select(map => map.Map.Layout)];

    /// <summary>
    /// The map list's rows: the saved maps and the open new ones, each open map as it is now and marked while unsaved,
    /// sorted by display name then id (decisions 122, 498).
    /// </summary>
    /// <param name="saved">The content as last saved.</param>
    public IReadOnlyList<MapEntry> EntriesWith(ContentCatalog saved) =>
        [.. saved.Maps.Values
            .Select(map => Find(map.Id) is { } open ? (Map: open.Map, IsUnsaved: open.IsDirty) : (Map: map, IsUnsaved: false))
            .Concat(_all.Where(open => !saved.Maps.ContainsKey(open.Map.Id)).Select(open => (Map: open.Map, IsUnsaved: open.IsDirty)))
            .OrderBy(row => row.Map.Settings.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Map.Id, StringComparer.Ordinal)
            .Select(row => MapEntry.Of(row.Map, row.IsUnsaved))];

    /// <summary>Every listed map's layout by id, each open map's as it is now in place of its saved one.</summary>
    /// <param name="saved">The content as last saved.</param>
    public IReadOnlyDictionary<string, MapLayout> LayoutsWith(ContentCatalog saved)
    {
        var layouts = saved.Maps.ToDictionary(pair => pair.Key, pair => pair.Value.Layout);
        foreach (var open in _all)
        {
            layouts[open.Map.Id] = open.Map.Layout;
        }

        return layouts;
    }

    /// <summary>
    /// The content a save checks (decision 518): every open map as it is now in place of its saved one, a new map
    /// added, for Save all.
    /// </summary>
    /// <param name="saved">The content as last saved.</param>
    public ContentCatalog ContentWith(ContentCatalog saved) => ContentWith(saved, _all);

    /// <summary>
    /// The content a save checks (decision 518): this one open map as it is now in place of its saved one, the other
    /// maps as saved, for Save.
    /// </summary>
    /// <param name="saved">The content as last saved.</param>
    /// <param name="only">The shown map.</param>
    public static ContentCatalog ContentWith(ContentCatalog saved, MapViewModel only) => ContentWith(saved, [only]);

    /// <summary>The open maps with unsaved changes, in id order.</summary>
    public IReadOnlyList<MapViewModel> Dirty =>
        [.. _all.Where(map => map.IsDirty).OrderBy(map => map.Map.Id, StringComparer.Ordinal)];

    /// <summary>The open map with this id; null when it isn't open.</summary>
    public MapViewModel? Find(string mapId) => _all.Find(map => map.Map.Id == mapId);

    /// <exception cref="InvalidOperationException">A map with the same id is already open.</exception>
    public void Add(MapViewModel map)
    {
        if (Find(map.Map.Id) is not null)
        {
            throw new InvalidOperationException($"Map '{map.Map.Id}' is already open.");
        }

        _all.Add(map);
        map.PropertyChanged += OnMapChanged;
    }

    /// <summary>
    /// After the purge wrote the content (decision 442): every open map without unsaved changes is rebuilt from what
    /// was written, with no history, and dropped when the written content lacks it; a map with unsaved changes is kept
    /// as it is, so nothing unsaved is lost.
    /// </summary>
    public void ReplaceFrom(ContentCatalog written)
    {
        for (var index = _all.Count - 1; index >= 0; index--)
        {
            var open = _all[index];
            if (open.IsDirty)
            {
                continue;
            }

            open.PropertyChanged -= OnMapChanged;
            _all.RemoveAt(index);
            if (written.Maps.GetValueOrDefault(open.Map.Id) is { } map)
            {
                var rebuilt = new MapViewModel(map);
                rebuilt.PropertyChanged += OnMapChanged;
                _all.Insert(index, rebuilt);
            }
        }
    }

    private static ContentCatalog ContentWith(ContentCatalog saved, IEnumerable<MapViewModel> open)
    {
        var maps = new Dictionary<string, MapDefinition>(saved.Maps);
        foreach (var map in open)
        {
            maps[map.Map.Id] = map.Map;
        }

        return saved with { Maps = maps };
    }

    private void OnMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.IsDirty))
        {
            DirtyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
