using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
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
    private readonly Dictionary<string, (int Width, int Height)> _sheetImageSizes = new(StringComparer.OrdinalIgnoreCase);
    private readonly UndoJournal _journal = new();

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
            var loaded = ContentCatalogLoader.Load(contentRoot);
            Content = new CatalogDraft(loaded);
            var artProblems = ArtReferenceCheck.FindProblems(loaded, ArtRoot);
            LoadError = artProblems.Count == 0 ? null : string.Join(Environment.NewLine, artProblems);
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            LoadError = exception.Message;
        }

        // Maps, tiles and objects only show when their content and art are whole, since drawing reads both.
        if (Content is not null && LoadError is null)
        {
            ShowMapList(Content.Current);
            Painting = new TilePaintingViewModel(Content, Layer, new SheetImages(() => ArtFolders.TilesheetImagesIn(ArtRoot), SheetImageSizeOf), confirm);
            Marking = new WalkabilityMarking(Content, Painting.Browser);
            Content.PropertyChanged += OnContentChanged;
            Content.StepRecorded += (_, _) => RecordStep(UndoTarget.Catalogs);
            Content.RedoDropped += (_, _) =>
            {
                _journal.ForgetUndoneCatalogSteps();
                NotifyUndoRedoChanged();
            };
            Placing = new ObjectPlacingViewModel(Content, Layer);
            EventEditing = new EventEditingViewModel(Content) { MapIds = MapIds };
            if (MapIds.Count > 0)
            {
                TryOpenMap(MapIds[0]);
            }
        }
    }

    public string ArtRoot { get; }

    /// <summary>The content the editor works on (ADR 0014); null when it didn't load.</summary>
    public CatalogDraft? Content { get; }

    /// <summary>The content as last loaded or saved; a save puts the saved map in it, so later saves check against it.</summary>
    public ContentCatalog? Catalog => Content?.Current;

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
    public WalkabilityMarking? Marking { get; }

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

    /// <summary>Whether blocked cells, collision outlines and above-player zones are drawn over the map; off by default.</summary>
    [ObservableProperty]
    public partial bool IsCollisionShown { get; set; }

    /// <summary>Why the last undo was refused: a tile it would remove is still in use; null after any other undo, redo or edit.</summary>
    [ObservableProperty]
    public partial string? UndoRefusal { get; private set; }

    /// <summary>What the last import copied and refused ("Imported Forest2.png"); empty before any import.</summary>
    [ObservableProperty]
    public partial string ImportStatus { get; private set; } = "";

    /// <summary>The images an import copied, raised before the last is shown, so cached pictures of replaced files are dropped.</summary>
    public event EventHandler<IReadOnlyList<string>>? SheetImagesImported;

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

    /// <summary>Whether the open map or the tiles changed since they were saved; the tiles' changes stay when another map opens.</summary>
    public bool HasUnsavedChanges => CurrentMap is { IsDirty: true } || Content is { IsDirty: true };

    /// <summary>Whether the editor may close: nothing unsaved, or the user agreed, when asked, to drop it.</summary>
    public bool ConfirmClose()
    {
        if (Content is not { IsDirty: true })
        {
            return ConfirmLeaveCurrentMap();
        }

        CurrentMap?.EndStroke();
        var unsaved = CurrentMap is { IsDirty: true } open ? $"Map '{open.Map.Id}' and the tiles have" : "The tiles have";
        return _confirm($"{unsaved} unsaved changes. Discard them?");
    }

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

    /// <summary>Says in the status bar why a click on the map painted nothing: no cell is chosen in the Tiles panel.</summary>
    public void ShowPaintHint() => CursorStatus = "Choose a tile in the Tiles panel";

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

    /// <summary>Undoes the newest step, whether it went to the open map or to the catalogs (decision 117).</summary>
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        // A stroke still being dragged is the newest step.
        CurrentMap?.EndStroke();
        if (!_journal.CanUndo)
        {
            return;
        }

        UndoRefusal = _journal.NextUndo == UndoTarget.Catalogs ? RefusalOfCatalogUndo() : null;
        if (UndoRefusal is not null)
        {
            return;
        }

        if (_journal.Undo() == UndoTarget.Map)
        {
            CurrentMap!.UndoCommand.Execute(null);
        }
        else
        {
            Content!.Undo();
        }

        NotifyUndoRedoChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        CurrentMap?.EndStroke();
        if (!_journal.CanRedo)
        {
            return;
        }

        UndoRefusal = null;

        if (_journal.Redo() == UndoTarget.Map)
        {
            CurrentMap!.RedoCommand.Execute(null);
        }
        else
        {
            Content!.Redo();
        }

        NotifyUndoRedoChanged();
    }

    // The first tile the undo would remove that something still uses, named with its first user (decision 117); or the
    // first tile cut from a sheet it would remove.
    private string? RefusalOfCatalogUndo()
    {
        foreach (var sheetId in Content!.SheetsRemovedByUndo())
        {
            if (CatalogUsage.TilesCutFrom(sheetId, Content.Current) is [var cutTileId, ..])
            {
                return $"Can't undo: tile {cutTileId} is cut from sheet {sheetId}.";
            }
        }

        foreach (var tileId in Content.TilesRemovedByUndo())
        {
            if (CatalogUsage.UsersOfTile(tileId, Content.Current, CurrentMap?.Map.Layout) is [var user, ..])
            {
                return $"Can't undo: tile {tileId} is used on {user}.";
            }
        }

        return null;
    }

    private bool CanUndo() => _journal.CanUndo;

    private bool CanRedo() => _journal.CanRedo;

    private void OnMapStepRecorded(object? sender, EventArgs e) => RecordStep(UndoTarget.Map);

    private void RecordStep(UndoTarget step)
    {
        UndoRefusal = null;
        _journal.Record(step);
        NotifyUndoRedoChanged();
    }

    private void NotifyUndoRedoChanged()
    {
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Saves the open map if the game would load it; otherwise keeps it unsaved and shows why.</summary>
    [RelayCommand]
    private void Save()
    {
        if (CurrentMap is null || Content is not { } content)
        {
            return;
        }

        var map = CurrentMap.Map;
        try
        {
            // The catalogs first, so a saved map never names a tile the game doesn't know.
            if (content.IsDirty)
            {
                _writer.WriteMapCatalogs(content.Current);
                content.MarkSaved();
            }

            _writer.WriteMap(map, content.Current);
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            SaveError = exception.Message;
            return;
        }

        content.AddSavedMap(map);
        ShowMapList(content.Current);

        CurrentMap.MarkSaved();
        SaveError = null;
    }

    partial void OnMapIdsChanged(IReadOnlyList<string> value) => EventEditing?.MapIds = value;

    partial void OnCurrentMapChanged(MapViewModel? oldValue, MapViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnOpenMapChanged;
        newValue?.PropertyChanged += OnOpenMapChanged;
        oldValue?.StepRecorded -= OnMapStepRecorded;
        newValue?.StepRecorded += OnMapStepRecorded;
        _journal.ForgetMapSteps();
        NotifyUndoRedoChanged();
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

    [RelayCommand]
    private void ToggleCollision() => IsCollisionShown = !IsCollisionShown;

    /// <summary>
    /// Copies images into the tilesheet art (asking before replacing one), then shows the last one copied, "(not set up)"
    /// if it has no sheet. A file copy, not an undo step.
    /// </summary>
    /// <param name="paths">The images chosen in the file dialog.</param>
    public void ImportSheetImages(IReadOnlyList<string> paths)
    {
        if (Content is null || Painting is null)
        {
            return;
        }

        var result = new SheetImport(_writer, Content).Import(paths, file => _confirm($"{file} is already in the tilesheet art. Replace it?"));
        foreach (var file in result.Imported)
        {
            _sheetImageSizes.Remove(file);
        }

        if (result.LastImported is { } last)
        {
            SheetImagesImported?.Invoke(this, result.Imported);
            Painting.Browser.Show(last);
        }

        ImportStatus = ImportStatusOf(result);
    }

    // "Imported A.png, B.png. Refused: C.png: not a PNG image."
    private static string ImportStatusOf(SheetImportResult result)
    {
        var refusals = result.Refusals.Select(refusal => $"Refused: {refusal}");
        if (result.Imported.Count == 0)
        {
            return string.Join(" ", refusals);
        }

        return string.Join(" ", refusals.Prepend($"Imported {string.Join(", ", result.Imported)}."));
    }

    // Read from the PNG header once per file; the art check has already found every sheet image readable.
    private (int Width, int Height) SheetImageSizeOf(string file)
    {
        if (!_sheetImageSizes.TryGetValue(file, out var size))
        {
            size = PngSize.Read(Path.Combine(ArtRoot, ArtFolders.Tilesheets, file));
            _sheetImageSizes.Add(file, size);
        }

        return size;
    }

    private void OnContentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogDraft.IsDirty))
        {
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
    }

    private void OnOpenMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.Map))
        {
            RefreshLayerEntries();
        }
        else if (e.PropertyName == nameof(MapViewModel.IsDirty))
        {
            OnPropertyChanged(nameof(HasUnsavedChanges));
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
