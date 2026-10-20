using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Battle;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

public sealed class GameSessionBattleTests
{
    // The second step rolls 0 (a battle) and picks goblin_trio, then the shuffle keeps the order: Sarxian, then the goblins.
    private static readonly int[] _encounterThenSarxianFirst = [0, 0, 3, 2, 1];

    // One hit from Sarxian (attack 10) fells a goblin, and three goblins can't fell Sarxian (100 HP).
    private static readonly EnemyDefinition _goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 8, attack: 20, defense: 2).Build();

    [Fact]
    public void StartPendingBattle_NoEncounterPending_Throws()
    {
        var session = GameSession.StartNew(Catalog(), new FixedSequenceRandom());

        Assert.Throws<InvalidOperationException>(session.StartPendingBattle);
    }

    [Fact]
    public void StartPendingBattle_GoblinTrioPending_PutsActivePartyAgainstThreeGoblins()
    {
        var session = SessionWithEncounterPending();

        var battle = session.StartPendingBattle();

        Assert.Same(battle, session.ActiveBattle);
        Assert.Equal([session.Party.Leader], battle.LivingOn(BattleSide.Party).Select(combatant => combatant.Member));
        Assert.Equal([_goblin, _goblin, _goblin], battle.LivingOn(BattleSide.Enemies).Select(combatant => combatant.Enemy));
    }

    [Fact]
    public void StartPendingBattle_BattleAlreadyActive_Throws()
    {
        var session = SessionWithEncounterPending();
        session.StartPendingBattle();

        Assert.Throws<InvalidOperationException>(session.StartPendingBattle);
    }

    [Fact]
    public void FinishBattle_BattleNotOver_Throws()
    {
        var session = SessionWithEncounterPending();
        session.StartPendingBattle();

        Assert.Throws<InvalidOperationException>(session.FinishBattle);
        Assert.NotNull(session.ActiveBattle);
    }

    [Fact]
    public void FinishBattle_AfterVictory_GrantsExperienceAndReturnsTotal()
    {
        var session = SessionWithEncounterPending();
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        var rewards = session.FinishBattle();

        Assert.NotNull(rewards);
        Assert.Equal(120, rewards.Experience);
        Assert.Equal(120, Assert.Single(rewards.Gains).Amount);
        Assert.Equal(2, session.Party.Leader.Level);
        Assert.Null(session.ActiveBattle);
        Assert.Null(session.Exploration.PendingEncounter);
    }

    [Fact]
    public void FinishBattle_AfterEscape_ReturnsNull()
    {
        var session = SessionWithEncounterPending();
        session.StartPendingBattle().Execute(new FleeCommand());

        var rewards = session.FinishBattle();

        Assert.Null(rewards);
        Assert.Null(session.ActiveBattle);
        Assert.Null(session.Exploration.PendingEncounter);
    }

    [Fact]
    public void Capture_AfterFinishBattle_Succeeds()
    {
        var session = SessionWithEncounterPending();
        session.StartPendingBattle().Execute(new FleeCommand());
        session.FinishBattle();

        var save = session.Capture(new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal("field", save.Location.MapId);
    }

    private static GameSession SessionWithEncounterPending()
    {
        var session = GameSession.StartNew(Catalog(), new FixedSequenceRandom(_encounterThenSarxianFirst));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        Assert.NotNull(session.Exploration.PendingEncounter);
        return session;
    }

    private static ContentCatalog Catalog() => new ContentCatalogBuilder()
        .WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition())
        .WithMaps(new MapLayoutBuilder().WithLayer("1111").Build() with { Id = "field" })
        .WithRandomEncounters("field", new RandomEncounterTable { OneInSteps = 1, GraceSteps = 1, EncounterIds = ["goblin_trio"] })
        .WithEnemies(_goblin)
        .WithEncounters(new EncounterDefinition { Id = "goblin_trio", EnemyIds = ["goblin", "goblin", "goblin"], BackgroundFile = "Forest.png" })
        .WithNewGame(new NewGameDefinition
        {
            RecruitedMemberIds = ["sarxian"],
            ActiveMemberIds = ["sarxian"],
            StartingItems = [],
            StartLocation = new MapLocation("field", new Point2D(0.5, 0.5), Facing.Right),
            FieldSpriteFile = "player.png",
        })
        .Build();
}
