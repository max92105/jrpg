using System.ComponentModel;
using Jrpg.Core.Content;

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

    private void OnMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.IsDirty))
        {
            DirtyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
