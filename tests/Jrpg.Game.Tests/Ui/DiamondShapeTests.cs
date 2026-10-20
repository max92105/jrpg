using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class DiamondShapeTests
{
    private const int Size = 48;

    [Fact]
    public void Pixels_Fill_CenterAndCorner()
    {
        var pixels = new DiamondShape(Size, 0, 22 / 24f, Color.White, Color.Red).Pixels();

        Assert.Equal(Color.Red, pixels[(24 * Size) + 24]);
        Assert.Equal(Color.Transparent, pixels[0]);
    }

    [Fact]
    public void Pixels_UpperHalf_UsesUpperColor()
    {
        var pixels = new DiamondShape(Size, 0, 22 / 24f, Color.White, Color.Red).Pixels();

        Assert.Equal(Color.White, pixels[(20 * Size) + 24]);
    }

    [Fact]
    public void Pixels_Ring_CenterIsEmpty()
    {
        // At 96 px the ring runs from 42 to 44 px off the center, tip-ward: pixel 5 of the middle row sits 43 px out.
        const int ringSize = 96;
        var pixels = new DiamondShape(ringSize, 21 / 24f, 22 / 24f, Color.White, Color.White).Pixels();

        Assert.Equal(Color.Transparent, pixels[(48 * ringSize) + 48]);
        Assert.Equal(Color.White, pixels[(48 * ringSize) + 5]);
    }
}
