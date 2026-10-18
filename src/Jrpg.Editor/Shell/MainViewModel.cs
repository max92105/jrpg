using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Objects;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The editor window's state: the loaded content, the maps to choose from and the open one. Content that can't load is
/// shown with the loader's message instead of crashing, so a broken file can be found and fixed.
/// </summary>
internal sealed partial class MainViewModel : ObservableObject
{
    // Legacy brush sizes, in cells; [ and ] step through them.
    private static readonly int[] _brushSizes = [1, 2, 3, 4, 5, 7, 9];

    private readonly ContentWriter _writer;
    private readonly Func<string, bool> _confirmDiscard;

    /// <param name="contentRoot">The content folder to edit.</param>
    /// <param name="confirmDiscard">Asks the user whether to drop unsaved changes, given the question; true to drop them.</param>
    public MainViewModel(string contentRoot, Func<string, bool> confirmDiscard)
    {
        _writer = new ContentWriter(contentRoot);
        _confirmDiscard = confirmDiscard;
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

        // Maps and tiles only show when their content and art are whole, since drawing reads both.
        MapIds = Catalog is not null && LoadError is null ? [.. Catalog.Maps.Keys.Order(StringComparer.Ordinal)] : [];
        Tiles = Catalog is not null && LoadError is null ? new TileBrowserViewModel(Catalog) : null;
        Objects = Catalog is not null && LoadError is null ? new ObjectBrowserViewModel(Catalog.Objects) : null;
        if (MapIds.Count > 0)
        {
            TryOpenMap(MapIds[0]);
        }
    }

    public string ArtRoot { get; }

    /// <summary>The content as last loaded or saved; a save puts the saved map in it, so later saves check against it.</summary>
    public ContentCatalog? Catalog { get; private set; }

    /// <summary>Why the content can't be edited, naming the file; null when it loaded.</summary>
    public string? LoadError { get; }

    public IReadOnlyList<string> MapIds { get; }

    /// <summary>The tile to paint; null when the content or its art didn't load.</summary>
    public TileBrowserViewModel? Tiles { get; }

    /// <summary>The object to place; null when the content or its art didn't load.</summary>
    public ObjectBrowserViewModel? Objects { get; }

    public IReadOnlyList<LayerPreset> LayerPresets { get; } = Maps.LayerPresets.All;

    public IReadOnlyList<int> BrushSizes { get; } = _brushSizes;

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

    /// <summary>The layer painted and erased on, picked from <see cref="LayerPresets"/> or typed in; the ground by default, as in the legacy editor.</summary>
    [ObservableProperty]
    public partial int LayerZ { get; set; } = Maps.LayerPresets.GroundZ;

    /// <summary>The brush's width and height in cells, one of <see cref="BrushSizes"/>.</summary>
    [ObservableProperty]
    public partial int BrushSize { get; set; } = 1;

    /// <summary>Whether clicks on the map paint tiles or place objects.</summary>
    [ObservableProperty]
    public partial EditingMode Mode { get; set; }

    /// <summary>The placed objects chosen with a dragged rectangle, for Delete; emptied when another map opens, and kept to objects still on the map.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> SelectedObjectUids { get; private set; } = [];

    /// <summary>What a left click or drag on the map does; the brush by default.</summary>
    [ObservableProperty]
    public partial TileTool Tool { get; set; }

    public IReadOnlyList<TileTool> Tools { get; } = Enum.GetValues<TileTool>();

    /// <summary>Whether everything off <see cref="LayerZ"/> is drawn faint.</summary>
    [ObservableProperty]
    public partial bool IsOtherLayersDimmed { get; set; }

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

    /// <summary>Starts painting the chosen tile on <see cref="LayerZ"/>; nothing happens until a tile is chosen.</summary>
    public void StartPainting(GridCell cell)
    {
        if (Tiles?.SelectedTileId is { } tileId)
        {
            CurrentMap?.StartStroke(new TileBrush(LayerZ, tileId, BrushSize), cell);
        }
    }

    public void StartErasing(GridCell cell) => CurrentMap?.StartStroke(new TileBrush(LayerZ, TileLayer.EmptyTileId, BrushSize), cell);

    public void ContinueStroke(GridCell cell) => CurrentMap?.ContinueStroke(cell);

    public void EndStroke() => CurrentMap?.EndStroke();

    /// <summary>Paints the chosen tile on every cell of a rectangle, as one undo step.</summary>
    public void PaintRectangle(GridCell corner, GridCell oppositeCorner) => PutChosenTile(BrushCells.Rectangle(corner, oppositeCorner));

    public void EraseRectangle(GridCell corner, GridCell oppositeCorner) => PutTiles(TileLayer.EmptyTileId, BrushCells.Rectangle(corner, oppositeCorner));

    /// <summary>Paints the chosen tile over the area of the same tile joined to a cell on <see cref="LayerZ"/>.</summary>
    public void FloodFill(GridCell start)
    {
        if (CurrentMap is { } open)
        {
            PutChosenTile(TileFloodFill.Cells(open.Map.Layout, LayerZ, start));
        }
    }

    /// <summary>The eyedropper: chooses the tile seen at a cell and its layer, to paint more of it.</summary>
    public void PickTileAt(GridCell cell)
    {
        if (CurrentMap?.TopTileAt(cell) is { } tile)
        {
            Tiles?.Select(tile.TileId);
            LayerZ = tile.LayerZ;
        }
    }

    public void GrowBrush() => StepBrushSize(1);

    public void ShrinkBrush() => StepBrushSize(-1);

    /// <summary>Places the chosen object centered on a point, on <see cref="LayerZ"/>, as in the legacy editor. A point off the map
    /// places nothing, since the map is drawn clipped and the object couldn't be seen.</summary>
    /// <param name="center">In map cells; positions aren't snapped to the grid.</param>
    public void PlaceObjectAt(Point2D center)
    {
        if (CurrentMap is not { } open || Objects?.SelectedObject is not { } definition || !open.Map.Layout.Contains(GridCell.Containing(center)))
        {
            return;
        }

        var takenUids = open.Map.Layout.Objects.Select(placed => placed.Uid).ToHashSet();
        open.Do(new PlaceObjectEdit(new PlacedObject
        {
            Uid = PlacedObjectUids.Next(definition.Id, takenUids),
            ObjectId = definition.Id,
            Position = new Point2D(center.X - (definition.Width / 2.0), center.Y - (definition.Height / 2.0)),
            Z = LayerZ,
        }));
    }

    /// <summary>The object drawn on top at a point, to move or remove; null when none is there.</summary>
    /// <param name="point">In map cells.</param>
    public PlacedObject? ObjectAt(Point2D point) =>
        CurrentMap is { } open ? PlacedObjectHits.TopAt(open.Map.Layout, point, Catalog!.Objects) : null;

    /// <param name="uid">The placed object to move.</param>
    /// <param name="to">Its new top-left corner, in map cells.</param>
    public void MoveObject(string uid, Point2D to)
    {
        var placed = CurrentMap?.Map.Layout.Objects.FirstOrDefault(candidate => candidate.Uid == uid);
        if (placed is not null && placed.Position != to)
        {
            CurrentMap!.Do(new MoveObjectEdit(uid, placed.Position, to));
        }
    }

    public void RemoveObject(string uid) => RemoveObjects([uid]);

    /// <summary>Selects the objects overlapping the rectangle between two corners, in map cells.</summary>
    public void SelectObjectsIn(Point2D corner, Point2D oppositeCorner) =>
        SelectedObjectUids = CurrentMap is { } open
            ? [.. PlacedObjectHits.InRectangle(open.Map.Layout, corner, oppositeCorner, Catalog!.Objects).Select(placed => placed.Uid)]
            : [];

    /// <summary>Removes the selected objects as one undo step.</summary>
    public void DeleteSelectedObjects()
    {
        RemoveObjects(SelectedObjectUids);
        SelectedObjectUids = [];
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
        CurrentMap.MarkSaved();
        SaveError = null;
    }

    /// <summary>Paints the chosen tile on every cell of <see cref="LayerZ"/>, as one undo step.</summary>
    [RelayCommand]
    private void FillLayer() => PutChosenTile(AllCellsOfCurrentMap());

    [RelayCommand]
    private void ClearLayer() => PutTiles(TileLayer.EmptyTileId, AllCellsOfCurrentMap());

    private IEnumerable<GridCell> AllCellsOfCurrentMap() =>
        CurrentMap?.Map.Layout is { } layout ? BrushCells.Rectangle(new GridCell(0, 0), new GridCell(layout.Width - 1, layout.Height - 1)) : [];

    // Nothing happens until a tile is chosen.
    private void PutChosenTile(IEnumerable<GridCell> cells)
    {
        if (Tiles?.SelectedTileId is { } tileId)
        {
            PutTiles(tileId, cells);
        }
    }

    private void PutTiles(int tileId, IEnumerable<GridCell> cells) => CurrentMap?.PutTiles(LayerZ, tileId, cells);

    partial void OnCurrentMapChanged(MapViewModel? oldValue, MapViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnOpenMapChanged;
        newValue?.PropertyChanged += OnOpenMapChanged;
        SelectedObjectUids = [];
    }

    // An undo can remove a selected object, and its uid can be given to the next object placed, which mustn't start out selected.
    private void OnOpenMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MapViewModel.Map) || CurrentMap is not { } open)
        {
            return;
        }

        var onMap = open.Map.Layout.Objects.Select(placed => placed.Uid).ToHashSet();
        if (SelectedObjectUids.Any(uid => !onMap.Contains(uid)))
        {
            SelectedObjectUids = [.. SelectedObjectUids.Where(onMap.Contains)];
        }
    }

    private void RemoveObjects(IReadOnlyCollection<string> uids)
    {
        if (CurrentMap is not { } open)
        {
            return;
        }

        var edit = new RemoveObjectsEdit(open.Map.Layout.Objects, uids);
        if (edit.Count > 0)
        {
            open.Do(edit);
        }
    }

    private void StepBrushSize(int steps)
    {
        var index = Math.Clamp(Array.IndexOf(_brushSizes, BrushSize) + steps, 0, _brushSizes.Length - 1);
        BrushSize = _brushSizes[index];
    }

    // A stroke still being dragged counts as a change, so it ends before the question.
    private bool ConfirmLeaveCurrentMap()
    {
        CurrentMap?.EndStroke();
        return CurrentMap is not { IsDirty: true } open
            || _confirmDiscard($"Map '{open.Map.Id}' has unsaved changes. Discard them?");
    }
}
