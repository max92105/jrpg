using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>Objects > New object from map selection (decision 514), on a copy of the test world's forest.</summary>
public sealed class MainViewModelObjectFromMapTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void StartObjectFromMap_NoSelection_SaysToSelectAndReturnsNull()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.Null(viewModel.StartObjectFromMap());

        Assert.Equal("Ctrl + drag a rectangle of cells on the map first (Tiles tab).", viewModel.CursorStatus);
    }

    // A new map has no tile anywhere, so its selection can't make an object.
    [Fact]
    public void StartObjectFromMap_SelectionOfEmptyCells_SaysWhyAndReturnsNull()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";
        viewModel.TryCreateMap(newMap);
        viewModel.Painting!.SelectMapCells(new GridCell(0, 0), new GridCell(1, 1));

        Assert.Null(viewModel.StartObjectFromMap());

        Assert.Equal("No tile in the selection.", viewModel.CursorStatus);
    }

    [Fact]
    public void StartObjectFromMap_GroundSelectedAsBase_StartsTheCanvasWithTheMapsCells()
    {
        var (viewModel, groundTileId) = SelectTopLeftOnTheGround();

        var composer = viewModel.StartObjectFromMap()!;

        var expected = SheetCell.Of(viewModel.Catalog!.Tiles[groundTileId]);
        Assert.Equal(expected, composer.Painting.Canvas.Cells[new CellOnLevel(new GridCell(0, 0), 0)]);
        Assert.Equal((2, 2), (composer.Painting.Canvas.Width, composer.Painting.Canvas.Height));
    }

    // The forest's ground layer is under the player's z, so the object would always draw under the player.
    [Fact]
    public void StartObjectFromMap_BaseUnderThePlayer_CarriesTheWarning()
    {
        var (viewModel, _) = SelectTopLeftOnTheGround();

        var composer = viewModel.StartObjectFromMap()!;

        Assert.Contains("is under the player", composer.MapLayerWarning, StringComparison.Ordinal);
    }

    [Fact]
    public void StartObjectFromMap_Saved_MakesAnObjectOfTheMapsTilesInOneStep()
    {
        var (viewModel, groundTileId) = SelectTopLeftOnTheGround();
        var tileCount = viewModel.Catalog!.Tiles.Count;
        var composer = viewModel.StartObjectFromMap()!;
        (composer.Naming.Id, composer.Naming.Category) = ("forest_patch", "Ground");

        Assert.True(viewModel.Composing!.TryApply(composer));

        Assert.Contains(viewModel.Catalog.Objects["forest_patch"].Tiles, tile => tile is { X: 0, Y: 0, Level: 0 } && tile.TileId == groundTileId);
        Assert.Equal(tileCount, viewModel.Catalog.Tiles.Count);
        viewModel.History!.UndoCommand.Execute(null);
        Assert.False(viewModel.Catalog.Objects.ContainsKey("forest_patch"));
    }

    [Fact]
    public void TryApplyObjectFromMap_Replacing_ErasesTheBaseLayersCells()
    {
        var (viewModel, composer) = ComposeFromTwoLayers();

        Assert.True(viewModel.TryApplyObjectFromMap(composer));

        Assert.All(TopLeftCells(), cell => Assert.Equal(TileLayer.EmptyTileId, TileAt(viewModel, DecorationZ, cell)));
    }

    // The ground chosen as the base: the decoration over it is level 1, and its cells go too.
    [Fact]
    public void TryApplyObjectFromMap_GroundChosen_ErasesTheLayerOverItToo()
    {
        var (viewModel, _) = ComposeFromTwoLayers();
        viewModel.Layer.Z = GroundZ;
        var composer = viewModel.StartObjectFromMap()!;
        (composer.Naming.Id, composer.Naming.Category) = ("ground_box", "Plants");

        viewModel.TryApplyObjectFromMap(composer);

        Assert.All(TopLeftCells(), cell => Assert.Equal(TileLayer.EmptyTileId, TileAt(viewModel, GroundZ, cell)));
        Assert.All(TopLeftCells(), cell => Assert.Equal(TileLayer.EmptyTileId, TileAt(viewModel, DecorationZ, cell)));
    }

    // A z with no layer on the forest chosen as the base: both painted layers are under it, so nothing is erased and
    // the object is still placed there.
    [Fact]
    public void TryApplyObjectFromMap_BaseZWithNoLayer_ErasesNothingAndPlacesTheObject()
    {
        var (viewModel, _) = ComposeFromTwoLayers();
        var decoration = TopLeftCells().Select(cell => TileAt(viewModel, DecorationZ, cell)).ToList();
        viewModel.Layer.Z = 3;
        var composer = viewModel.StartObjectFromMap()!;
        (composer.Naming.Id, composer.Naming.Category) = ("high_box", "Plants");

        viewModel.TryApplyObjectFromMap(composer);

        Assert.Equal(decoration, TopLeftCells().Select(cell => TileAt(viewModel, DecorationZ, cell)));
        Assert.Equal(("high_box", 3), (viewModel.CurrentMap!.Map.Layout.Objects[^1].ObjectId, viewModel.CurrentMap.Map.Layout.Objects[^1].Z));
    }

    // The ground under the base (level -1) keeps its map tiles, so it never leaves a hole (decision 515).
    [Fact]
    public void TryApplyObjectFromMap_Replacing_LeavesTheLayersUnderTheBase()
    {
        var (viewModel, composer) = ComposeFromTwoLayers();
        var ground = TopLeftCells().Select(cell => TileAt(viewModel, GroundZ, cell)).ToList();

        viewModel.TryApplyObjectFromMap(composer);

        Assert.Equal(ground, TopLeftCells().Select(cell => TileAt(viewModel, GroundZ, cell)));
    }

    [Fact]
    public void TryApplyObjectFromMap_CellErasedInTheComposer_StaysOnTheMap()
    {
        var (viewModel, composer) = ComposeFromTwoLayers();
        var kept = TileAt(viewModel, DecorationZ, new GridCell(1, 1));
        composer.Painting.Level = 0;
        composer.Painting.EraseAt(new GridCell(1, 1));

        viewModel.TryApplyObjectFromMap(composer);

        Assert.Equal(kept, TileAt(viewModel, DecorationZ, new GridCell(1, 1)));
        Assert.Equal(TileLayer.EmptyTileId, TileAt(viewModel, DecorationZ, new GridCell(0, 0)));
    }

    // Another sheet cell stamped over (0, 0) on the base level: the map's tile isn't the object's there, so it stays.
    [Fact]
    public void TryApplyObjectFromMap_CellRepaintedInTheComposer_StaysOnTheMap()
    {
        var (viewModel, composer) = ComposeFromTwoLayers();
        var kept = TileAt(viewModel, DecorationZ, new GridCell(0, 0));
        _content.ChooseTwoCellsNotTiles(viewModel);
        composer.Painting.Level = 0;
        composer.Painting.PaintAt(new GridCell(0, 0));

        viewModel.TryApplyObjectFromMap(composer);

        Assert.Equal(kept, TileAt(viewModel, DecorationZ, new GridCell(0, 0)));
    }

    [Fact]
    public void TryApplyObjectFromMap_Replacing_PlacesTheObjectAtTheSelectionOnTheBaseLayer()
    {
        var (viewModel, composer) = ComposeFromTwoLayers();

        viewModel.TryApplyObjectFromMap(composer);

        var placed = viewModel.CurrentMap!.Map.Layout.Objects[^1];
        Assert.Equal(("leaf_box", new Point2D(0, 0), DecorationZ), (placed.ObjectId, placed.Position, placed.Z));
        Assert.Equal([placed.Uid], viewModel.Placing!.SelectedObjectUids);
        Assert.Equal(EditingMode.Objects, viewModel.Mode);
    }

    [Fact]
    public void TryApplyObjectFromMap_ThenUndoTwice_UndoesTheReplacementThenTheObject()
    {
        var (viewModel, composer) = ComposeFromTwoLayers();
        var decoration = TopLeftCells().Select(cell => TileAt(viewModel, DecorationZ, cell)).ToList();
        viewModel.TryApplyObjectFromMap(composer);

        viewModel.History!.UndoCommand.Execute(null);
        var objectAfterOneUndo = viewModel.Catalog!.Objects.ContainsKey("leaf_box");
        viewModel.History.UndoCommand.Execute(null);

        Assert.Equal(decoration, TopLeftCells().Select(cell => TileAt(viewModel, DecorationZ, cell)));
        Assert.True(objectAfterOneUndo);
        Assert.False(viewModel.Catalog.Objects.ContainsKey("leaf_box"));
        Assert.Null(viewModel.History.Refusal);
    }

    [Fact]
    public void TryApplyObjectFromMap_NotReplacing_LeavesTheMap()
    {
        var (viewModel, composer) = ComposeFromTwoLayers();
        var decoration = TopLeftCells().Select(cell => TileAt(viewModel, DecorationZ, cell)).ToList();
        composer.IsReplacingMapCells = false;

        viewModel.TryApplyObjectFromMap(composer);

        Assert.Equal(decoration, TopLeftCells().Select(cell => TileAt(viewModel, DecorationZ, cell)));
        Assert.DoesNotContain(viewModel.CurrentMap!.Map.Layout.Objects, placed => placed.ObjectId == "leaf_box");
    }

    [Fact]
    public void TryApplyObjectFromMap_ComposerWithAProblem_ChangesNothing()
    {
        var (viewModel, composer) = ComposeFromTwoLayers();
        composer.Naming.Id = "";
        var objects = viewModel.CurrentMap!.Map.Layout.Objects.Count;

        Assert.False(viewModel.TryApplyObjectFromMap(composer));

        Assert.Equal(objects, viewModel.CurrentMap.Map.Layout.Objects.Count);
        Assert.NotEqual(TileLayer.EmptyTileId, TileAt(viewModel, DecorationZ, new GridCell(0, 0)));
    }

    private const int GroundZ = -5;
    private const int DecorationZ = -4;

    private static GridCell[] TopLeftCells() => [new(0, 0), new(1, 0), new(0, 1), new(1, 1)];

    private static int TileAt(MainViewModel viewModel, int z, GridCell cell) =>
        viewModel.CurrentMap!.Map.Layout.Layers.Single(layer => layer.Z == z).TileIdAt(cell);

    // The forest's top-left two by two painted with one tile on the ground and another on the decoration layer,
    // selected with the decoration layer chosen, and the composer named "leaf_box".
    private (MainViewModel ViewModel, ObjectComposerViewModel Composer) ComposeFromTwoLayers()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var tileIds = viewModel.Catalog!.Tiles.Keys.Order().Take(2).ToList();
        viewModel.CurrentMap!.PutTiles(GroundZ, tileIds[0], TopLeftCells());
        viewModel.CurrentMap.PutTiles(DecorationZ, tileIds[1], TopLeftCells());
        viewModel.Layer.Z = DecorationZ;
        viewModel.Painting!.SelectMapCells(new GridCell(0, 0), new GridCell(1, 1));
        var composer = viewModel.StartObjectFromMap()!;
        (composer.Naming.Id, composer.Naming.Category) = ("leaf_box", "Plants");
        return (viewModel, composer);
    }

    // The forest's ground layer chosen, its two by two top-left cells selected.
    private (MainViewModel ViewModel, int GroundTileId) SelectTopLeftOnTheGround()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var ground = viewModel.CurrentMap!.Map.Layout.Layers.OrderBy(layer => layer.Z).First();
        viewModel.Layer.Z = ground.Z;
        viewModel.Painting!.SelectMapCells(new GridCell(0, 0), new GridCell(1, 1));
        return (viewModel, ground.TileIdAt(new GridCell(0, 0)));
    }
}
