using Jrpg.Core.Geometry;
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
