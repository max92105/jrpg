using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Maps;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Battle;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

// Loot rolls after a victory, from the session's random (ADR 0030). Sarxian fells the three goblins one hit each; the
// battle itself draws nothing, so any value after the encounter's draws goes to the loot.
public sealed class GameSessionDropTests
{
    // The second step rolls 0 (a battle) and picks goblin_trio; the shuffle keeps Sarxian first, then the goblins.
    private static readonly int[] _encounterThenSarxianFirst = [0, 0, 3, 2, 1];

    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).Build();
    private static readonly EnemyDefinition _goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 8, damage: 20).Build();

    // Every goblin drops a potion; the sword's 50 % draws once per goblin: 0 drops, 999 doesn't, 0 drops.
    [Fact]
    public void FinishBattle_Victory_AddsDropsToInventory()
    {
        var session = SessionWithEncounterPending(_goblin with { DropTableId = "goblin" }, [.. _encounterThenSarxianFirst, 0, 999, 0]);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        var drops = session.FinishBattle()?.Drops;

        Assert.NotNull(drops);
        Assert.Equal([new ItemDrop(_potion, 3)], drops.Stacks);
        Assert.Equal([GearPiece.Plain(_sword), GearPiece.Plain(_sword)], drops.Gear);
        Assert.Equal((3, 2), (session.Inventory.QuantityOf("potion"), session.Inventory.GearCountOf("bronze_sword")));
    }

    // The bag already holds the two potions a stack takes, so the three dropped are lost and not listed.
    [Fact]
    public void FinishBattle_StackFull_ListsOnlyWhatFit()
    {
        var potion = _potion with { MaxStack = 2 };
        var session = SessionWithEncounterPending(_goblin with { DropTableId = "goblin" }, [.. _encounterThenSarxianFirst, 999, 999, 999], potion);
        session.Inventory.Add(potion, 2);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        var drops = session.FinishBattle()?.Drops;

        Assert.NotNull(drops);
        Assert.Empty(drops.Stacks);
        Assert.Equal(2, session.Inventory.QuantityOf("potion"));
    }

    [Fact]
    public void FinishBattle_Escaped_ReturnsNullAndRollsNothing()
    {
        var session = SessionWithEncounterPending(_goblin with { DropTableId = "goblin" }, _encounterThenSarxianFirst);
        session.StartPendingBattle().Execute(new FleeCommand());

        var spoils = session.FinishBattle();

        Assert.Null(spoils);
        Assert.Equal(0, session.Inventory.QuantityOf("potion"));
    }

    // The scripted random holds nothing past the encounter's draws, so a roll would throw.
    [Fact]
    public void FinishBattle_EnemyWithoutTable_DrawsNothing()
    {
        var session = SessionWithEncounterPending(_goblin, _encounterThenSarxianFirst);
        BattleScript.AttackUntilOver(session.StartPendingBattle());

        var spoils = session.FinishBattle();

        Assert.True(spoils?.Drops.IsEmpty);
    }

    private static GameSession SessionWithEncounterPending(EnemyDefinition goblin, int[] draws, ItemDefinition? potion = null)
    {
        var session = GameSession.StartNew(Catalog(goblin, potion ?? _potion), "sarxian", new FixedSequenceRandom(draws));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        Assert.NotNull(session.Exploration.PendingEncounter);
        return session;
    }

    private static ContentCatalog Catalog(EnemyDefinition goblin, ItemDefinition potion) => new ContentCatalogBuilder()
        .WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition())
        .WithMaps(new MapLayoutBuilder().WithLayer("1111").Build() with { Id = "field" })
        .WithRandomEncounters("field", new RandomEncounterTable { OneInSteps = 1, GraceSteps = 1, EncounterIds = ["goblin_trio"] })
        .WithEnemies(goblin)
        .WithEncounters(new EncounterDefinition { Id = "goblin_trio", EnemyIds = ["goblin", "goblin", "goblin"], BackgroundFile = "Forest.png" })
        .WithItems(potion, _sword)
        .WithDropTables(new DropTable("goblin", [new DropEntry("potion", 100), new DropEntry("bronze_sword", 50)]))
        .WithNewGame(new NewGameDefinition
        {
            LeaderChoices = [new LeaderChoice { MemberId = "sarxian" }],
            StartingItems = [],
            StartLocation = new MapLocation("field", new Point2D(0.5, 0.5), Facing.Right),
            FieldSpriteFile = "player.png",
        })
        .Build();
}
