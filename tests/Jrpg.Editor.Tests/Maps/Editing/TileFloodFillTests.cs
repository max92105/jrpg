using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class TileFloodFillTests
{
    private const int GroundZ = -5;
    private const int Grass = 1;
    private const int Wall = 9;
    private const int Empty = TileLayer.EmptyTileId;

    // Grass split by a wall column, with an empty pocket at the bottom left; the corner touch at (1, 2)-(2, 1) doesn't join.
    private static readonly MapLayout _yard = new()
    {
        Id = "yard",
        Width = 4,
        Height = 3,
        Layers =
        [
            new TileLayer
            {
                Z = GroundZ,
                Rows = [[Grass, Grass, Wall, Grass], [Grass, Grass, Wall, Grass], [Empty, Empty, Grass, Wall]],
            },
        ],
        Objects = [],
    };

    [Fact]
    public void Cells_StopsAtDifferentTile()
    {
        var cells = TileFloodFill.Cells(_yard, GroundZ, new GridCell(0, 0));

        Assert.Equal(
            [new GridCell(0, 0), new GridCell(1, 0), new GridCell(0, 1), new GridCell(1, 1)],
            cells.OrderBy(cell => cell.Y).ThenBy(cell => cell.X));
    }

    [Fact]
    public void Cells_EmptyCells_FillsConnectedEmptyArea()
    {
        var cells = TileFloodFill.Cells(_yard, GroundZ, new GridCell(1, 2));

        Assert.Equal([new GridCell(0, 2), new GridCell(1, 2)], cells.OrderBy(cell => cell.X));
    }

    [Fact]
    public void Cells_LayerTheMapLacks_FillsTheWholeMap()
    {
        Assert.Equal(12, TileFloodFill.Cells(_yard, -3, new GridCell(2, 1)).Count);
    }

    [Fact]
    public void Cells_SameIdAsStart_ReturnsRegionOnce()
    {
        var cells = TileFloodFill.Cells(_yard, -3, new GridCell(0, 0));

        Assert.Equal(cells.Count, cells.Distinct().Count());
        Assert.Equal(new GridCell(0, 0), cells[0]);
    }

    [Fact]
    public void Cells_StartOffTheMap_IsEmpty()
    {
        Assert.Empty(TileFloodFill.Cells(_yard, GroundZ, new GridCell(4, 0)));
    }
}
