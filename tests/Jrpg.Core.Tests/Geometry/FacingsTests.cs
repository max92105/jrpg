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
}
