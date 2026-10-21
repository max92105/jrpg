using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Maps.Settings;
using Jrpg.Editor.Objects;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The editor window's state: the loaded content, the maps to choose from and the open one, which it opens, saves and
/// closes; painting tiles, placing objects and editing their events are <see cref="Painting"/>, <see cref="Placing"/>
/// and <see cref="EventEditing"/>. Content that can't load is shown with the loader's message instead of crashing, so a
/// broken file can be found and fixed.
/// </summary>
internal sealed partial class MainViewModel : ObservableObject
{
    private readonly ContentWriter _writer;
    private readonly Func<string, bool> _confirm;

    /// <param name="contentRoot">The content folder to edit.</param>
    /// <param name="confirm">Asks the user a yes or no question, such as whether to drop unsaved changes; true for yes.</param>
    public MainViewModel(string contentRoot, Func<string, bool> confirm)
    {
        _writer = new ContentWriter(contentRoot);
        _confirm = confirm;
        Layer.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LayerChoiceViewModel.Z))
            {
                RefreshLayerEntries();
            }
        };
        ArtRoot = ArtFolders.RootIn(contentRoot);
        try
        {
            Catalog = ContentCatalogLoader.Load(contentRoot);
            var artProblems = ArtReferenceCheck.FindProblems(Catalog, ArtRoot);
            LoadError = artProblems.Count == 0 ? null : string.Join(Environment.NewLine, artProblems);
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            LoadError = exception.Message;
        }

        // Maps, tiles and objects only show when their content and art are whole, since drawing reads both.
        if (Catalog is not null && LoadError is null)
        {
            ShowMapList(Catalog);
            Painting = new TilePaintingViewModel(Catalog, Layer);
            Placing = new ObjectPlacingViewModel(Catalog.Objects, Layer);
            EventEditing = new EventEditingViewModel(Catalog) { MapIds = MapIds };
            if (MapIds.Count > 0)
            {
                TryOpenMap(MapIds[0]);
            }
        }
    }

    public string ArtRoot { get; }

    /// <summary>The content as last loaded or saved; a save puts the saved map in it, so later saves check against it.</summary>
    public ContentCatalog? Catalog { get; private set; }

    /// <summary>Why the content can't be edited, naming the file; null when it loaded.</summary>
    public string? LoadError { get; }

    /// <summary>The saved maps, by id; a new map joins when it is first saved.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> MapIds { get; private set; } = [];

    /// <summary>The map list's rows: display name then id, sorted by display name (decision 122).</summary>
    [ObservableProperty]
    public partial IReadOnlyList<MapEntry> MapEntries { get; private set; } = [];

    /// <summary>The open map's layers, bottom first, and the chosen Z marked empty when the map has nothing there yet.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<LayerEntry> LayerEntries { get; private set; } = [];

    /// <summary>The layer tiles are painted and objects placed on.</summary>
    public LayerChoiceViewModel Layer { get; } = new();

    /// <summary>Null when the content or its art didn't load.</summary>
    public TilePaintingViewModel? Painting { get; }

    /// <summary>Null when the content or its art didn't load.</summary>
    public ObjectPlacingViewModel? Placing { get; }

    /// <summary>Null when the content or its art didn't load.</summary>
    public EventEditingViewModel? EventEditing { get; }

    [ObservableProperty]
    public partial MapViewModel? CurrentMap { get; private set; }

    /// <summary>Why the last save was refused, in the loader's words; null after a successful save.</summary>
    [ObservableProperty]
    public partial string? SaveError { get; private set; }

    /// <summary>Whether the cell grid is drawn over the map; on by default, as in the legacy editor.</summary>
    [ObservableProperty]
    public partial bool IsGridShown { get; set; } = true;

    /// <summary>What the status bar says about the cell under the cursor; empty when the cursor is off the map.</summary>
    [ObservableProperty]
    public partial string CursorStatus { get; private set; } = "";

    /// <summary>Whether clicks on the map paint tiles or place objects.</summary>
    [ObservableProperty]
    public partial EditingMode Mode { get; set; }

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

    /// <summary>The new map dialog's state; null when the content or its art didn't load, so no map can be made.</summary>
    public NewMapViewModel? StartNewMap() => Painting is not null ? new NewMapViewModel([.. Catalog!.Maps.Keys]) : null;

    /// <summary>
    /// Opens the new map dialog's map as an unsaved draft, asking first if the open map has unsaved changes. It joins the
    /// map list, and the content, when it is saved.
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

    /// <summary>Whether the editor may close: nothing unsaved, or the user agreed, when asked, to drop it.</summary>
    public bool ConfirmClose() => ConfirmLeaveCurrentMap();

    /// <param name="cell">The cell under the cursor, or null when the cursor left the view.</param>
    public void ShowCursorAt(GridCell? cell)
    {
        if (cell is not { } at || CurrentMap is null || !CurrentMap.Map.Layout.Contains(at))
        {
            CursorStatus = "";
            return;
        }

        var position = $"Cell ({at.X}, {at.Y})";
        CursorStatus = CurrentMap.TopTileAt(at) is { } tile ? $"{position}   Tile {tile.TileId} on layer z {tile.LayerZ}" : $"{position}   No tile";
    }

    /// <summary>The settings dialog's state for the open map; null when no map is open.</summary>
    public MapSettingsViewModel? StartEditingSettings() => CurrentMap is { } open ? new(open.Map, Catalog!.Encounters.Keys) : null;

    /// <summary>
    /// Applies the settings dialog to the open map: the new size first, asking before it drops anything, then the name
    /// and random battles. Each is one undo step.
    /// </summary>
    /// <returns>False when nothing was applied: a number is invalid or the user kept the old size.</returns>
    public bool TryApplySettings(MapSettingsViewModel edited)
    {
        if (CurrentMap is not { } open || edited.NewSize is not { } size || edited.OddsProblem is not null)
        {
            return false;
        }

        if (edited.IsResizing && !TryResize(open, size.Width, size.Height))
        {
            return false;
        }

        if (edited.AreSettingsChanged)
        {
            open.Do(new SetMapSettingsEdit(open.Map.Settings, edited.ToSettings()));
        }

        return true;
    }

    /// <summary>Saves the open map if the game would load it; otherwise keeps it unsaved and shows why.</summary>
    [RelayCommand]
    private void Save()
    {
        if (CurrentMap is null)
        {
            return;
        }

        var map = CurrentMap.Map;
        try
        {
            _writer.WriteMap(map, Catalog!);
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            SaveError = exception.Message;
            return;
        }

        Catalog = Catalog! with { Maps = new Dictionary<string, MapDefinition>(Catalog.Maps) { [map.Id] = map } };
        ShowMapList(Catalog);

        CurrentMap.MarkSaved();
        SaveError = null;
    }

    partial void OnMapIdsChanged(IReadOnlyList<string> value) => EventEditing?.MapIds = value;

    partial void OnCurrentMapChanged(MapViewModel? oldValue, MapViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnOpenMapChanged;
        newValue?.PropertyChanged += OnOpenMapChanged;
        Painting?.OpenMap = newValue;
        Placing?.OpenMap = newValue;
        EventEditing?.OpenMap = newValue;
        RefreshLayerEntries();
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

    private void OnOpenMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.Map))
        {
            RefreshLayerEntries();
        }
    }

    // Replaced only when a row changed: every painted cell changes the map, and the side list writes Z itself, so a new
    // list each time would reset the list's selection mid-click.
    private void RefreshLayerEntries()
    {
        var layers = CurrentMap?.Layers ?? [];
        var entries = layers.Select(layer => new LayerEntry(layer.Z, LayerPresets.NameOf(layer.Z), IsEmpty: false)).ToList();
        if (CurrentMap is not null && !layers.Any(layer => layer.Z == Layer.Z))
        {
            entries.Add(new LayerEntry(Layer.Z, LayerPresets.NameOf(Layer.Z), IsEmpty: true));
        }

        IReadOnlyList<LayerEntry> sorted = [.. entries.OrderBy(entry => entry.Z)];
        if (!sorted.SequenceEqual(LayerEntries))
        {
            LayerEntries = sorted;
        }
    }

    private bool TryResize(MapViewModel open, int width, int height)
    {
        var resize = new ResizeMapEdit(open.Map.Layout, width, height, Catalog!.Objects);
        var dropsSomething = resize.DroppedTileCount + resize.DroppedObjectCount + resize.DroppedCellEventCount > 0;
        var question = $"Resizing to {width} x {height} drops {resize.DroppedTileCount} tiles, {resize.DroppedObjectCount} objects "
            + $"and {resize.DroppedCellEventCount} cell events outside the new size. Resize anyway?";
        if (dropsSomething && !_confirm(question))
        {
            return false;
        }

        open.Do(resize);
        return true;
    }

    // A stroke still being dragged counts as a change, so it ends before the question.
    private bool ConfirmLeaveCurrentMap()
    {
        CurrentMap?.EndStroke();
        return CurrentMap is not { IsDirty: true } open
            || _confirm($"Map '{open.Map.Id}' has unsaved changes. Discard them?");
    }
}
