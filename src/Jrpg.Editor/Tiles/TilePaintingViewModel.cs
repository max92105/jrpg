using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Painting tiles on the open map: the chosen tile, tool and brush size, and what each tool does on the chosen layer.
/// Every change is one undo step; nothing is painted until a tile is chosen.
/// </summary>
internal sealed partial class TilePaintingViewModel : ObservableObject
{
    // Legacy brush sizes, in cells; [ and ] step through them.
    private static readonly int[] _brushSizes = [1, 2, 3, 4, 5, 7, 9];

    private readonly LayerChoiceViewModel _layer;

    public TilePaintingViewModel(CatalogDraft catalog, LayerChoiceViewModel layer)
    {
        Browser = new TileBrowserViewModel(catalog);
        _layer = layer;
    }

    public TileBrowserViewModel Browser { get; }

    public IReadOnlyList<int> BrushSizes { get; } = _brushSizes;

    public IReadOnlyList<TileTool> Tools { get; } = Enum.GetValues<TileTool>();

    /// <summary>The map painted on; set by the shell when a map opens.</summary>
    public MapViewModel? OpenMap { get; set; }

    /// <summary>The brush's width and height in cells, one of <see cref="BrushSizes"/>.</summary>
    [ObservableProperty]
    public partial int BrushSize { get; set; } = 1;

    /// <summary>What a left click or drag on the map does; the brush by default.</summary>
    [ObservableProperty]
    public partial TileTool Tool { get; set; }

    public void StartPainting(GridCell cell)
    {
        if (Browser.SelectedTileId is { } tileId)
        {
            OpenMap?.StartStroke(new TileBrush(_layer.Z, tileId, BrushSize), cell);
        }
    }

    public void StartErasing(GridCell cell) => OpenMap?.StartStroke(new TileBrush(_layer.Z, TileLayer.EmptyTileId, BrushSize), cell);

    public void ContinueStroke(GridCell cell) => OpenMap?.ContinueStroke(cell);

    public void EndStroke() => OpenMap?.EndStroke();

    public void PaintRectangle(GridCell corner, GridCell oppositeCorner) => PutChosenTile(BrushCells.Rectangle(corner, oppositeCorner));

    public void EraseRectangle(GridCell corner, GridCell oppositeCorner) => PutTiles(TileLayer.EmptyTileId, BrushCells.Rectangle(corner, oppositeCorner));

    /// <summary>Paints the chosen tile over the area of the same tile joined to a cell on the chosen layer.</summary>
    public void FloodFill(GridCell start)
    {
        if (OpenMap is { } open)
        {
            PutChosenTile(TileFloodFill.Cells(open.Map.Layout, _layer.Z, start));
        }
    }

    /// <summary>The eyedropper: chooses the tile seen at a cell and its layer, to paint more of it.</summary>
    public void PickTileAt(GridCell cell)
    {
        if (OpenMap?.TopTileAt(cell) is { } tile)
        {
            Browser.Select(tile.TileId);
            _layer.Z = tile.LayerZ;
        }
    }

    public void GrowBrush() => StepBrushSize(1);

    public void ShrinkBrush() => StepBrushSize(-1);

    /// <summary>Paints the chosen tile on every cell of the chosen layer.</summary>
    [RelayCommand]
    private void FillLayer() => PutChosenTile(AllCellsOfOpenMap());

    [RelayCommand]
    private void ClearLayer() => PutTiles(TileLayer.EmptyTileId, AllCellsOfOpenMap());

    private IEnumerable<GridCell> AllCellsOfOpenMap() =>
        OpenMap?.Map.Layout is { } layout ? BrushCells.Rectangle(new GridCell(0, 0), new GridCell(layout.Width - 1, layout.Height - 1)) : [];

    private void PutChosenTile(IEnumerable<GridCell> cells)
    {
        if (Browser.SelectedTileId is { } tileId)
        {
            PutTiles(tileId, cells);
        }
    }

    private void PutTiles(int tileId, IEnumerable<GridCell> cells) => OpenMap?.PutTiles(_layer.Z, tileId, cells);

    private void StepBrushSize(int steps)
    {
        var index = Math.Clamp(Array.IndexOf(_brushSizes, BrushSize) + steps, 0, _brushSizes.Length - 1);
        BrushSize = _brushSizes[index];
    }
}
