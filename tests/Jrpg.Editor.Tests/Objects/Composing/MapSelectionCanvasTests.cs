using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects.Composing;

namespace Jrpg.Editor.Tests.Objects.Composing;

/// <summary>
/// The canvas a map selection makes (decision 514), on layouts built in code: tile 1 is grass on the "ground" sheet at
/// (0, 0), tile 2 a trunk on the "trees" sheet at (3, 4), tile 3 a leaf on "trees" at (5, 1).
/// </summary>
public sealed class MapSelectionCanvasTests
{
    private const int Grass = 1;
    private const int Trunk = 2;
    private const int Leaf = 3;

    private static readonly IReadOnlyDictionary<int, TileDefinition> _tiles = new Dictionary<int, TileDefinition>
    {
        [Grass] = new() { Id = Grass, IsWalkable = true, SheetId = "ground", Column = 0, Row = 0 },
        [Trunk] = new() { Id = Trunk, IsWalkable = false, SheetId = "trees", Column = 3, Row = 4 },
        [Leaf] = new() { Id = Leaf, IsWalkable = true, SheetId = "trees", Column = 5, Row = 1 },
    };

    private static readonly MapCellRectangle _twoByTwo = new(new GridCell(0, 0), 2, 2);

    [Fact]
    public void TakenLayersOf_BaseAndOneOver_AreLevelsZeroAndOne()
    {
        var layout = Layout(Layer(2, Leaf), Layer(1, Trunk));

        Assert.Equal([(1, 0), (2, 1)], MapSelectionCanvas.TakenLayersOf(layout, _twoByTwo, baseZ: 1));
    }

    // z 0 and z -2 under the base z 1, listed out of z order: z 0 is the nearest, so level -1.
    [Fact]
    public void TakenLayersOf_TwoLayersUnderTheBase_AreMinusOneThenMinusTwoFromTheNearest()
    {
        var layout = Layout(Layer(0, Grass), Layer(1, Trunk), Layer(-2, Grass));

        Assert.Equal([(-2, -2), (0, -1), (1, 0)], MapSelectionCanvas.TakenLayersOf(layout, _twoByTwo, baseZ: 1));
    }

    [Fact]
    public void TakenLayersOf_LayerWithNoTileInTheSelection_TakesNoLevel()
    {
        var layout = Layout(Layer(1, Trunk), Layer(2, 0), Layer(3, Leaf));

        Assert.Equal([(1, 0), (3, 1)], MapSelectionCanvas.TakenLayersOf(layout, _twoByTwo, baseZ: 1));
    }

    [Fact]
    public void TakenLayersOf_BaseEmptyInTheSelection_StaysLevelZero()
    {
        var layout = Layout(Layer(1, 0), Layer(2, Leaf));

        Assert.Equal([(1, 0), (2, 1)], MapSelectionCanvas.TakenLayersOf(layout, _twoByTwo, baseZ: 1));
    }

    // The developer's case: grass under a tree, the tree's layer chosen.
    [Fact]
    public void Of_GroundUnderTheBase_IsOnLevelMinusOne()
    {
        var layout = Layout(Layer(0, Grass), Layer(1, Trunk));

        var canvas = MapSelectionCanvas.Of(layout, _twoByTwo, baseZ: 1, _tiles);

        Assert.Equal(new SheetCell("ground", 0, 0), canvas.Cells[new CellOnLevel(new GridCell(0, 0), -1)]);
        Assert.Equal(new SheetCell("trees", 3, 4), canvas.Cells[new CellOnLevel(new GridCell(0, 0), 0)]);
    }

    [Fact]
    public void Of_EachCell_IsItsTilesSheetCell()
    {
        var layout = Layout(Layer(1, Trunk), Layer(2, Leaf));

        var canvas = MapSelectionCanvas.Of(layout, _twoByTwo, baseZ: 1, _tiles);

        Assert.Equal(new SheetCell("trees", 5, 1), canvas.Cells[new CellOnLevel(new GridCell(1, 1), 1)]);
    }

    [Fact]
    public void Of_EmptyMapCell_StaysEmptyOnTheCanvas()
    {
        var layout = Layout(new TileLayer { Z = 1, Rows = [[Trunk, 0, 0], [0, 0, 0], [0, 0, 0]] });

        var canvas = MapSelectionCanvas.Of(layout, _twoByTwo, baseZ: 1, _tiles);

        Assert.Equal([new CellOnLevel(new GridCell(0, 0), 0)], canvas.Cells.Keys);
    }

    // A rectangle away from the corner: its cells land from the canvas's own (0, 0).
    [Fact]
    public void Of_Rectangle_GivesTheCanvasItsSizeAndPlaces()
    {
        var layout = Layout(Layer(1, Trunk));

        var canvas = MapSelectionCanvas.Of(layout, new MapCellRectangle(new GridCell(1, 1), 2, 1), baseZ: 1, _tiles);

        Assert.Equal((2, 1), (canvas.Width, canvas.Height));
        Assert.Equal([new GridCell(0, 0), new GridCell(1, 0)], canvas.Cells.Keys.Select(key => key.Cell).OrderBy(cell => cell.X));
    }

    [Fact]
    public void ProblemOf_NoTileOnAnyLayer_SaysSo()
    {
        Assert.Equal("No tile in the selection.", MapSelectionCanvas.ProblemOf(Layout(Layer(1, 0)), _twoByTwo, baseZ: 1));
    }

    [Fact]
    public void ProblemOf_ThirtyThreeColumns_SaysTheLimit()
    {
        var problem = MapSelectionCanvas.ProblemOf(Layout(40, 40, Layer(1, Trunk, 40)), new MapCellRectangle(new GridCell(0, 0), 33, 2), baseZ: 1);

        Assert.Equal("The selection is 33 x 2 cells; an object is at most 32 each way.", problem);
    }

    [Fact]
    public void ProblemOf_ThirtyThreeRows_SaysTheLimit()
    {
        var problem = MapSelectionCanvas.ProblemOf(Layout(40, 40, Layer(1, Trunk, 40)), new MapCellRectangle(new GridCell(0, 0), 2, 33), baseZ: 1);

        Assert.Equal("The selection is 2 x 33 cells; an object is at most 32 each way.", problem);
    }

    [Fact]
    public void ProblemOf_ThirtyTwoByThirtyTwo_IsNull()
    {
        Assert.Null(MapSelectionCanvas.ProblemOf(Layout(40, 40, Layer(1, Trunk, 40)), new MapCellRectangle(new GridCell(0, 0), 32, 32), baseZ: 1));
    }

    [Fact]
    public void ProblemOf_EightLayersOverTheBase_SaysTheLimit()
    {
        var layout = Layout([.. Enumerable.Range(0, 9).Select(z => Layer(z, Leaf))]);

        Assert.Equal("The selection has tiles on 8 layers over z 0; an object has 7 levels over its base.", MapSelectionCanvas.ProblemOf(layout, _twoByTwo, baseZ: 0));
    }

    [Fact]
    public void ProblemOf_EightLayersUnderTheBase_SaysTheLimit()
    {
        var layout = Layout([.. Enumerable.Range(-8, 9).Select(z => Layer(z, Grass))]);

        Assert.Equal("The selection has tiles on 8 layers under z 0; an object has 7 levels under its base.", MapSelectionCanvas.ProblemOf(layout, _twoByTwo, baseZ: 0));
    }

    [Fact]
    public void ProblemOf_SevenUnderAndSevenOver_IsNull()
    {
        var layout = Layout([.. Enumerable.Range(-7, 15).Select(z => Layer(z, Grass))]);

        Assert.Null(MapSelectionCanvas.ProblemOf(layout, _twoByTwo, baseZ: 0));
    }

    [Fact]
    public void DrawingWarningOf_LayerAtAbovePlayerZ_Warns()
    {
        var warning = MapSelectionCanvas.DrawingWarningOf([(MapLayers.PlayerZ, 0), (MapLayers.AbovePlayerZ, 1)], MapLayers.PlayerZ);

        Assert.Equal($"Layer z {MapLayers.AbovePlayerZ} drew over the player; in an object it draws by depth.", warning);
    }

    [Fact]
    public void DrawingWarningOf_LayerJustUnderAbovePlayerZ_IsNull()
    {
        Assert.Null(MapSelectionCanvas.DrawingWarningOf([(MapLayers.PlayerZ, 0), (MapLayers.AbovePlayerZ - 1, 1)], MapLayers.PlayerZ));
    }

    [Fact]
    public void DrawingWarningOf_BaseUnderPlayerZ_Warns()
    {
        var warning = MapSelectionCanvas.DrawingWarningOf([(MapLayers.PlayerZ - 1, 0)], MapLayers.PlayerZ - 1);

        Assert.Equal($"The base layer z {MapLayers.PlayerZ - 1} is under the player: the object will always draw under the player.", warning);
    }

    [Fact]
    public void DrawingWarningOf_BothApply_JoinsThem()
    {
        var warning = MapSelectionCanvas.DrawingWarningOf([(MapLayers.PlayerZ - 1, 0), (MapLayers.AbovePlayerZ, 1)], MapLayers.PlayerZ - 1);

        Assert.Equal(
            $"Layer z {MapLayers.AbovePlayerZ} drew over the player; in an object it draws by depth. The base layer z {MapLayers.PlayerZ - 1} is under the player: the object will always draw under the player.",
            warning);
    }

    [Fact]
    public void DrawingWarningOf_BaseAtPlayerZ_IsNull()
    {
        Assert.Null(MapSelectionCanvas.DrawingWarningOf([(MapLayers.PlayerZ, 0)], MapLayers.PlayerZ));
    }

    [Fact]
    public void Of_WithAProblem_Throws()
    {
        var thrown = Assert.Throws<ArgumentException>(() => MapSelectionCanvas.Of(Layout(Layer(1, 0)), _twoByTwo, baseZ: 1, _tiles));

        Assert.StartsWith("No tile in the selection.", thrown.Message, StringComparison.Ordinal);
    }

    private static MapLayout Layout(params TileLayer[] layers) => Layout(3, 3, layers);

    private static MapLayout Layout(int width, int height, params TileLayer[] layers) =>
        new() { Id = "glade", Width = width, Height = height, Layers = layers, Objects = [] };

    // A square layer of one tile everywhere (0 for an empty one).
    private static TileLayer Layer(int z, int tileId, int size = 3) =>
        new() { Z = z, Rows = [.. Enumerable.Range(0, size).Select(_ => (IReadOnlyList<int>)[.. Enumerable.Repeat(tileId, size)])] };
}
