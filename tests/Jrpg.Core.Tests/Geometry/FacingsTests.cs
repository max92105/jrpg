using Jrpg.Core.Geometry;

namespace Jrpg.Core.Tests.Geometry;

public sealed class FacingsTests
{
    [Theory]
    [InlineData(Facing.Down, Facing.Up)]
    [InlineData(Facing.Up, Facing.Down)]
    [InlineData(Facing.Left, Facing.Right)]
    [InlineData(Facing.Right, Facing.Left)]
    public void Opposite_EachFacing_IsTheOtherWay(Facing facing, Facing opposite) =>
        Assert.Equal(opposite, Facings.Opposite(facing));

    [Fact]
    public void Opposite_UnknownValue_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => Facings.Opposite((Facing)9));

    [Theory]
    [InlineData(Facing.Up, 2, 1)]
    [InlineData(Facing.Down, 2, 3)]
    [InlineData(Facing.Left, 1, 2)]
    [InlineData(Facing.Right, 3, 2)]
    public void Step_EachFacing_MovesOneCell(Facing facing, int x, int y) =>
        Assert.Equal(new GridCell(x, y), Facings.Step(new GridCell(2, 2), facing));
}
