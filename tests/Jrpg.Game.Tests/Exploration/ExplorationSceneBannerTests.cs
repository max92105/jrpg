using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The entry banner driven by the scene (decision 529), on the test world at 1/60 s updates.</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ExplorationSceneBannerTests
{
    private static readonly TimeSpan _untilShown = MapBanner.WaitTime + MapBanner.FadeInTime + StartMapScenarios.OneUpdate;

    [Fact]
    public void Update_NewGameStart_ShowsTheStartMap()
    {
        var (_, scene, overlays) = Start(StartMapScenarios.AfterTheIntro);

        StepFor(scene, _untilShown);

        Assert.Equal("start", overlays.Banner.Shown?.Id);
        Assert.Equal(1, overlays.Banner.Opacity);
        Assert.Equal(overlays.Banner.Shown!.Settings.DisplayName, overlays.BannerView.NameLine);
    }

    // The test world's new game starts on the intro map with its cutscene running: the banner waits for it.
    [Fact]
    public void Update_EventRunning_BannerWaits()
    {
        var (exploration, scene, overlays) = Start(StartMapScenarios.TestWorld);
        Assert.True(exploration.IsEventRunning);

        StepFor(scene, _untilShown);

        Assert.Equal("intro", overlays.Banner.Shown?.Id);
        Assert.Equal(0, overlays.Banner.Opacity);
    }

    [Fact]
    public void Update_WarpToAnotherMap_ShowsThatMap()
    {
        var (exploration, scene, overlays) = Start(StartMapScenarios.NextToForestWarp);
        for (var update = 0; update < 120 && exploration.CurrentMap.Id != "forest"; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, StartMapScenarios.HoldingLeft);
        }

        Assert.Equal("forest", exploration.CurrentMap.Id);

        for (var update = 0; update < 600 && overlays.Banner.Opacity == 0; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, default);
        }

        Assert.Equal("forest", overlays.Banner.Shown?.Id);
        Assert.True(overlays.Banner.Opacity > 0);
        Assert.Equal(exploration.CurrentMap.Settings.DisplayName, overlays.BannerView.NameLine);
    }

    private static void StepFor(ExplorationScene scene, TimeSpan span)
    {
        for (var stepped = TimeSpan.Zero; stepped < span; stepped += StartMapScenarios.OneUpdate)
        {
            scene.Update(StartMapScenarios.OneUpdate, default);
        }
    }

    private static (ExplorationState Exploration, ExplorationScene Scene, FieldOverlays Overlays) Start(ContentCatalog catalog)
    {
        var exploration = StartMapScenarios.StartExploring(catalog);
        var overlays = StartMapScenarios.OverlaysFor(exploration);
        return (exploration, StartMapScenarios.SceneOf(exploration, catalog, overlays), overlays);
    }
}
