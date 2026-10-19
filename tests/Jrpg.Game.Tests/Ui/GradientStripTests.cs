using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class GradientStripTests
{
    private static readonly Color _clear = new(0, 0, 0, 0);
    private static readonly Color _bright = new(200, 200, 200, 200);

    [Fact]
    public void Colors_TwoStops_InterpolatesMidpoint()
    {
        // Five pixels: the middle one's center sits at offset 0.5.
        var colors = GradientStrip.Colors([new GradientStop(0, _clear), new GradientStop(1, _bright)], width: 5);

        Assert.Equal(new Color(100, 100, 100, 100), colors[2]);
    }

    [Fact]
    public void Colors_BeforeFirstStop_TakesFirstStopColor()
    {
        var colors = GradientStrip.Colors([new GradientStop(0.5f, _bright), new GradientStop(1, _clear)], width: 4);

        Assert.Equal(_bright, colors[0]);
    }

    [Fact]
    public void Colors_ThreeStops_UsesTheSegmentHoldingThePixel()
    {
        var colors = GradientStrip.Colors(
            [new GradientStop(0, _clear), new GradientStop(0.5f, _bright), new GradientStop(1, _clear)],
            width: 4);

        // Pixel 3's center is at 0.875, three quarters of the way from bright to clear: a quarter bright.
        Assert.Equal(new Color(50, 50, 50, 50), colors[3]);
    }
}
