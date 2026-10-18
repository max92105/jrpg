using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Rendering;

public sealed class VirtualScreenTests
{
    [Fact]
    public void DestinationIn_ExactlyTwiceVirtualSize_ScalesByTwo()
    {
        var destination = VirtualScreen.DestinationIn(new Rectangle(0, 0, 3840, 2160));

        Assert.Equal(new Rectangle(0, 0, 3840, 2160), destination);
    }

    [Fact]
    public void DestinationIn_UltrawideAtVirtualHeight_ScalesByOneWithSideBars()
    {
        var destination = VirtualScreen.DestinationIn(new Rectangle(0, 0, 2560, 1080));

        Assert.Equal(new Rectangle(320, 0, 1920, 1080), destination);
    }

    [Fact]
    public void DestinationIn_2560x1440_FitsFractionally()
    {
        var destination = VirtualScreen.DestinationIn(new Rectangle(0, 0, 2560, 1440));

        Assert.Equal(new Rectangle(0, 0, 2560, 1440), destination);
    }

    [Fact]
    public void DestinationIn_SmallerWindow_ScalesDownToFit()
    {
        var destination = VirtualScreen.DestinationIn(new Rectangle(0, 0, 1366, 768));

        Assert.Equal(new Rectangle(0, 0, 1365, 768), destination);
    }

    [Fact]
    public void DestinationIn_TallWindow_LetterboxesTopAndBottom()
    {
        var destination = VirtualScreen.DestinationIn(new Rectangle(0, 0, 1920, 1440));

        Assert.Equal(new Rectangle(0, 180, 1920, 1080), destination);
    }
}
