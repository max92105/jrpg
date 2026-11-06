using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Audio;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Maps.Settings;
using Jrpg.Editor.Objects;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Quests;
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
    private readonly string _audioRoot;
    private readonly Func<string, bool> _confirm;
    private readonly SheetImageSizes _sheetImageSizes;

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
        _audioRoot = AudioFolders.RootIn(contentRoot);
        _sheetImageSizes = new SheetImageSizes(ArtRoot);
        try
        {
            // Both or neither: the window reads the art status whenever the content is there.
            var content = new CatalogDraft(ContentCatalogLoader.Load(contentRoot));
            Art = new ArtStatus(content, ArtRoot);
            Content = content;
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            LoadError = exception.Message;
        }

        // Maps, tiles and objects show whenever the content loaded: missing art is a warning, and the drawing marks
        // what it can't show (decision 442).
        if (Content is not null)
        {
            ShowMapList(Content.Current);
            var images = new SheetImages(() => [.. ArtFolders.TilesheetImagesIn(ArtRoot).Where(_sheetImageSizes.IsReadable)], _sheetImageSizes.SizeOf);
            Painting = new TilePaintingViewModel(Content, Layer, images, confirm);
            Marking = new WalkabilityMarking(Content, Painting.Browser);
            Animating = new TileAnimationViewModel(Content, Painting.Browser, images);
            AnimationTime = new TileAnimationTime(Content);
            Removal = new SheetRemoval(Content, Painting.Browser, () => CurrentMap, _writer);
            Content.PropertyChanged += OnContentChanged;
            _openMaps.DirtyChanged += OnOpenMapsDirtyChanged;
            Content.Rewriting += (_, written) => ShowRewritten(written);
            History = new CombinedHistory(Content);
            Placing = new ObjectPlacingViewModel(Content, Layer);
            Composing = new ObjectComposing(Content, Placing.Browser, Painting.Browser);
            AnimationImport = new AnimationImport(Content);
            EventEditing = new EventEditingViewModel(Content) { MapIds = MapIds };
            QuestEditing = new QuestEditingViewModel(Content);
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

    /// <summary>
    /// The art problems, as a warning, and what the drawing can't show; null when the content didn't load.
    /// </summary>
    public ArtStatus? Art { get; }

    /// <summary>The open map's layers, bottom first, and the chosen Z marked empty when the map has nothing there yet.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<LayerEntry> LayerEntries { get; private set; } = [];

    /// <summary>The layer tiles are painted and objects placed on.</summary>
    public LayerChoiceViewModel Layer { get; } = new();

    /// <summary>Undo and Redo over the open map and the catalogs; null when the content or its art didn't load.</summary>
    public CombinedHistory? History { get; }

    /// <summary>Null when the content or its art didn't load.</summary>
    public TilePaintingViewModel? Painting { get; }

    /// <summary>Null when the content or its art didn't load.</summary>
    public WalkabilityMarking? Marking { get; }

    /// <summary>The Animate panel (decision 467); null when the content didn't load.</summary>
    public TileAnimationViewModel? Animating { get; }

    /// <summary>The editor's animation time, moved on by the window's render tick (decision 470).</summary>
    public TileAnimationTime? AnimationTime { get; }

    /// <summary>Remove sheet in the Tiles panel (decision 443); null when the content didn't load.</summary>
    public SheetRemoval? Removal { get; }

    /// <summary>Objects > New object and Edit object (decision 455); null when the content didn't load.</summary>
    public ObjectComposing? Composing { get; }

    /// <summary>Tiles > Import animations (decision 469); null when the content didn't load.</summary>
    public AnimationImport? AnimationImport { get; }

    /// <summary>Null when the content or its art didn't load.</summary>
    public ObjectPlacingViewModel? Placing { get; }

    /// <summary>Null when the content or its art didn't load.</summary>
    public EventEditingViewModel? EventEditing { get; }

    /// <summary>Null when the content or its art didn't load.</summary>
    public QuestEditingViewModel? QuestEditing { get; }

    /// <summary>Whether the Quests menu can open its window: false while the content didn't load.</summary>
    public bool HasQuestEditing => QuestEditing is not null;

    [ObservableProperty]
    public partial MapViewModel? CurrentMap { get; private set; }

    /// <summary>Why the last save was refused, in the loader's words; null after a successful save.</summary>
    [ObservableProperty]
    public partial string? SaveError { get; private set; }

    /// <summary>Whether the cell grid is drawn over the map; on by default.</summary>
    [ObservableProperty]
    public partial bool IsGridShown { get; set; } = true;

    /// <summary>Whether blocked cells, collision outlines and above-player zones are drawn over the map; off by default.</summary>
    [ObservableProperty]
    public partial bool IsCollisionShown { get; set; }

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

    /// <summary>Whether Objects > New object can open the composer: once the content loaded.</summary>
    public bool CanStartNewObject => Composing is not null;

    /// <summary>Whether a new NPC can be made: only once the content and its art loaded.</summary>
    public bool CanStartNewNpc => Placing is not null;

    /// <summary>The New NPC dialog's state, listing the field sprites on disk now; null when the content didn't load.</summary>
    public NewNpcViewModel? StartNewNpc() =>
        CanStartNewNpc ? new NewNpcViewModel([.. Catalog!.Objects.Keys], ArtFolders.FieldSpriteImagesIn(ArtRoot)) : null;

    /// <summary>Makes the NPC the New NPC dialog describes, as one undo step; it is chosen in the Objects panel, in Objects mode.</summary>
    /// <returns>False when nothing was made: the id or sprite has a problem.</returns>
    public bool CreateNpc(NewNpcViewModel newNpc)
    {
        if (newNpc.IdProblem is not null || newNpc.SpriteProblem is not null || Content is null || Placing is null)
        {
            return false;
        }

        AddAndChoose(newNpc.ToDefinition());
        return true;
    }

    /// <summary>Whether the editor may close: nothing unsaved, or the user agreed, when asked, to drop it.</summary>
    public bool ConfirmClose()
    {
        // A stroke still being dragged counts as a change, so it ends before the question.
        CurrentMap?.EndStroke();
        return UnsavedChangesWording() is not { } unsaved || _confirm($"{unsaved} unsaved changes. Discard them?");
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
        CursorStatus = CurrentMap.TopTileAt(at) is { } tile
            ? $"{position}   Tile {tile.TileId} on layer z {tile.LayerZ}{Art!.NoteFor(tile.TileId)}"
            : $"{position}   No tile";
    }

    /// <summary>Says in the status bar why a click on the map painted nothing: no cell is chosen in the Tiles panel.</summary>
    public void ShowPaintHint() => CursorStatus = "Choose a tile in the Tiles panel";

    /// <summary>The settings dialog's state for the open map; null when no map is open.</summary>
    public MapSettingsViewModel? StartEditingSettings() =>
        CurrentMap is { } open ? new(open.Map, Catalog!.Encounters.Keys, AudioFolders.TracksIn(_audioRoot)) : null;

    /// <summary>
    /// Applies the settings dialog to the open map: the new size first, asking before it drops anything, then the name,
    /// the track and the random battles. Each is one undo step.
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
        Art!.Refresh();
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
            _sheetImageSizes.Forget(file);
        }

        if (result.LastImported is { } last)
        {
            Art!.Refresh();
            SheetImagesImported?.Invoke(this, result.Imported);
            Painting.Browser.Show(last);
        }

        ImportStatus = result.Summary;
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

    // The new object joins the catalog as one undo step, then is chosen in the Objects panel, in Objects mode.
    private void AddAndChoose(ObjectDefinition definition)
    {
        Content!.SetObject(definition);
        Placing!.Browser.SelectedObject = definition;
        Mode = EditingMode.Objects;
    }
}
