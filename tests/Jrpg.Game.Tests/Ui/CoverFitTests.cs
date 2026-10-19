using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class CoverFitTests
{
    private static readonly Point _virtualScreen = new(1920, 1080);

    [Fact]
    public void SourceIn_TallerImage_CropsTopAndBottomEqually()
    {
        var source = CoverFit.SourceIn(new Point(1536, 1024), _virtualScreen);

        Assert.Equal(new Rectangle(0, 80, 1536, 864), source);
    }

    [Fact]
    public void SourceIn_WiderImage_CropsBothSidesEqually()
    {
        var source = CoverFit.SourceIn(new Point(3000, 1080), _virtualScreen);

        Assert.Equal(new Rectangle(540, 0, 1920, 1080), source);
    }

    [Fact]
    public void SourceIn_SameProportions_KeepsWholeImage()
    {
        var source = CoverFit.SourceIn(new Point(3840, 2160), _virtualScreen);

        Assert.Equal(new Rectangle(0, 0, 3840, 2160), source);
    }
}
