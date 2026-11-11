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
using Jrpg.Editor.Objects;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Quests;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The editor window's state: the loaded content, the maps open in the session and the shown one (decision 497);
/// saving, switching, renaming, folders, settings, the sheet import and an object from the map are its partial files;
/// painting tiles, placing objects and editing their events are <see cref="Painting"/>, <see cref="Placing"/> and
/// <see cref="EventEditing"/>. Content that can't load is shown with the loader's message instead of crashing, so a
/// broken file can be found and fixed.
/// </summary>
internal sealed partial class MainViewModel : ObservableObject
{
    private readonly ContentWriter _writer;
    private readonly string _audioRoot;
    private readonly Func<string, bool> _confirm;
    private readonly Func<string, UnsavedChangesAnswer> _askToClose;
    private readonly SheetImageSizes _sheetImageSizes;

    /// <param name="contentRoot">The content folder to edit.</param>
    /// <param name="confirm">Asks the user a yes or no question, such as whether to resize; true for yes.</param>
    /// <param name="askToClose">Asks what to do with unsaved changes when the editor closes (decision 501).</param>
    public MainViewModel(string contentRoot, Func<string, bool> confirm, Func<string, UnsavedChangesAnswer> askToClose)
    {
        _writer = new ContentWriter(contentRoot);
        _confirm = confirm;
        _askToClose = askToClose;
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
            Painting = new TilePaintingViewModel(Content, Layer, new TileBrowserViewModel(Content, images, confirm, () => _openMaps.Layouts));
            Marking = new WalkabilityMarking(Content, Painting.Browser);
            Animating = new TileAnimationViewModel(Content, Painting.Browser, images);
            AnimationTime = new TileAnimationTime(Content);
            Removal = new SheetRemoval(Content, Painting.Browser, _openMaps, _writer);
            Content.PropertyChanged += OnContentChanged;
            _openMaps.DirtyChanged += OnOpenMapsDirtyChanged;
            Content.Rewriting += (_, written) => ShowRewritten(written);
            History = new CombinedHistory(Content, _openMaps);
            Placing = new ObjectPlacingViewModel(Content, Layer, _openMaps);
            Composing = new ObjectComposing(Content, Placing.Browser, Painting.Browser);
            AnimationImport = new AnimationImport(Content);
            EventEditing = new EventEditingViewModel(Content, _openMaps, Layer) { MapIds = MapIds };
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

    [RelayCommand]
    private void ToggleCollision() => IsCollisionShown = !IsCollisionShown;

    private void OnContentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogDraft.IsDirty))
        {
            RaiseUnsavedChanged();
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

    // The new object joins the catalog as one undo step, then is chosen in the Objects panel, in Objects mode.
    private void AddAndChoose(ObjectDefinition definition)
    {
        Content!.SetObject(definition);
        Placing!.Browser.SelectedObject = definition;
        Mode = EditingMode.Objects;
    }
}
