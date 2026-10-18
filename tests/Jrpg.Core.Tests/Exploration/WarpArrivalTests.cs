using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Exploration;

public sealed class WarpArrivalTests
{
    private static readonly Dictionary<int, TileDefinition> _tiles = new()
    {
        [ActiveMapBuilder.Floor] = new TileDefinition { Id = ActiveMapBuilder.Floor, IsWalkable = true },
        [ActiveMapBuilder.Wall] = new TileDefinition { Id = ActiveMapBuilder.Wall, IsWalkable = false },
    };

    [Fact]
    public void Find_WalkableTarget_KeepsExactPoint()
    {
        var grid = Grid("111", "111");

        var arrival = WarpArrival.Find(grid, new Point2D(1.2, 0.7));

        Assert.Equal(new Point2D(1.2, 0.7), arrival);
    }

    [Fact]
    public void Find_BlockedTarget_ReturnsCenterOfNearestWalkableCell()
    {
        var grid = Grid("22222", "22221", "22222");

        var arrival = WarpArrival.Find(grid, new Point2D(2.5, 1.5));

        Assert.Equal(new Point2D(4.5, 1.5), arrival);
    }

    [Fact]
    public void Find_EdgeAndCornerInSameRing_PrefersEdge()
    {
        // In the ring around (1, 1), the corner (0, 0) comes first in scan order but the edge (2, 1) is nearer.
        var grid = Grid("122", "221", "222");

        var arrival = WarpArrival.Find(grid, new Point2D(1.5, 1.5));

        Assert.Equal(new Point2D(2.5, 1.5), arrival);
    }

    [Fact]
    public void Find_TwoEqualEdges_FirstInScanOrderWins()
    {
        var grid = Grid("212", "222", "212");

        var arrival = WarpArrival.Find(grid, new Point2D(1.5, 1.5));

        Assert.Equal(new Point2D(1.5, 0.5), arrival);
    }

    [Fact]
    public void Find_TargetOffMapNextToWalkableEdge_LandsOnEdgeCell()
    {
        var grid = Grid("111", "111");

        var arrival = WarpArrival.Find(grid, new Point2D(7, -3));

        Assert.Equal(new Point2D(2.5, 0.5), arrival);
    }

    [Fact]
    public void Find_TargetOffMapNextToBlockedEdge_SearchesFromClampedCell()
    {
        var grid = Grid("112");

        var arrival = WarpArrival.Find(grid, new Point2D(9, 0.5));

        Assert.Equal(new Point2D(1.5, 0.5), arrival);
    }

    [Fact]
    public void Find_NothingWithinRadius_ReturnsClampedCellCenter()
    {
        var wallRow = new string('2', WarpArrival.SearchRadius + 2);
        var grid = Grid(wallRow[..^1] + "1");

        var arrival = WarpArrival.Find(grid, new Point2D(0.5, 0.5));

        Assert.Equal(new Point2D(0.5, 0.5), arrival);
    }

    private static WalkabilityGrid Grid(params string[] rows) =>
        WalkabilityGrid.Build(new MapLayoutBuilder().WithLayer(rows).Build(), _tiles);
}
