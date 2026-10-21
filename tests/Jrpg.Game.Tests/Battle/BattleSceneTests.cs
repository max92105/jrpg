using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class BattleSceneTests
{
    private static readonly InputFrame _confirm = InputFrames.Pressing(GameButton.Confirm);

    [Fact]
    public void Update_ConfirmRun_ShowsEscapedAndClearsEncounter()
    {
        var (session, scene) = StartForestBattle(StartMapScenarios.NextToForestWarpAlwaysRolling);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Up));
        scene.Update(StartMapScenarios.OneUpdate, _confirm);

        var request = Settle(scene);

        Assert.Null(request);
        Assert.Null(session.ActiveBattle);
        Assert.Null(session.Exploration.PendingEncounter);
    }

    [Fact]
    public void Update_RunThenWait_RequestsBattleFinished()
    {
        var (_, scene) = StartForestBattle(StartMapScenarios.NextToForestWarpAlwaysRolling);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Up));
        scene.Update(StartMapScenarios.OneUpdate, _confirm);
        Settle(scene);

        var request = Settle(scene);

        Assert.IsType<BattleFinished>(request);
    }

    [Fact]
    public void Update_AfterOutcomeRequest_ReturnsNull()
    {
        var (_, scene) = StartForestBattle(StartMapScenarios.NextToForestWarpAlwaysRolling);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Up));
        scene.Update(StartMapScenarios.OneUpdate, _confirm);
        Settle(scene);
        Settle(scene);

        var request = scene.Update(StartMapScenarios.OneUpdate, _confirm);

        Assert.Null(request);
    }

    // A test catalog, not repository balance (devil's advocate B-2): one goblin that a single hit fells.
    [Fact]
    public void Update_AttackUntilOver_FinishesBattleAndShowsVictory()
    {
        var catalog = WithOneGoblin(StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { MaxHp = 1 });
        var (session, scene) = StartForestBattle(catalog);
        var experienceBefore = session.Party.Leader.Experience;
        scene.Update(StartMapScenarios.OneUpdate, _confirm);
        scene.Update(StartMapScenarios.OneUpdate, _confirm);

        var request = Settle(scene);

        Assert.Null(request);
        Assert.Null(session.ActiveBattle);
        Assert.Equal(experienceBefore + catalog.Enemies["goblin"].ExperienceReward, session.Party.Leader.Experience);
    }

    [Fact]
    public void Update_ConfirmOnFilledVictory_RequestsBattleFinished()
    {
        var catalog = WithOneGoblin(StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { MaxHp = 1 });
        var (_, scene) = StartForestBattle(catalog);
        scene.Update(StartMapScenarios.OneUpdate, _confirm);
        scene.Update(StartMapScenarios.OneUpdate, _confirm);
        Settle(scene);
        Settle(scene);

        var request = scene.Update(StartMapScenarios.OneUpdate, _confirm);

        Assert.IsType<BattleFinished>(request);
    }

    [Fact]
    public void Update_DefendOnPartyTurn_PassesTheTurn()
    {
        var (session, scene) = StartForestBattle(StartMapScenarios.NextToForestWarpAlwaysRolling);
        var hpBefore = session.Party.Leader.Vitals.Hp;

        var request = DefendAndSettle(scene);

        Assert.Null(request);
        Assert.Equal(BattlePhase.PartyTurn, session.ActiveBattle?.Phase);
        Assert.True(session.Party.Leader.Vitals.Hp < hpBefore, "The goblins should have acted after Defend.");
    }

    // The goblin fells Sarxian on its first turn, whether it acts before him or after his Defend.
    [Fact]
    public void Update_PartyFallsThenMainMenu_RequestsTitle()
    {
        var catalog = WithOneGoblin(StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { Attack = 9999 });
        var (session, scene) = StartForestBattleScene(catalog);
        Settle(scene);
        if (session.ActiveBattle is not null)
        {
            DefendAndSettle(scene);
        }

        Settle(scene); // The defeat screen's choices come in before they take input.
        var request = scene.Update(StartMapScenarios.OneUpdate, _confirm);

        Assert.IsType<TitleRequested>(request);
        Assert.Null(session.ActiveBattle);
    }

    // No enemy acts in a battle lost at the start: it is finished at once, and the defeat screen waits for a choice.
    [Fact]
    public void Update_BattleLostAtStart_FinishesBattleAndWaitsOnDefeat()
    {
        var catalog = StartMapScenarios.NextToForestWarpAlwaysRolling;
        var (session, scene) = StartForestBattleScene(catalog, session => session.Party.Leader.Vitals.TakeDamage(9999));

        var first = Settle(scene);
        var second = Settle(scene);

        Assert.Null(first);
        Assert.Null(second);
        Assert.Null(session.ActiveBattle);
    }

    // Settling plays any enemy turns ahead of Sarxian (200 HP, out of the goblins' reach), so the list is offered.
    private static (GameSession Session, BattleScene Scene) StartForestBattle(ContentCatalog catalog)
    {
        var (session, scene) = StartForestBattleScene(catalog);
        Assert.Null(Settle(scene));
        return (session, scene);
    }

    // Long enough for any enemy turns and the actor's steps to play out.
    private static SceneRequest? Settle(BattleScene scene) => scene.Update(TimeSpan.FromSeconds(10), default);

    // Sarxian knows skills and the new game's bag holds potions, so Defend is three rows down, past Skills and Items.
    private static SceneRequest? DefendAndSettle(BattleScene scene)
    {
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Down));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Down));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Down));
        scene.Update(StartMapScenarios.OneUpdate, _confirm);
        return Settle(scene);
    }

    private static (GameSession Session, BattleScene Scene) StartForestBattleScene(ContentCatalog catalog) =>
        StartForestBattleScene(catalog, _ => { });

    // beforeBattle changes the session once the encounter is pending, before the battle starts.
    private static (GameSession Session, BattleScene Scene) StartForestBattleScene(ContentCatalog catalog, Action<GameSession> beforeBattle)
    {
        var session = GameSession.StartNew(catalog, new SeededRandom(1));
        Assert.True(StartMapScenarios.WalkUntilBattleRequested(StartMapScenarios.SceneOf(session.Exploration, catalog)));
        beforeBattle(session);
        session.StartPendingBattle();
        return (session, new BattleScene(session, new BattleSceneSources(catalog.Items, UnwrittenSaveSlots.Create()), RepositoryScreenText.Text, new UiTextures()));
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
