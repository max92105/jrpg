using Jrpg.Game.Exploration;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Exploration;

public sealed class BackgroundReleaseTests
{
    [Fact]
    public void Of_FileOnlyOnThePreviousMap_IsReleased()
    {
        var released = BackgroundRelease.Of(["Forest.png", "Cave.png"], ["Cave.png"]);

        Assert.Equal(["Forest.png"], released);
    }

    [Fact]
    public void Of_FileOnBothMaps_IsKept()
    {
        var released = BackgroundRelease.Of(["Cave.png"], ["Arena.png", "Cave.png"]);

        Assert.Empty(released);
    }

    [Fact]
    public void Of_MenuBackgroundOnThePreviousMap_IsNeverReleased()
    {
        var released = BackgroundRelease.Of([UiArtFiles.Backgrounds.Menu, "Forest.png"], ["Cave.png"]);

        Assert.Equal(["Forest.png"], released);
    }

    [Fact]
    public void Of_TitleBackgroundOnThePreviousMap_IsNeverReleased()
    {
        var released = BackgroundRelease.Of([UiArtFiles.Backgrounds.Title, "Forest.png"], ["Cave.png"]);

        Assert.Equal(["Forest.png"], released);
    }

    [Fact]
    public void Of_NoPreviousMap_ReleasesNothing()
    {
        var released = BackgroundRelease.Of([], ["Forest.png"]);

        Assert.Empty(released);
    }
}
