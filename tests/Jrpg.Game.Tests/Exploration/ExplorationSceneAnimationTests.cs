using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

// The map's animation time (decision 464): moved on by every update of the map scene, restarted on entering a map.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ExplorationSceneAnimationTests
{
    [Fact]
    public void Update_Walking_AdvancesTheTileAnimationTimeByTheDelta()
    {
        var (_, scene) = Start(StartMapScenarios.AfterTheIntro);

        for (var update = 0; update < 3; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, StartMapScenarios.HoldingLeft);
        }

        Assert.Equal(3 * StartMapScenarios.OneUpdate, scene.TileAnimationTime);
    }

    [Fact]
    public void Update_WhileAMessageShows_StillAdvancesTheTileAnimationTime()
    {
        var (exploration, scene) = Start(StartMapScenarios.BelowTalkingChest);
        Assert.True(exploration.TryInteract());

        for (var update = 0; update < 10; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, default);
        }

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        Assert.Equal(10 * StartMapScenarios.OneUpdate, scene.TileAnimationTime);
    }

    [Fact]
    public void Update_AfterAWarp_RestartsTheTileAnimationTime()
    {
        var (exploration, scene) = Start(StartMapScenarios.NextToForestWarp);
        for (var update = 0; update < 120 && exploration.CurrentMap.Id != "forest"; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, StartMapScenarios.HoldingLeft);
        }

        Assert.Equal("forest", exploration.CurrentMap.Id);
        Assert.Equal(TimeSpan.Zero, scene.TileAnimationTime);
    }

    private static (ExplorationState Exploration, ExplorationScene Scene) Start(ContentCatalog catalog)
    {
        var exploration = StartMapScenarios.StartExploring(catalog);
        return (exploration, StartMapScenarios.SceneOf(exploration, catalog));
    }
}
