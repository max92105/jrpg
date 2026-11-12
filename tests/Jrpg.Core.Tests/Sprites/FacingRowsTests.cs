using Jrpg.Core.Geometry;
using Jrpg.Core.Sprites;

namespace Jrpg.Core.Tests.Sprites;

public sealed class FacingRowsTests
{
    // The player sheet's order (decision 561), each row different so a swapped arm shows.
    private static readonly FacingRows _rows = new() { Down = 0, Left = 1, Right = 2, Up = 3 };

    [Theory]
    [InlineData(Facing.Down, 0)]
    [InlineData(Facing.Left, 1)]
    [InlineData(Facing.Right, 2)]
    [InlineData(Facing.Up, 3)]
    public void RowOf_EachFacing_ReturnsItsRow(Facing facing, int row) => Assert.Equal(row, _rows.RowOf(facing));

    [Fact]
    public void RowOf_UnknownFacing_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => _rows.RowOf((Facing)9));
}
