using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Battle;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

// Gold draws once per defeated enemy with a range, after every item draw (ADR 0031). Sarxian fells the three goblins
// one hit each; the battle itself draws nothing, so any value after the encounter's draws goes to the loot.
public sealed class GameSessionGoldTests
{
    // The second step rolls 0 (a battle) and picks goblin_trio; the shuffle keeps Sarxian first, then the goblins.
    private static readonly int[] _encounterThenSarxianFirst = [0, 0, 3, 2, 1];

    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).Build();
    private static readonly EnemyDefinition _goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 8, damage: 20).Build();
    private static readonly EnemyDefinition _richGoblin = new EnemyDefinitionBuilder().WithStats(maxHp: 8, damage: 20).WithGold(6, 12).Build();

    // Within 6..12 each goblin draws below 7: 3, 0 and 6 give 9, 6 and 12.
    [Fact]
    public void FinishBattle_VictoryOverGoldEnemies_AddsTheDrawnGold()
    {
        var session = SessionWithEncounterPending(_richGoblin, [.. _encounterThenSarxianFirst, 3, 0, 6]);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        var spoils = session.FinishBattle();

        Assert.Equal(27, spoils?.Gold);
        Assert.Equal(27, session.Party.Purse.Gold);
    }

    [Fact]
    public void FinishBattle_PurseNearlyFull_SpoilsReportOnlyWhatFit()
    {
        var session = SessionWithEncounterPending(_richGoblin, [.. _encounterThenSarxianFirst, 3, 0, 6]);
        session.Party.Purse.Add(Purse.MaxGold - 10);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        var spoils = session.FinishBattle();

        Assert.Equal(10, spoils?.Gold);
        Assert.Equal(Purse.MaxGold, session.Party.Purse.Gold);
    }

    // The scripted random holds nothing past the encounter's draws, so a roll would throw.
    [Fact]
    public void FinishBattle_EnemyWithoutGold_DrawsNothing()
    {
        var session = SessionWithEncounterPending(_goblin, _encounterThenSarxianFirst);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        var spoils = session.FinishBattle();

        Assert.Equal(0, spoils?.Gold);
        Assert.Equal(0, session.Party.Purse.Gold);
    }

    // The same seed drops the same items whether or not the goblins carry gold: the gold draws come after the swords'.
    [Fact]
    public void FinishBattle_GoldEnemies_DropsSameAsWithoutGold()
    {
        int[] swordDraws = [0, 999, 0];
        var poor = SessionWithEncounterPending(_goblin with { DropTableId = "goblin" }, [.. _encounterThenSarxianFirst, .. swordDraws]);
        var rich = SessionWithEncounterPending(_richGoblin with { DropTableId = "goblin" }, [.. _encounterThenSarxianFirst, .. swordDraws, 3, 0, 6]);
        BattleScript.AttackUntilOver(poor.StartPendingBattle());
        BattleScript.AttackUntilOver(rich.StartPendingBattle());

        var poorDrops = poor.FinishBattle()?.Drops;
        var richSpoils = rich.FinishBattle();

        Assert.Equal(poorDrops?.Stacks, richSpoils?.Drops.Stacks);
        Assert.Equal(poorDrops?.Gear, richSpoils?.Drops.Gear);
        Assert.Equal(27, richSpoils?.Gold);
    }

    [Fact]
    public void StartNew_StartingGold_PurseHoldsIt()
    {
        var session = GameSession.StartNew(Catalog(_goblin, startingGold: 100), "sarxian", new FixedSequenceRandom());

        Assert.Equal(100, session.Party.Purse.Gold);
    }

    private static GameSession SessionWithEncounterPending(EnemyDefinition goblin, int[] draws)
    {
        var session = GameSession.StartNew(Catalog(goblin, startingGold: 0), "sarxian", new FixedSequenceRandom(draws));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        Assert.NotNull(session.Exploration.PendingEncounter);
        return session;
    }

    private static ContentCatalog Catalog(EnemyDefinition goblin, int startingGold) => new ContentCatalogBuilder()
        .WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition())
        .WithMaps(new MapLayoutBuilder().WithLayer("1111").Build() with { Id = "field" })
        .WithRandomEncounters("field", new RandomEncounterTable { OneInSteps = 1, GraceSteps = 1, EncounterIds = ["goblin_trio"] })
        .WithEnemies(goblin)
        .WithEncounters(new EncounterDefinition { Id = "goblin_trio", EnemyIds = ["goblin", "goblin", "goblin"], BackgroundFile = "Forest.png" })
        .WithItems(_potion, _sword)
        .WithDropTables(new DropTable("goblin", [new DropEntry("potion", 100), new DropEntry("bronze_sword", 50)]))
        .WithNewGame(new NewGameDefinition
        {
            LeaderChoices = [new LeaderChoice { MemberId = "sarxian" }],
            StartingItems = [],
            StartingGold = startingGold,
            StartLocation = new MapLocation("field", new Point2D(0.5, 0.5), Facing.Right),
            FieldSpriteFile = "player.png",
        })
        .Build();
}
