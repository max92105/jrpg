using Jrpg.Core.Battle;
using Jrpg.Core.Content;
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
    private static readonly InputFrame _confirm = InputFrames.Pressing(GameButton.Confirm);

    [Fact]
    public void Update_ConfirmRun_RequestsBattleFinishedAndClearsEncounter()
    {
        var (session, scene) = StartForestBattle(StartMapScenarios.NextToForestWarpAlwaysRolling);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Up));

        var request = scene.Update(StartMapScenarios.OneUpdate, _confirm);

        Assert.IsType<BattleFinished>(request);
        Assert.Null(session.ActiveBattle);
        Assert.Null(session.Exploration.PendingEncounter);
    }

    [Fact]
    public void Update_AfterOutcomeRequest_ReturnsNull()
    {
        var (_, scene) = StartForestBattle(StartMapScenarios.NextToForestWarpAlwaysRolling);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Up));
        scene.Update(StartMapScenarios.OneUpdate, _confirm);

        var request = scene.Update(StartMapScenarios.OneUpdate, _confirm);

        Assert.Null(request);
    }

    // A test catalog, not repository balance (devil's advocate B-2): one goblin that a single hit fells.
    [Fact]
    public void Update_AttackUntilOver_RequestsBattleFinished()
    {
        var catalog = WithOneGoblin(StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { MaxHp = 1 });
        var (session, scene) = StartForestBattle(catalog);
        var experienceBefore = session.Party.Leader.Experience;
        scene.Update(StartMapScenarios.OneUpdate, _confirm);

        var request = scene.Update(StartMapScenarios.OneUpdate, _confirm);

        Assert.IsType<BattleFinished>(request);
        Assert.Equal(experienceBefore + catalog.Enemies["goblin"].ExperienceReward, session.Party.Leader.Experience);
    }

    [Fact]
    public void Update_DefendOnPartyTurn_PassesTheTurn()
    {
        var (session, scene) = StartForestBattle(StartMapScenarios.NextToForestWarpAlwaysRolling);
        var hpBefore = session.Party.Leader.Vitals.Hp;
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Down));

        var request = scene.Update(StartMapScenarios.OneUpdate, _confirm);

        Assert.Null(request);
        Assert.Equal(BattlePhase.PartyTurn, session.ActiveBattle?.Phase);
        Assert.True(session.Party.Leader.Vitals.Hp < hpBefore, "The goblins should have acted after Defend.");
    }

    // The goblin fells Sarxian on its first turn, whether it acts before him or after his Defend.
    [Fact]
    public void Update_PartyFalls_RequestsTitle()
    {
        var catalog = WithOneGoblin(StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { Attack = 9999 });
        var (session, scene) = StartForestBattleScene(catalog);

        var request = scene.Update(StartMapScenarios.OneUpdate, default)
            ?? scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Down))
            ?? scene.Update(StartMapScenarios.OneUpdate, _confirm);

        Assert.IsType<TitleRequested>(request);
        Assert.Null(session.ActiveBattle);
    }

    // The first update plays any enemy turns ahead of Sarxian (200 HP, out of the goblins' reach), so the list is offered.
    private static (GameSession Session, BattleScene Scene) StartForestBattle(ContentCatalog catalog)
    {
        var (session, scene) = StartForestBattleScene(catalog);
        Assert.Null(scene.Update(StartMapScenarios.OneUpdate, default));
        return (session, scene);
    }

    private static (GameSession Session, BattleScene Scene) StartForestBattleScene(ContentCatalog catalog)
    {
        var session = GameSession.StartNew(catalog, new SeededRandom(1));
        Assert.True(StartMapScenarios.WalkUntilBattleRequested(StartMapScenarios.SceneOf(session.Exploration, catalog)));
        session.StartPendingBattle();
        return (session, new BattleScene(session, RepositoryScreenText.Text, new UiTextures()));
    }

    // The forest encounter becomes a single goblin, changed as the test needs.
    private static ContentCatalog WithOneGoblin(ContentCatalog catalog, Func<EnemyDefinition, EnemyDefinition> change)
    {
        var goblin = change(catalog.Enemies["goblin"]);
        var trio = catalog.Encounters["goblin_trio"] with { EnemyIds = ["goblin"] };
        return catalog with
        {
            Enemies = new Dictionary<string, EnemyDefinition>(catalog.Enemies) { ["goblin"] = goblin },
            Encounters = new Dictionary<string, EncounterDefinition>(catalog.Encounters) { ["goblin_trio"] = trio },
        };
    }
}
