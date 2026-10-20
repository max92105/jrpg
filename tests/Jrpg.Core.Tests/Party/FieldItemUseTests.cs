using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class FieldItemUseTests
{
    private readonly PartyMember _sarxian = Member("sarxian", hp: 50);
    private readonly PartyMember _jazog = Member("jazog", hp: 40);
    private readonly PartyMember _reserve = Member("reserve", hp: 30);
    private readonly Inventory _inventory = new();
    private readonly FieldItemUse _fieldItems;

    public FieldItemUseTests()
    {
        var party = new PartyState([_sarxian, _jazog, _reserve], [_sarxian, _jazog], new PartyRules(MaxActiveMembers: 4));
        _fieldItems = new FieldItemUse(party, _inventory);
    }

    [Fact]
    public void Use_RestoreHpOnLivingMember_RestoresHpAndConsumesOneCopy()
    {
        var potion = Owned(Item("potion", EffectKind.RestoreHp, 15, Targeting.SingleAlly), copies: 2);

        var amounts = _fieldItems.Use(potion, _sarxian, [_jazog]);

        Assert.Equal([15], amounts);
        Assert.Equal(55, _jazog.Vitals.Hp);
        Assert.Equal(1, _inventory.QuantityOf(potion.Id));
    }

    [Fact]
    public void Use_ItemNotUsableFromMenu_Throws()
    {
        var bomb = Owned(Item("bomb", EffectKind.Damage, 15, Targeting.SingleEnemy, isUsableFromMenu: false), copies: 1);

        Assert.False(_fieldItems.CanUse(bomb));
        Assert.Throws<InvalidOperationException>(() => _fieldItems.Use(bomb, _sarxian, [_jazog]));
    }

    [Fact]
    public void Use_ItemNotOwned_Throws()
    {
        var potion = Item("potion", EffectKind.RestoreHp, 15, Targeting.SingleAlly);

        Assert.False(_fieldItems.CanUse(potion));
        Assert.Throws<InvalidOperationException>(() => _fieldItems.Use(potion, _sarxian, [_jazog]));
    }

    [Fact]
    public void Use_FallenAllyItemOnLivingMember_ThrowsAndKeepsTheCopy()
    {
        // Someone is fallen, so the feather can be used; just not on a living member.
        _reserve.Vitals.TakeDamage(1000);
        var feather = Owned(Item("revive_feather", EffectKind.Revive, 15, Targeting.SingleFallenAlly), copies: 1);

        Assert.Throws<ArgumentException>(() => _fieldItems.Use(feather, _sarxian, [_jazog]));

        Assert.Equal(1, _inventory.QuantityOf(feather.Id));
    }

    [Fact]
    public void Use_SelfItemOnOtherMember_ThrowsAndChangesNothing()
    {
        var tonic = Owned(Item("tonic", EffectKind.RestoreHp, 10, Targeting.Self), copies: 1);

        Assert.Throws<ArgumentException>(() => _fieldItems.Use(tonic, _sarxian, [_jazog]));

        Assert.Equal(40, _jazog.Vitals.Hp);
        Assert.Equal(1, _inventory.QuantityOf(tonic.Id));
    }

    [Fact]
    public void CanUse_MenuUsableItemWithoutEffect_ReturnsFalse()
    {
        var trinket = Owned(new ItemDefinitionBuilder().WithId("trinket").Build() with { IsUsableFromMenu = true }, copies: 1);

        var canUse = _fieldItems.CanUse(trinket);

        Assert.False(canUse);
    }

    [Fact]
    public void Use_SelfItemOnUser_Heals()
    {
        var tonic = Owned(Item("tonic", EffectKind.RestoreHp, 10, Targeting.Self), copies: 1);

        var amounts = _fieldItems.Use(tonic, _sarxian, [_sarxian]);

        Assert.Equal([10], amounts);
    }

    [Fact]
    public void Use_AllAlliesItemMissingTheReserveMember_Throws()
    {
        var mist = Owned(Item("healing_mist", EffectKind.RestoreHp, 10, Targeting.AllAllies), copies: 1);

        Assert.Throws<ArgumentException>(() => _fieldItems.Use(mist, _sarxian, [_sarxian, _jazog]));
    }

    [Fact]
    public void Use_AllAlliesItem_HealsEveryRecruitedMemberIncludingTheReserve()
    {
        var mist = Owned(Item("healing_mist", EffectKind.RestoreHp, 10, Targeting.AllAllies), copies: 1);

        var amounts = _fieldItems.Use(mist, _sarxian, [_sarxian, _jazog, _reserve]);

        Assert.Equal([10, 10, 10], amounts);
        Assert.Equal(40, _reserve.Vitals.Hp);
    }

    [Fact]
    public void Use_ReviveItemOnFallenReserveMember_RevivesThem()
    {
        var feather = Owned(Item("revive_feather", EffectKind.Revive, 15, Targeting.SingleFallenAlly), copies: 1);
        _reserve.Vitals.TakeDamage(999);

        var amounts = _fieldItems.Use(feather, _sarxian, [_reserve]);

        Assert.Equal([15], amounts);
        Assert.True(_reserve.Vitals.IsAlive);
    }

    [Fact]
    public void Use_UserNotRecruited_Throws()
    {
        var stranger = Member("stranger", hp: 100);
        var potion = Owned(Item("potion", EffectKind.RestoreHp, 15, Targeting.SingleAlly), copies: 1);

        Assert.Throws<ArgumentException>(() => _fieldItems.Use(potion, stranger, [_jazog]));
    }

    [Fact]
    public void TargetChoiceFor_HealItem_ListsLivingMembersIncludingReserve()
    {
        _jazog.Vitals.TakeDamage(1000);
        var potion = Item("potion", EffectKind.RestoreHp, 15, Targeting.SingleAlly);

        var choice = _fieldItems.TargetChoiceFor(potion, _sarxian);

        Assert.Equal([_sarxian, _reserve], choice.Candidates);
        Assert.False(choice.TakesAll);
    }

    [Fact]
    public void TargetChoiceFor_ReviveItem_ListsOnlyFallen()
    {
        _reserve.Vitals.TakeDamage(1000);
        var feather = Item("revive_feather", EffectKind.Revive, 15, Targeting.SingleFallenAlly);

        var choice = _fieldItems.TargetChoiceFor(feather, _sarxian);

        Assert.Equal([_reserve], choice.Candidates);
    }

    [Fact]
    public void TargetChoiceFor_AllAlliesItem_TakesAll()
    {
        var tent = Item("tent", EffectKind.RestoreHp, 30, Targeting.AllAllies);

        var choice = _fieldItems.TargetChoiceFor(tent, _sarxian);

        Assert.True(choice.TakesAll);
        Assert.Equal([_sarxian, _jazog, _reserve], choice.Candidates);
    }

    [Fact]
    public void TargetChoiceFor_SelfItem_ListsOnlyUser()
    {
        var tonic = Item("tonic", EffectKind.RestoreHp, 10, Targeting.Self);

        var choice = _fieldItems.TargetChoiceFor(tonic, _jazog);

        Assert.Equal([_jazog], choice.Candidates);
    }

    [Fact]
    public void CanUse_ReviveWithNobodyFallen_ReturnsFalse()
    {
        var feather = Owned(Item("revive_feather", EffectKind.Revive, 15, Targeting.SingleFallenAlly), copies: 1);

        Assert.False(_fieldItems.CanUse(feather));
    }

    [Fact]
    public void CanUse_ReviveWithSomeoneFallen_ReturnsTrue()
    {
        _jazog.Vitals.TakeDamage(1000);
        var feather = Owned(Item("revive_feather", EffectKind.Revive, 15, Targeting.SingleFallenAlly), copies: 1);

        Assert.True(_fieldItems.CanUse(feather));
    }

    [Fact]
    public void TargetChoiceFor_ItemWithoutEffect_Throws()
    {
        var trinket = new ItemDefinitionBuilder().WithId("trinket").Build();

        Assert.Throws<ArgumentException>(() => _fieldItems.TargetChoiceFor(trinket, _sarxian));
    }

    private static PartyMember Member(string id, int hp) =>
        new PartyMemberBuilder().WithId(id).WithVitals(hp, maxHp: 100, mp: 0, maxMp: 0).Build();

    private static ItemDefinition Item(string id, EffectKind kind, int power, Targeting targeting, bool isUsableFromMenu = true) =>
        new ItemDefinitionBuilder().WithId(id).WithEffect(new Effect(kind, power, targeting)).Build() with { IsUsableFromMenu = isUsableFromMenu };

    private ItemDefinition Owned(ItemDefinition item, int copies)
    {
        _inventory.Add(item, copies);
        return item;
    }
}
