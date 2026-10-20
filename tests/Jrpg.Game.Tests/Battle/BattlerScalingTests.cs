using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

public sealed class BattlerScalingTests
{
    [Fact]
    public void Of_1024PxPainting_ShrinksSmoothlyTo96()
    {
        var scaling = BattlerScaling.Of(new Point(1024, 1024));

        Assert.Equal(new BattlerScaling(new Point(96, 96), IsShrunkSmoothly: true), scaling);
    }

    [Fact]
    public void Of_512PxImage_IsStillPixelArt()
    {
        var scaling = BattlerScaling.Of(new Point(512, 512));

        Assert.Equal(new BattlerScaling(new Point(256, 256), IsShrunkSmoothly: false), scaling);
    }

    [Fact]
    public void Of_256PxPixelArt_PointScalesByHalf()
    {
        var scaling = BattlerScaling.Of(new Point(256, 256));

        Assert.Equal(new BattlerScaling(new Point(128, 128), IsShrunkSmoothly: false), scaling);
    }
}
