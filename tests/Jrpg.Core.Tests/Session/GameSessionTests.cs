using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
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
    public void StartNew_ActiveMembers_FollowDefinitionOrder()
    {
        var catalog = _catalog.WithNewGame(NewGame(recruited: ["sarxian", "jazog", "teelle"], active: ["teelle", "sarxian"])).Build();

        var session = GameSession.StartNew(catalog);

        Assert.Equal(["teelle", "sarxian"], session.Party.ActiveMembers.Select(member => member.Id));
        Assert.Equal("teelle", session.Party.Leader.Id);
        Assert.Equal(["sarxian", "jazog", "teelle"], session.Party.Members.Select(member => member.Id));
    }

    [Fact]
    public void StartNew_StartingItems_FillInventory()
    {
        var potion = new ItemDefinitionBuilder().WithId("potion").Build();
        var bomb = new ItemDefinitionBuilder().WithId("bomb").Build();
        var catalog = _catalog
            .WithItems(potion, bomb)
            .WithNewGame(NewGame(recruited: ["sarxian"], active: ["sarxian"], new InventoryEntry("potion", 5), new InventoryEntry("bomb", 3)))
            .Build();

        var session = GameSession.StartNew(catalog);

        Assert.Equal([new InventoryEntry("potion", 5), new InventoryEntry("bomb", 3)], session.Inventory.Entries);
    }

    [Fact]
    public void StartNew_MemberWithSkillIds_KnowsTheResolvedSkills()
    {
        var catalog = _catalog.WithNewGame(NewGame(recruited: ["sarxian"], active: ["sarxian"])).Build();

        var session = GameSession.StartNew(catalog);

        Assert.Equal([_heal], session.Party.Leader.Skills);
    }

    [Fact]
    public void StartNew_Equipment_UsesTheSessionInventory()
    {
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5, "warrior").Build();
        var catalog = _catalog
            .WithItems(sword)
            .WithNewGame(NewGame(recruited: ["sarxian"], active: ["sarxian"], new InventoryEntry("bronze_sword", 1)))
            .Build();
        var session = GameSession.StartNew(catalog);

        var equipped = session.Equipment.TryEquip(session.Party.Leader, sword);

        Assert.True(equipped);
        Assert.Equal(0, session.Inventory.QuantityOf(sword.Id));
    }

    private static NewGameDefinition NewGame(string[] recruited, string[] active, params InventoryEntry[] items) =>
        new() { RecruitedMemberIds = recruited, ActiveMemberIds = active, StartingItems = items };
}
