using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class PointedBarShapeTests
{
    [Fact]
    public void RewardBar_PointedEnd_IsClearAtCorners()
    {
        const int width = PointedBarShape.RewardBarWidth;
        const int height = PointedBarShape.RewardBarHeight;
        var pixels = PointedBarShape.RewardBar(Enumerable.Repeat(Color.Blue, width).ToArray());

        Assert.Equal(Color.Transparent, pixels[0]);
        Assert.Equal(Color.Transparent, pixels[(height - 1) * width]);
        Assert.Equal(Color.Blue, pixels[((height / 2) * width) + 1]);
        Assert.Equal(Color.Blue, pixels[width - 1]);
    }

    [Fact]
    public void CardHeader_BothEndsPointed()
    {
        const int width = PointedBarShape.CardHeaderWidth;
        const int height = PointedBarShape.CardHeaderHeight;
        var pixels = PointedBarShape.CardHeader(Enumerable.Repeat(Color.Blue, width).ToArray(), Color.White);

        Assert.Equal(Color.Transparent, pixels[0]);
        Assert.Equal(Color.Transparent, pixels[width - 1]);
        Assert.Equal(Color.Transparent, pixels[((height - 1) * width) + width - 1]);
        Assert.Equal(Color.Blue, pixels[((height / 2) * width) + (width / 2)]);
    }
}
