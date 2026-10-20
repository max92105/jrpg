using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class BattleSceneTests
{
    [Fact]
    public void Update_ConfirmRun_RequestsBattleFinishedAndClearsEncounter()
    {
        var (session, scene) = StartForestBattle();

        var request = scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<BattleFinished>(request);
        Assert.Null(session.ActiveBattle);
        Assert.Null(session.Exploration.PendingEncounter);
    }

    [Fact]
    public void Update_AfterOutcomeRequest_ReturnsNull()
    {
        var (_, scene) = StartForestBattle();
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        var request = scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.Null(request);
    }

    // The first update plays any enemy turns ahead of Sarxian (200 HP, out of the goblins' reach), so Run is offered.
    private static (GameSession Session, BattleScene Scene) StartForestBattle()
    {
        var catalog = StartMapScenarios.NextToForestWarpAlwaysRolling;
        var session = GameSession.StartNew(catalog, new SeededRandom(1));
        Assert.True(StartMapScenarios.WalkUntilBattleRequested(StartMapScenarios.SceneOf(session.Exploration, catalog)));
        session.StartPendingBattle();
        var scene = new BattleScene(session, RepositoryScreenText.Text, new UiTextures());
        Assert.Null(scene.Update(StartMapScenarios.OneUpdate, default));
        return (session, scene);
    }
}
