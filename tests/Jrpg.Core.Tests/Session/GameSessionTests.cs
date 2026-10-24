using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Session;

public sealed class GameSessionTests
{
    private static readonly SkillDefinition _heal = new()
    {
        Id = "heal",
        Name = "Heal",
        Description = "",
        IconFile = "",
        MpCost = 8,
        Effect = new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly),
    };

    private readonly ContentCatalogBuilder _catalog = new ContentCatalogBuilder()
        .WithSkills(_heal)
        .WithMembers(
            new PartyMemberBuilder().WithId("sarxian").WithSkills(_heal).BuildDefinition(),
            new PartyMemberBuilder().WithId("jazog").BuildDefinition(),
            new PartyMemberBuilder().WithId("teelle").BuildDefinition());

    [Fact]
    public void StartNew_ChosenLeader_LeadsAlone()
    {
        var catalog = _catalog.WithNewGame(NewGame()).Build();

        var session = GameSession.StartNew(catalog, "jazog", new SeededRandom(0));

        Assert.Equal("jazog", session.Party.Leader.Id);
        Assert.Equal(["jazog"], session.Party.Members.Select(member => member.Id));
        Assert.Equal(["jazog"], session.Party.ActiveMembers.Select(member => member.Id));
    }

    [Fact]
    public void StartNew_LeaderNotAmongChoices_Throws()
    {
        var catalog = _catalog.WithNewGame(NewGame() with { LeaderChoices = [Choice("sarxian")] }).Build();

        var exception = Assert.Throws<ArgumentException>(() => GameSession.StartNew(catalog, "teelle", new SeededRandom(0)));

        Assert.Contains("'teelle'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StartNew_StartingItems_FillInventory()
    {
        var potion = new ItemDefinitionBuilder().WithId("potion").Build();
        var bomb = new ItemDefinitionBuilder().WithId("bomb").Build();
        var catalog = _catalog
            .WithItems(potion, bomb)
            .WithNewGame(NewGame(new InventoryEntry("potion", 5), new InventoryEntry("bomb", 3)))
            .Build();

        var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(0));

        Assert.Equal([new InventoryEntry("potion", 5), new InventoryEntry("bomb", 3)], session.Inventory.Entries);
    }

    [Fact]
    public void StartNew_MemberWithSkillIds_KnowsTheResolvedSkills()
    {
        var catalog = _catalog.WithNewGame(NewGame()).Build();

        var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(0));

        Assert.Equal([_heal], session.Party.Leader.Skills);
    }

    [Fact]
    public void StartNew_Equipment_UsesTheSessionInventory()
    {
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build();
        var catalog = _catalog
            .WithItems(sword)
            .WithNewGame(NewGame(new InventoryEntry("bronze_sword", 1)))
            .Build();
        var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(0));

        var equipped = session.Equipment.TryEquip(session.Party.Leader, sword);

        Assert.True(equipped);
        Assert.Equal(0, session.Inventory.QuantityOf(sword.Id));
    }

    private static NewGameDefinition NewGame(params InventoryEntry[] items) =>
        new() { LeaderChoices = [Choice("sarxian"), Choice("jazog"), Choice("teelle")], StartingItems = items, StartLocation = ContentCatalogBuilder.DefaultStart, FieldSpriteFile = "player.png" };

    private static LeaderChoice Choice(string memberId) => new() { MemberId = memberId };
}
