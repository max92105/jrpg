using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class TilePaintingViewModelTests
{
    private static readonly string _contentRoot = Path.Combine(AppContext.BaseDirectory, "content");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_contentRoot);

    private readonly CatalogDraft _draft = new(_catalog);
    private readonly LayerChoiceViewModel _layer = new();
    private readonly TilePaintingViewModel _painting;

    public TilePaintingViewModelTests()
    {
        _painting = new TilePaintingViewModel(_draft, _layer, new SheetImages(ArtFolders.TilesheetImagesIn(ArtFolders.RootIn(_contentRoot)), _ => (1000, 1000)), _ => true) { OpenMap = new MapViewModel(_catalog.Maps["forest"]) };
    }

    private MapViewModel Map => _painting.OpenMap!;

    [Fact]
    public void StartPainting_TileChosen_PaintsItOnTheChosenLayerAsOneStep()
    {
        _painting.PickTileAt(new GridCell(0, 0));
        var tileId = _painting.Browser.SelectedTileId;
        _layer.Z = -3;

        _painting.StartPainting(new GridCell(4, 4));
        _painting.ContinueStroke(new GridCell(5, 4));
        _painting.EndStroke();

        var layer = Map.Layers.Single(layer => layer.Z == -3);
        Assert.Equal((tileId, tileId), (layer.TileIdAt(new GridCell(4, 4)), layer.TileIdAt(new GridCell(5, 4))));
        Assert.True(Map.IsDirty);
    }

    [Fact]
    public void StartPainting_NoTileChosen_ChangesNothing()
    {
        _painting.StartPainting(new GridCell(4, 4));
        _painting.EndStroke();

        Assert.False(Map.IsDirty);
    }

    [Fact]
    public void StartPainting_CellNotATile_AddsTileAndPaintsIt()
    {
        var tilesBefore = _draft.Current.Tiles.Count;
        ChooseCellNotATile();

        _painting.StartPainting(new GridCell(4, 4));
        _painting.EndStroke();

        var ground = Map.Layers.Single(layer => layer.Z == _layer.Z);
        Assert.Equal(tilesBefore + 1, _draft.Current.Tiles.Count);
        Assert.Equal(_draft.TileAt(_painting.Browser.SelectedCell!.Value)!.Id, ground.TileIdAt(new GridCell(4, 4)));
    }

    [Fact]
    public void StartPainting_CellAlreadyATile_AddsNoTile()
    {
        _painting.PickTileAt(new GridCell(0, 0));
        var tilesBefore = _draft.Current.Tiles.Count;

        _painting.StartPainting(new GridCell(4, 4));
        _painting.EndStroke();

        Assert.Equal(tilesBefore, _draft.Current.Tiles.Count);
        Assert.False(_draft.IsDirty);
    }

    [Fact]
    public void StartPainting_NoOpenMap_AddsNoTile()
    {
        ChooseCellNotATile();
        _painting.OpenMap = null;

        _painting.StartPainting(new GridCell(4, 4));

        Assert.False(_draft.IsDirty);
    }

    [Fact]
    public void FillLayerCommand_CellNotATile_AddsOneTile()
    {
        var tilesBefore = _draft.Current.Tiles.Count;
        ChooseCellNotATile();

        _painting.FillLayerCommand.Execute(null);

        Assert.Equal(tilesBefore + 1, _draft.Current.Tiles.Count);
    }

    [Fact]
    public void HasChoice_NothingChosen_IsFalse()
    {
        Assert.False(_painting.HasChoice);
    }

    [Fact]
    public void HasChoice_CellChosen_IsTrue()
    {
        _painting.Browser.Select(_catalog.Tiles.Keys.First());

        Assert.True(_painting.HasChoice);
    }

    [Fact]
    public void StartErasing_UsesTheChosenLayerAndBrushSize()
    {
        _painting.GrowBrush();

        _painting.StartErasing(new GridCell(4, 4));
        _painting.EndStroke();

        var ground = Map.Layers.Single(layer => layer.Z == LayerPresets.GroundZ);
        Assert.All([new GridCell(4, 4), new GridCell(5, 4), new GridCell(4, 5), new GridCell(5, 5)], cell =>
            Assert.Equal(TileLayer.EmptyTileId, ground.TileIdAt(cell)));
    }

    [Fact]
    public void PickTileAt_CellWithTiles_ChoosesTheTopTileAndItsLayer()
    {
        _painting.OpenMap = new MapViewModel(_catalog.Maps["start"]);
        var top = Map.TopTileAt(new GridCell(3, 2))!.Value;

        _painting.PickTileAt(new GridCell(3, 2));

        Assert.Equal((top.TileId, top.LayerZ), (_painting.Browser.SelectedTileId, _layer.Z));
    }

    [Fact]
    public void PickTileAt_CellWithoutTiles_KeepsTheChoice()
    {
        // The start map's one hole, left by the legacy editor (seen in slice 3a).
        _painting.OpenMap = new MapViewModel(_catalog.Maps["start"]);
        _layer.Z = -3;

        _painting.PickTileAt(new GridCell(5, 13));

        Assert.Equal(-3, _layer.Z);
        Assert.Null(_painting.Browser.SelectedTileId);
    }

    [Fact]
    public void PaintRectangle_TileChosen_PaintsEveryCellOfIt()
    {
        ChooseTopLeftTile(layerZ: -3);

        _painting.PaintRectangle(new GridCell(3, 2), new GridCell(1, 1));

        var layer = Map.Layers.Single(layer => layer.Z == -3);
        Assert.Equal(6, layer.Rows.SelectMany(row => row).Count(tileId => tileId == _painting.Browser.SelectedTileId));
    }

    [Fact]
    public void EraseRectangle_EmptiesItsCellsOnTheChosenLayer()
    {
        _painting.EraseRectangle(new GridCell(0, 0), new GridCell(1, 1));

        var ground = Map.Layers.Single(layer => layer.Z == LayerPresets.GroundZ);
        Assert.All(BrushCells.Rectangle(new GridCell(0, 0), new GridCell(1, 1)), cell => Assert.Equal(TileLayer.EmptyTileId, ground.TileIdAt(cell)));
    }

    [Fact]
    public void FloodFill_EmptyLayer_FillsTheWholeMapAsOneStep()
    {
        ChooseTopLeftTile(layerZ: -3);

        _painting.FloodFill(new GridCell(2, 2));

        var layer = Map.Layers.Single(layer => layer.Z == -3);
        Assert.All(layer.Rows.SelectMany(row => row), tileId => Assert.Equal(_painting.Browser.SelectedTileId, tileId));
        Map.UndoCommand.Execute(null);
        Assert.DoesNotContain(Map.Layers, layer => layer.Z == -3);
    }

    [Fact]
    public void FillLayerCommand_NoTileChosen_ChangesNothing()
    {
        _painting.FillLayerCommand.Execute(null);

        Assert.False(Map.IsDirty);
    }

    [Fact]
    public void ClearLayerCommand_RemovesTheLayer()
    {
        _painting.ClearLayerCommand.Execute(null);

        Assert.DoesNotContain(Map.Layers, layer => layer.Z == LayerPresets.GroundZ);
    }

    [Fact]
    public void GrowBrush_FromFive_SkipsToSeven()
    {
        _painting.BrushSize = 5;

        _painting.GrowBrush();

        Assert.Equal(7, _painting.BrushSize);
    }

    [Fact]
    public void ShrinkBrush_AtSmallest_StaysThere()
    {
        _painting.ShrinkBrush();

        Assert.Equal(1, _painting.BrushSize);
    }

    // The first cell of the shown sheet that isn't a tile yet; every sheet image is 1000 pixels square here.
    private void ChooseCellNotATile()
    {
        var sheet = _painting.Browser.SelectedSheet!;
        var cell = (from row in Enumerable.Range(0, sheet.RowsIn(1000))
                    from column in Enumerable.Range(0, sheet.ColumnsIn(1000))
                    select new SheetCell(sheet.Id, column, row)).First(cell => _draft.TileAt(cell) is null);
        _painting.Browser.SelectCellAt(sheet.CellLeft(cell.Column), sheet.CellTop(cell.Row));
    }

    // The forest's top-left tile, painted on the given layer.
    private void ChooseTopLeftTile(int layerZ)
    {
        _painting.PickTileAt(new GridCell(0, 0));
        _layer.Z = layerZ;
    }
}
