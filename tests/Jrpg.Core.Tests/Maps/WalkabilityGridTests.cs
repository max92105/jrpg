using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Maps;

public sealed class WalkabilityGridTests
{
    private const int Grass = 1;
    private const int Rock = 2;

    private static readonly Dictionary<int, TileDefinition> _tiles = new()
    {
        [Grass] = new TileDefinition { Id = Grass, IsWalkable = true },
        [Rock] = new TileDefinition { Id = Rock, IsWalkable = false },
    };

    [Fact]
    public void IsWalkable_AllTilesWalkable_ReturnsTrue()
    {
        var layout = new MapLayoutBuilder().WithLayer("11").WithLayer("01").Build();

        var grid = WalkabilityGrid.Build(layout, _tiles);

        Assert.True(grid.IsWalkable(new GridCell(0, 0)));
        Assert.True(grid.IsWalkable(new GridCell(1, 0)));
    }

    [Fact]
    public void IsWalkable_BlockingTileOnUpperLayer_ReturnsFalse()
    {
        var layout = new MapLayoutBuilder().WithLayer("11").WithLayer("20").Build();

        var grid = WalkabilityGrid.Build(layout, _tiles);

        Assert.False(grid.IsWalkable(new GridCell(0, 0)));
        Assert.True(grid.IsWalkable(new GridCell(1, 0)));
    }

    [Fact]
    public void IsWalkable_CellWithNoTile_ReturnsFalse()
    {
        var layout = new MapLayoutBuilder().WithLayer("01").WithLayer("00").Build();

        var grid = WalkabilityGrid.Build(layout, _tiles);

        Assert.False(grid.IsWalkable(new GridCell(0, 0)));
    }

    [Fact]
    public void IsWalkable_UnknownTileId_ReturnsFalse()
    {
        var layout = new MapLayoutBuilder().WithLayer("19").Build();

        var grid = WalkabilityGrid.Build(layout, _tiles);

        Assert.False(grid.IsWalkable(new GridCell(1, 0)));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(2, 0)]
    [InlineData(0, 3)]
    public void IsWalkable_OutsideMap_ReturnsFalse(int x, int y)
    {
        var layout = new MapLayoutBuilder().WithLayer("11", "11", "11").Build();

        var grid = WalkabilityGrid.Build(layout, _tiles);

        Assert.False(grid.IsWalkable(new GridCell(x, y)));
    }

    [Fact]
    public void IsWalkable_NonSquareMap_ReadsRowsByY()
    {
        var layout = new MapLayoutBuilder().WithLayer("112", "111").Build();

        var grid = WalkabilityGrid.Build(layout, _tiles);

        Assert.False(grid.IsWalkable(new GridCell(2, 0)));
        Assert.True(grid.IsWalkable(new GridCell(2, 1)));
    }
}
