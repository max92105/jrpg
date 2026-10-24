using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Session;

/// <summary>
/// "camp" is a 4x4 floor with a chest at (1, 0) that gives a potion once and opens; the party starts below it facing it.
/// "field" is a 4x1 floor where every step after the first starts a goblin battle.
/// </summary>
public sealed class GameSessionSaveTests
{
    private static readonly DateTimeOffset _savedAt = new(2026, 10, 17, 20, 30, 0, TimeSpan.Zero);
    private static readonly MapLocation _belowChest = new("camp", new Point2D(1.5, 1.5), Facing.Up);

    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build();
    private static readonly ItemDefinition _mail = new ItemDefinitionBuilder().WithId("chain_mail").AsArmor(3).Build();
    private static readonly ObjectDefinition _chest = new() { Id = "chest", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };

    private static readonly MapEvent _openChest = new MapEventBuilder().WithId("chest_0").AsOnce()
        .WithSteps(new GiveItemStep("potion", 1), new SwapObjectStep("chest_opened"))
        .Build();

    [Fact]
    public void CaptureThenRestore_RestoresLevelsExperienceAndVitals()
    {
        var session = StartNew();
        var leader = session.Party.Leader;
        leader.GainExperience(150, new ExperienceTable([100, 200]));
        leader.Vitals.TakeDamage(7);
        leader.Vitals.TrySpendMp(3);

        var restored = SaveAndRestore(session).Party.Leader;

        Assert.Equal((2, 50), (restored.Level, restored.Experience));
        Assert.Equal((leader.Vitals.Hp, leader.Vitals.Mp), (restored.Vitals.Hp, restored.Vitals.Mp));
        Assert.Equal((leader.Vitals.MaxHp, leader.Vitals.MaxMp), (restored.Vitals.MaxHp, restored.Vitals.MaxMp));
    }

    [Fact]
    public void CaptureThenRestore_RecruitedDuringPlay_KeepsThem()
    {
        var session = StartNew();
        var catalog = Catalog(_belowChest);
        var jazog = catalog.PartyMembers["jazog"];
        session.Party.Recruit(PartyMember.Recruit(jazog, catalog.ClassOf(jazog), catalog.Skills));

        var restored = SaveAndRestore(session).Party;

        Assert.Equal(["sarxian", "jazog"], restored.ActiveMembers.Select(member => member.Id));
    }

    // A save keeps only level, experience, HP and MP, so one from before classes had stats loads as is (ADR 0017).
    [Fact]
    public void Restore_MemberSavedAtLevelThree_HasTheClassStatsAtThree()
    {
        var growing = new ClassBuilder()
            .WithBase(new ClassStats(MaxHp: 100, MaxMp: 20, Attack: 10, Defense: 5))
            .WithGrowth(new ClassStats(MaxHp: 10, MaxMp: 2, Attack: 3, Defense: 1))
            .Build();
        var catalog = Catalog(_belowChest) with { Classes = new Dictionary<string, ClassDefinition> { ["warrior"] = growing } };
        var save = StartNew().Capture(_savedAt);
        var savedAtThree = save with { Members = [save.Members[0] with { Level = 3, Hp = 90, Mp = 12 }] };

        var leader = GameSession.Restore(savedAtThree, catalog, new FixedSequenceRandom()).Party.Leader;

        Assert.Equal(new CombatStats(Attack: 16, Defense: 7), leader.BaseStats);
        Assert.Equal((90, 120, 12, 24), (leader.Vitals.Hp, leader.Vitals.MaxHp, leader.Vitals.Mp, leader.Vitals.MaxMp));
    }

    [Fact]
    public void CaptureThenRestore_EquippedItemsAreWornNotInInventory()
    {
        var session = StartNew();
        session.Equipment.TryEquip(session.Party.Leader, _sword);
        session.Equipment.TryEquip(session.Party.Leader, _mail);

        var restored = SaveAndRestore(session);

        Assert.Equal((_sword, _mail), (restored.Party.Leader.Weapon, restored.Party.Leader.Armor));
        Assert.Equal((0, 0), (restored.Inventory.QuantityOf("bronze_sword"), restored.Inventory.QuantityOf("chain_mail")));
        Assert.Equal(3, restored.Inventory.QuantityOf("potion"));
    }

    [Fact]
    public void CaptureThenRestore_ActiveOrderFollowsSavedIds()
    {
        var session = StartWithRecruits("jazog", "teelle");
        session.Party.TryAddToActive(session.Party.MemberById("teelle"));
        session.Party.TryAddToActive(session.Party.MemberById("jazog"));

        var restored = SaveAndRestore(session);

        Assert.Equal(["sarxian", "teelle", "jazog"], restored.Party.ActiveMembers.Select(member => member.Id));
        Assert.Equal(["sarxian", "jazog", "teelle"], restored.Party.Members.Select(member => member.Id));
    }

    [Fact]
    public void CaptureThenRestore_OpenedChestStaysOpenAndDone()
    {
        var session = StartNew();
        session.Exploration.TryInteract();
        session.Exploration.ContinueEvent();

        var save = session.Capture(_savedAt);
        var restored = GameSession.Restore(save, Catalog(_belowChest), new FixedSequenceRandom());

        Assert.Equal([new MapEventKey("camp", "chest_0")], save.CompletedEvents);
        Assert.Equal([new ObjectSwap("camp", "chest@1,0", "chest_opened")], save.ObjectSwaps);
        Assert.False(restored.Exploration.TryInteract());
        Assert.Equal(4, restored.Inventory.QuantityOf("potion"));
    }

    [Fact]
    public void CaptureThenRestore_PlayerAtSavedLocation()
    {
        var session = StartNew();
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0.5), TimeSpan.FromSeconds(0.2));

        var restored = SaveAndRestore(session);

        Assert.Equal(session.Exploration.Location, restored.Exploration.Location);
    }

    [Fact]
    public void CaptureThenRestore_KeepsPlayTime()
    {
        var session = StartNew();
        session.AddPlayTime(TimeSpan.FromMinutes(42));

        var restored = SaveAndRestore(session);

        Assert.Equal(TimeSpan.FromMinutes(42), restored.PlayTime);
    }

    [Fact]
    public void Capture_SavedAt_IsTheGivenTime()
    {
        var save = StartNew().Capture(_savedAt);

        Assert.Equal(_savedAt, save.SavedAt);
    }

    [Fact]
    public void AddPlayTime_TwoDeltas_SumsThem()
    {
        var session = StartNew();

        session.AddPlayTime(TimeSpan.FromSeconds(1.5));
        session.AddPlayTime(TimeSpan.FromSeconds(0.25));

        Assert.Equal(TimeSpan.FromSeconds(1.75), session.PlayTime);
    }

    [Fact]
    public void AddPlayTime_NegativeDelta_Throws()
    {
        var session = StartNew();

        Assert.Throws<ArgumentOutOfRangeException>(() => session.AddPlayTime(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void Capture_WhileEventRunning_Throws()
    {
        var session = StartNew();
        session.Exploration.TryInteract();

        Assert.Throws<InvalidOperationException>(() => session.Capture(_savedAt));
    }

    [Fact]
    public void Capture_WhileEncounterPending_Throws()
    {
        var session = GameSession.StartNew(
            Catalog(new MapLocation("field", new Point2D(0.5, 0.5), Facing.Right)), "sarxian", new FixedSequenceRandom(0, 0));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        Assert.NotNull(session.Exploration.PendingEncounter);

        Assert.Throws<InvalidOperationException>(() => session.Capture(_savedAt));
    }

    private static GameSession StartNew() => GameSession.StartNew(Catalog(_belowChest), "sarxian", new FixedSequenceRandom());

    // Recruiting happens only through a save for now: the new game starts with its leader alone (decision 163).
    private static GameSession StartWithRecruits(params string[] memberIds)
    {
        var catalog = Catalog(_belowChest);
        var save = StartNew().Capture(_savedAt);
        var recruits = memberIds
            .Select(id => catalog.PartyMembers[id])
            .Select(definition => PartyMember.Recruit(definition, catalog.ClassOf(definition), catalog.Skills).CaptureProgress());
        return GameSession.Restore(save with { Members = [.. save.Members, .. recruits] }, catalog, new FixedSequenceRandom());
    }

    private static GameSession SaveAndRestore(GameSession session) =>
        GameSession.Restore(session.Capture(_savedAt), Catalog(_belowChest), new FixedSequenceRandom());

    private static ContentCatalog Catalog(MapLocation start)
    {
        var camp = new MapLayoutBuilder().WithLayer("1111", "1111", "1111", "1111").Build() with
        {
            Id = "camp",
            Objects = [new PlacedObject { Uid = "chest@1,0", ObjectId = "chest", Position = new Point2D(1, 0), Z = MapLayers.PlayerZ, Events = [_openChest] }],
        };
        var field = new MapLayoutBuilder().WithLayer("1111").Build() with { Id = "field" };
        var newGame = new NewGameDefinition
        {
            LeaderChoices = [new LeaderChoice { MemberId = "sarxian" }],
            StartingItems = [new InventoryEntry("potion", 3), new InventoryEntry("bronze_sword", 1), new InventoryEntry("chain_mail", 1)],
            StartLocation = start,
            FieldSpriteFile = "player.png",
        };
        return new ContentCatalogBuilder()
            .WithMembers(
                new PartyMemberBuilder().WithId("sarxian").BuildDefinition(),
                new PartyMemberBuilder().WithId("jazog").BuildDefinition(),
                new PartyMemberBuilder().WithId("teelle").BuildDefinition())
            .WithItems(_potion, _sword, _mail)
            .WithObjects(_chest, _chest with { Id = "chest_opened" })
            .WithMaps(camp, field)
            .WithRandomEncounters("field", new RandomEncounterTable { OneInSteps = 1, GraceSteps = 1, EncounterIds = ["goblin_trio"] })
            .WithEncounters(new EncounterDefinition { Id = "goblin_trio", EnemyIds = ["goblin"], BackgroundFile = "Forest.png" })
            .WithNewGame(newGame)
            .Build();
    }
}
