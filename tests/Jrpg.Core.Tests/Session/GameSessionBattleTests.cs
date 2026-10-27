using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Resources;
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
    private static readonly EnemyDefinition _goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 8, attack: 20).Build();

    [Fact]
    public void StartPendingBattle_NoEncounterPending_Throws()
    {
        var session = GameSession.StartNew(Catalog(), "sarxian", new FixedSequenceRandom());

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
    public void StartPendingBattle_NobodyStanding_StartsLostBattle()
    {
        var session = SessionWithEncounterPending();
        session.Party.Leader.Vitals.TakeDamage(999);

        var battle = session.StartPendingBattle();

        Assert.Equal(BattlePhase.Defeat, battle.Phase);
        Assert.Null(session.FinishBattle());
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

    // Sarxian fells the three goblins with three hits, each adding 1 focus.
    [Fact]
    public void FinishBattle_Victory_CarriesLastingAmountBack()
    {
        var session = SessionWithEncounterPending(Lasting("focus", gains: new ResourceGains { PerHitLanded = 1 }));
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        session.FinishBattle();

        Assert.Equal(5, session.Party.Leader.LastingAmounts["focus"]);
    }

    [Fact]
    public void FinishBattle_Escape_CarriesBackAndAppliesPerBattle()
    {
        var session = SessionWithEncounterPending(Lasting("focus", gains: new ResourceGains { PerBattleFought = -1 }));
        session.StartPendingBattle().Execute(new FleeCommand());

        session.FinishBattle();

        Assert.Equal(1, session.Party.Leader.LastingAmounts["focus"]);
    }

    // Rage gains 1 per hit landed but isn't lasting, so the next battle starts it at 0 again.
    [Fact]
    public void FinishBattle_NonLastingResource_NotCarried()
    {
        var rage = new ResourceDefinition("rage", "Rage", Max: 10) { Gains = new ResourceGains { PerHitLanded = 1 } };
        var session = SessionWithEncounterPending(rage);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        session.FinishBattle();

        Assert.Empty(session.Party.Leader.LastingAmounts);
        Assert.Equal(0, Combatant.FromPartyMember(session.Party.Leader).Resources.AmountOf("rage"));
    }

    // The victory takes several of Sarxian's turns, but the per-battle loss applies only once, at the end.
    [Fact]
    public void FinishBattle_PerBattleFought_AppliedOncePerBattle()
    {
        var session = SessionWithEncounterPending(Lasting("focus", gains: new ResourceGains { PerBattleFought = -1 }));
        var battle = session.StartPendingBattle();
        BattleScript.AttackUntilOver(battle);

        session.FinishBattle();

        Assert.Equal(1, session.Party.Leader.LastingAmounts["focus"]);
    }

    // Three hits bring focus from 2 to 5 of 5, and the per-battle gain can't take it past its max.
    [Fact]
    public void FinishBattle_PerBattleGainAtMax_ClampsAfterTheGain()
    {
        var focus = new ResourceDefinition("focus", "Focus", Max: 5, StartsAt: 2)
        {
            IsLasting = true,
            Gains = new ResourceGains { PerHitLanded = 1, PerBattleFought = 1 },
        };
        var session = SessionWithEncounterPending(focus);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        session.FinishBattle();

        Assert.Equal(5, session.Party.Leader.LastingAmounts["focus"]);
    }

    [Fact]
    public void FinishBattle_MageFought_ClarityDropsByOne()
    {
        var mana = new ResourceDefinition("mana", "Mana", Max: 10, StartsAt: 3);
        var clarity = new ResourceDefinition("clarity", "Clarity", Max: 3, StartsAt: 3)
        {
            IsLasting = true,
            Gains = new ResourceGains { PerBattleFought = -1 },
            Strain = new StrainRule("mana", 1),
        };
        var session = SessionWithEncounterPending(mana, clarity);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        session.FinishBattle();

        Assert.Equal(2, session.Party.Leader.LastingAmounts["clarity"]);
        Assert.Equal(2, Combatant.FromPartyMember(session.Party.Leader).Resources.AmountOf("mana"));
    }

    // The two goblins left after Sarxian's first blow hit three times in all before they fall; each hit costs 1 faith,
    // and the next battle starts from what is left.
    [Fact]
    public void FinishBattle_FaithLostInBattle_StaysLowered()
    {
        var faith = Lasting("faith", gains: new ResourceGains { PerHitTaken = -1 }) with { Max = 5, StartsAt = 5 };
        var session = SessionWithEncounterPending(faith);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        session.FinishBattle();

        Assert.Equal(2, Combatant.FromPartyMember(session.Party.Leader).Resources.AmountOf("faith"));
    }

    // Focus (max 10) starts at 2.
    private static ResourceDefinition Lasting(string id, ResourceGains gains) =>
        new(id, id, Max: 10, StartsAt: 2) { IsLasting = true, Gains = gains };

    private static GameSession SessionWithEncounterPending(params ResourceDefinition[] resources)
    {
        var session = GameSession.StartNew(Catalog(resources), "sarxian", new FixedSequenceRandom(_encounterThenSarxianFirst));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        Assert.NotNull(session.Exploration.PendingEncounter);
        return session;
    }

    private static ContentCatalog Catalog(params ResourceDefinition[] resources) => new ContentCatalogBuilder()
        .WithClasses(new ClassBuilder().WithResources(resources).Build())
        .WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition())
        .WithMaps(new MapLayoutBuilder().WithLayer("1111").Build() with { Id = "field" })
        .WithRandomEncounters("field", new RandomEncounterTable { OneInSteps = 1, GraceSteps = 1, EncounterIds = ["goblin_trio"] })
        .WithEnemies(_goblin)
        .WithEncounters(new EncounterDefinition { Id = "goblin_trio", EnemyIds = ["goblin", "goblin", "goblin"], BackgroundFile = "Forest.png" })
        .WithNewGame(new NewGameDefinition
        {
            LeaderChoices = [new LeaderChoice { MemberId = "sarxian" }],
            StartingItems = [],
            StartLocation = new MapLocation("field", new Point2D(0.5, 0.5), Facing.Right),
            FieldSpriteFile = "player.png",
        })
        .Build();
}
