using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class NavTabShapeTests
{
    private static readonly Color _outline = Color.White;

    // Each column filled with a different color, so a test can tell which column a fill pixel came from.
    private static readonly Color[] _fill = [.. Enumerable.Range(0, NavTabShape.Width).Select(x => new Color(x % 256, 0, 1))];

    private static readonly Color[] _pixels = NavTabShape.Pixels(_fill, _outline);

    [Fact]
    public void Pixels_Middle_TakesItsColumnsFill()
    {
        var x = 200;

        Assert.Equal(_fill[x], PixelAt(x, NavTabShape.Height / 2));
    }

    [Fact]
    public void Pixels_TopLeftCorner_IsOutline()
    {
        Assert.Equal(_outline, PixelAt(0, 0));
    }

    [Fact]
    public void Pixels_TopRightCornerBesideThePoint_IsTransparent()
    {
        Assert.Equal(Color.Transparent, PixelAt(NavTabShape.Width - 1, 0));
    }

    [Fact]
    public void Pixels_TipOfThePoint_IsOutline()
    {
        Assert.Equal(_outline, PixelAt(NavTabShape.Width - 2, NavTabShape.Height / 2));
    }

    private static Color PixelAt(int x, int y) => _pixels[(y * NavTabShape.Width) + x];
}
