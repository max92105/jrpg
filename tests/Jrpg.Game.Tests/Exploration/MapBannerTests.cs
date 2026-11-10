using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The entry banner's timeline, stepped at the game's 1/60 s (decisions 528, 529).</summary>
public sealed class MapBannerTests
{
    private static readonly MapDefinition _village = Map("village", showsBanner: true);
    private static readonly MapDefinition _forest = Map("forest", showsBanner: true);
    private static readonly MapDefinition _intro = Map("intro", showsBanner: false);

    private readonly MapBanner _banner = new();

    [Fact]
    public void Enter_FirstMap_ShowsIt()
    {
        _banner.Enter(_village);

        Assert.Same(_village, _banner.Shown);
        Assert.Equal(0, _banner.Opacity);
    }

    [Fact]
    public void Advance_DuringTheWait_StaysTransparent()
    {
        _banner.Enter(_village);

        StepFor(MapBanner.WaitTime / 2);

        Assert.Equal(0, _banner.Opacity);
    }

    [Fact]
    public void Advance_HalfwayThroughTheFadeIn_IsHalfOpaque()
    {
        _banner.Enter(_village);

        StepFor(MapBanner.WaitTime + (MapBanner.FadeInTime / 2));

        Assert.InRange(_banner.Opacity, 0.45f, 0.55f);
    }

    [Fact]
    public void Advance_DuringTheHold_IsOpaque()
    {
        _banner.Enter(_village);

        StepFor(MapBanner.WaitTime + MapBanner.FadeInTime + (MapBanner.HoldTime / 2));

        Assert.Equal(1, _banner.Opacity);
    }

    [Fact]
    public void Advance_HalfwayThroughTheFadeOut_IsHalfOpaque()
    {
        _banner.Enter(_village);

        StepFor(MapBanner.WaitTime + MapBanner.FadeInTime + MapBanner.HoldTime + (MapBanner.FadeOutTime / 2));

        Assert.InRange(_banner.Opacity, 0.45f, 0.55f);
    }

    [Fact]
    public void Advance_AfterTheFadeOut_IsTransparent()
    {
        _banner.Enter(_village);

        StepFor(MapBanner.WaitTime + MapBanner.FadeInTime + MapBanner.HoldTime + MapBanner.FadeOutTime + StartMapScenarios.OneUpdate);

        Assert.Equal(0, _banner.Opacity);
        Assert.Null(_banner.Shown);
    }

    // A warp within the map: the banner keeps fading out rather than starting over.
    [Fact]
    public void Enter_SameMapAgain_DoesNotRestart()
    {
        _banner.Enter(_village);
        StepFor(MapBanner.WaitTime + MapBanner.FadeInTime + MapBanner.HoldTime + (MapBanner.FadeOutTime / 2));
        var before = _banner.Opacity;

        _banner.Enter(_village);
        StepFor(StartMapScenarios.OneUpdate);

        // A restart would be back in its wait, at 0: the opacity must still be falling from where it was.
        Assert.InRange(_banner.Opacity, 0.01f, before - 0.001f);
        Assert.Same(_village, _banner.Shown);
    }

    [Fact]
    public void Enter_SameMapAfterTheBannerEnded_ShowsNothing()
    {
        _banner.Enter(_village);
        StepFor(MapBanner.WaitTime + MapBanner.FadeInTime + MapBanner.HoldTime + MapBanner.FadeOutTime + StartMapScenarios.OneUpdate);

        _banner.Enter(_village);
        StepFor(MapBanner.WaitTime + MapBanner.FadeInTime);

        Assert.Null(_banner.Shown);
        Assert.Equal(0, _banner.Opacity);
    }

    [Fact]
    public void Enter_AnotherMapMidHold_RestartsOnIt()
    {
        _banner.Enter(_village);
        StepFor(MapBanner.WaitTime + MapBanner.FadeInTime + (MapBanner.HoldTime / 2));

        _banner.Enter(_forest);

        Assert.Same(_forest, _banner.Shown);
        Assert.Equal(0, _banner.Opacity);
    }

    [Fact]
    public void Enter_BackToTheFirstMap_ShowsItAgain()
    {
        _banner.Enter(_village);
        _banner.Enter(_forest);

        _banner.Enter(_village);

        Assert.Same(_village, _banner.Shown);
    }

    [Fact]
    public void Enter_MapNotShowingTheBanner_ShowsNothing()
    {
        _banner.Enter(_intro);

        Assert.Null(_banner.Shown);
        for (var step = TimeSpan.Zero; step < MapBanner.WaitTime + MapBanner.FadeInTime + MapBanner.HoldTime; step += StartMapScenarios.OneUpdate)
        {
            _banner.Advance(StartMapScenarios.OneUpdate);
            Assert.Equal(0, _banner.Opacity);
        }
    }

    [Fact]
    public void Enter_HiddenMapMidHold_StopsTheBanner()
    {
        _banner.Enter(_village);
        StepFor(MapBanner.WaitTime + MapBanner.FadeInTime + (MapBanner.HoldTime / 2));

        _banner.Enter(_intro);

        Assert.Null(_banner.Shown);
        Assert.Equal(0, _banner.Opacity);
    }

    [Fact]
    public void Enter_FromHiddenMapToShownMap_ShowsIt()
    {
        _banner.Enter(_intro);

        _banner.Enter(_village);

        Assert.Same(_village, _banner.Shown);
    }

    private void StepFor(TimeSpan span)
    {
        for (var stepped = TimeSpan.Zero; stepped < span; stepped += StartMapScenarios.OneUpdate)
        {
            _banner.Advance(StartMapScenarios.OneUpdate);
        }
    }

    private static MapDefinition Map(string id, bool showsBanner) => new(
        new MapLayout { Id = id, Width = 1, Height = 1, Layers = [], Objects = [] },
        new MapSettings { Id = id, DisplayName = id, ShowsBanner = showsBanner });
}
