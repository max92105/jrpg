using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class PanelViewTests
{
    private static readonly Color _fill = new(0, 0, 0, 0xAA);
    private static readonly PanelStyle _style = new(CornerRadius: 4, BorderWidth: 1, Fill: _fill, Border: Color.White);

    // The baked texture is 2 x 4 + 1 = 9 pixels square: the corners and one stretchable middle row and column.
    private readonly Color[] _pixels = PanelView.BakePixels(_style);

    [Fact]
    public void BakePixels_OutsideCorner_IsTransparent()
    {
        Assert.Equal(Color.Transparent, PixelAt(0, 0));
    }

    [Fact]
    public void BakePixels_Middle_IsFill()
    {
        Assert.Equal(_fill, PixelAt(4, 4));
    }

    [Fact]
    public void BakePixels_MiddleOfTopEdge_IsBorder()
    {
        Assert.Equal(Color.White, PixelAt(4, 0));
    }

    [Fact]
    public void BakePixels_Size_IsTwoCornersAndOneMiddlePixel()
    {
        Assert.Equal(9 * 9, _pixels.Length);
    }

    private Color PixelAt(int x, int y) => _pixels[(y * 9) + x];
}
