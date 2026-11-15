using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

// The impact's soft disc (decision 660): opaque at the center, clear at the edge, (1 - d²)² between.
public sealed class RadialGlowShapeTests
{
    private const int Size = RadialGlowShape.Size;

    private static readonly Microsoft.Xna.Framework.Color[] _pixels = RadialGlowShape.Pixels();

    [Fact]
    public void Pixels_Center_IsNearlyOpaque()
    {
        Assert.True(AlphaAt(Size / 2, Size / 2) >= 254);
    }

    [Fact]
    public void Pixels_EdgesMiddle_IsClear()
    {
        Assert.Equal(0, AlphaAt(0, Size / 2));
    }

    [Fact]
    public void Pixels_Corner_IsClear()
    {
        Assert.Equal(0, AlphaAt(0, 0));
    }

    [Fact]
    public void Pixels_HalfwayOut_IsNineSixteenths()
    {
        // At d = 0.5, (1 - 0.25)² = 0.5625 of 255.
        Assert.InRange(AlphaAt((Size / 2) + (Size / 4), Size / 2), 140, 147);
    }

    [Fact]
    public void Pixels_MirrorAcrossTheCenter_IsTheSame()
    {
        Assert.Equal(AlphaAt(10, 40), AlphaAt(Size - 1 - 10, Size - 1 - 40));
    }

    [Fact]
    public void Pixels_IsWhitePremultiplied()
    {
        var pixel = _pixels[((Size / 2) * Size) + (Size / 4)];

        Assert.Equal((pixel.A, pixel.A, pixel.A), (pixel.R, pixel.G, pixel.B));
    }

    private static int AlphaAt(int x, int y) => _pixels[(y * Size) + x].A;
}
